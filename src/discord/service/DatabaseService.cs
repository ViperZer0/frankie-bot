using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using SQLite;

using FrankieBot.DB;
using FrankieBot.DB.Model;
using VModel = FrankieBot.DB.ViewModel;

/// <summary>
/// Service responsible for handling database transactions
/// </summary>
public class DataBaseService
{
	/// <summary>
	/// Fired when a new Quote is added to the database
	/// </summary>
	public event EventHandler<QuoteEventArgs> QuoteAdded;

	private readonly IServiceProvider _services;

	private readonly DiscordSocketClient _client;

	/// <summary>
	/// Constructs a new DatabaseService
	/// </summary>
	/// <param name="services"></param>
	public DataBaseService(IServiceProvider services)
	{
		_services = services;
		_client = _services.GetRequiredService<DiscordSocketClient>();
		QuoteAdded += OnQuoteAdded;
	}

	/// <summary>
	/// Runs a generic database action defined in the action parameter
	/// </summary>
	/// <param name="context"></param>
	/// <param name="action"></param>
	/// <remarks>
	/// Performs baseline sanity checking and validation and is called by
	/// most other DB operation methods under the hood
	/// </remarks>
	public async Task RunDBAction(SocketCommandContext context, Action<SocketCommandContext> action)
	{
		// We don't want DM commands to clutter up the database, so we'll only allow commands sent from
		// non-private channels to affect the database.
		if (context.IsPrivate)
		{
			await context.Channel.SendMessageAsync("Sorry, but this action isn't permitted in private channels or DMs");
			return;
		}

		await CheckDB(context.Guild);

		await Task.Run(() => action(context));
	}

	private async Task CheckDB(SocketGuild guild)
	{
		await Task.Run(() =>
		{
			var dbFile = GetServerDBFilePath(guild);
			if (!File.Exists(dbFile))
			{
				File.Create(dbFile).Close();
				using (var connection = new DBConnection(dbFile))
				{
					connection.CreateTable<Option>();
					connection.CreateTable<Quote>();
					connection.CreateTable<Server>();
				}
			}
		});
	}

	/// <summary>
	/// Adds a new Quote to the database
	/// </summary>
	/// <param name="context"></param>
	/// <param name="user"></param>
	/// <param name="message"></param>
	/// <param name="recorder"></param>
	/// <returns></returns>
	public async Task AddQuote(SocketCommandContext context, IUser user, string message, IUser recorder)
	{
		await RunDBAction(context, (c) =>
		{
			using (var db = new DBConnection(GetServerDBFilePath(c.Guild)))
			{
				var quote = new Quote(db)
				{
					AuthorID = user.Id.ToString(),
					Content = message,
					RecorderID = recorder.Id.ToString(),
					RecordTimeSamp = DateTime.UtcNow,
					QuoteTimeStamp = DateTime.UtcNow
				};

				if (context.Message.ReferencedMessage != null)
				{
					quote.QuoteTimeStamp = context.Message.ReferencedMessage.Timestamp.UtcDateTime;
				}

				try
				{
					quote.Save();
					QuoteAdded?.Invoke(this, new QuoteEventArgs()
					{
						Quote = quote,
						Context = context
					});
				}
				catch (SQLiteException ex)
				{
					context.Channel.SendMessageAsync($"Something went wrong when recording that last quote. {ex}");
				}
			}
		});
	}

	/// <summary>
	/// Posts a Quote embed as a reply
	/// </summary>
	/// <param name="context"></param>
	/// <param name="quote"></param>
	/// <returns></returns>
	public async Task PostQuote(SocketCommandContext context, VModel.Quote quote)
	{
		var eb = new EmbedBuilder()
			.WithAuthor(quote.User)
			.WithDescription(quote.Content)
			.WithTimestamp(new DateTimeOffset(quote.QuoteTimeStamp.ToUniversalTime()));

		await context.Channel.SendMessageAsync(embed: eb.Build());
	}

	/// <summary>
	/// Gets a quote from the DB by ID
	/// </summary>
	/// <param name="context"></param>
	/// <param name="id"></param>
	/// <returns></returns>
	public async Task<VModel.Quote> GetQuote(SocketCommandContext context, ulong id)
	{
		return await Task.Run(() =>
		{
			Quote quote = null;
			using (var connection = new DBConnection(GetServerDBFilePath(context.Guild)))
			{
				quote = connection.Find<Quote>(id);
			}
			return new VModel.Quote(context.Guild, quote);
		});
	}

	private string GetServerDBFilePath(ulong guildId)
	{
		return Path.Combine(DBConfig.SERVER_DATA_ROOT, guildId.ToString() + DBConfig.DATABASE_FILE_EXTENSION);
	}

	private string GetServerDBFilePath(SocketGuild guild) => GetServerDBFilePath(guild.Id);

	private async void OnQuoteAdded(object sender, QuoteEventArgs q)
	{
		await q.Context.Channel.SendMessageAsync("New quote added!");
		await PostQuote(q.Context, new VModel.Quote(q.Context.Guild, q.Quote));
	}
}