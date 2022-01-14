using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;
using Discord.Commands;

using SQLite;
using Cronos;

using FrankieBot.DB;
using FrankieBot.DB.ViewModel;
using FrankieBot.DB.Container;
using FrankieBot.Discord.Services;
using FrankieBot.Discord;

namespace FrankieBot.Discord.Modules
{
	/// <summary>
	/// Module responsible for handling word tracking and daily word goals
	/// </summary>
	[Group("wordtracker")]
	[Alias("wt")]
	public class WordTrackerModule : ModuleBase<SocketCommandContext>
	{
		#region Options

		/// <summary>
		/// Option title for option which enables the Word Tracker module.
		/// </summary>
		public const string OptionEnabled = "word_tracker_module_enabled";

		/// <summary>
		/// Option title for option which sets the announcement channel
		/// </summary>
		public const string OptionAnnounceChannel = "word_tracker_announce_channel";

		/// <summary>
		/// Option title for option which sets the channel reports must be submitted in
		/// </summary>
		public const string OptionReportChannel = "word_tracker_report_channel";

		/// <summary>
		/// Option title for option which sets the random word count goal minimum
		/// </summary>
		public const string OptionGoalMinimum = "word_tracker_goal_minimum";

		/// <summary>
		/// /// Option title for option which sets the random word count goal maximum
		/// </summary>
		public const string OptionGoalMaximum = "word_tracker_goal_maximum";

		#endregion // Options

		#region Jobs

		/// <summary>
		/// Job title for the daily wordtracker refresh
		/// </summary>
		public const string JobRefresh = "word_tracker_refresh";

		#endregion // Jobs

		/// <summary>
		/// This module's DataBaseService reference
		/// </summary>
		/// <value></value>
		public DataBaseService DataBaseService { get; set; }

		/// <summary>
		/// This module's SchedulerService reference
		/// </summary>
		/// <value></value>
		public SchedulerService SchedulerService { get; set; }

		/// <summary>
		/// Checks and ensures that correct jobs are running
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="dataBaseService"></param>
		/// <param name="schedulerService"></param>
		/// <returns></returns>
		protected static async Task RebuildJobs(IGuild guild, DataBaseService dataBaseService, SchedulerService schedulerService)
		{
			Dictionary<string, string> options = null;
			await dataBaseService.RunGuildDBAction(guild, connection =>
			{
				options = Option.FindAll(connection).As<Options, Option>().Get();
			});

			bool enabled = false;
			if (options.TryGetValue(OptionEnabled, out string value))
			{
				enabled = bool.Parse(value);
			}

			if (enabled)
			{
				// find and stop refresh job if exists
				var refreshJob = schedulerService.GetJob(guild, JobRefresh);
				if (refreshJob != null)
				{
					schedulerService.RemoveJob(refreshJob);
				}

				// build/run refresh job
				await dataBaseService.RunGuildDBAction(guild, connection =>
				{
					refreshJob = new CronJob(connection)
					{
						Name = JobRefresh,
						Guild = guild,
						CronString = "0 0 * * *"
					};
				});

				await schedulerService.AddJob(refreshJob);
				refreshJob.Run += async (object sender, EventArgs e) =>
				{
					await RefreshTracker(guild, dataBaseService, schedulerService);
				};
			}
			else
			{
				// Module is disabled. Find any related jobs and remove them
				var refreshJob = schedulerService.GetJob(guild, JobRefresh);
				if (refreshJob != null)
				{
					schedulerService.RemoveJob(refreshJob);
				}
			}
		}

		private static async Task RefreshTracker(IGuild guild, DataBaseService dataBaseService, SchedulerService schedulerService)
		{
			List<WTSubscriber> subscribers = null;
			Option announceChannelOption = null;

			List<(IUser User, int Goal, int Progress)> todaysGoals = new List<(IUser User, int Goal, int Progress)>();
			List<(IUser User, int Goal, int Progress)> tomorrowsGoals = new List<(IUser User, int Goal, int Progress)>();

			var rand = new Random();

			await dataBaseService.RunGuildDBAction(guild, connection =>
			{
				subscribers = WTSubscriber.FindAll(connection).ContentAs<WTSubscriber>().Content;
				announceChannelOption = Option.FindOne(connection, o => o.Name == OptionAnnounceChannel).As<Option>();
				var minOption = Option.FindOne(connection, o => o.Name == OptionGoalMinimum).As<Option>();
				var maxOption = Option.FindOne(connection, o => o.Name == OptionGoalMaximum).As<Option>();

				if (minOption.IsEmpty)
				{
					minOption = new Option(connection)
					{
						Name = OptionGoalMinimum
					};
					minOption.Initialize();
					minOption.Save();
				}

				if (maxOption.IsEmpty)
				{
					maxOption = new Option(connection)
					{
						Name = OptionGoalMaximum
					};
					maxOption.Initialize();
					maxOption.Save();
				}

				var min = int.Parse(minOption.Value);
				var max = int.Parse(maxOption.Value);

				foreach (var sub in subscribers)
				{
					sub.Initialize(guild);

					todaysGoals.Add((sub.User, sub.WordCountGoal, sub.WordCountProgress));

					sub.WordCountProgress = 0;
					sub.WordCountGoal = sub.HasCustomGoal ?
						sub.CustomGoal :
						rand.Next(min, max + 1);

					sub.Save();

					tomorrowsGoals.Add((sub.User, sub.WordCountGoal, sub.WordCountProgress));
				}
			});

			if (announceChannelOption != null && !announceChannelOption.IsEmpty)
			{
				var guildChannel = await guild.GetChannelAsync(ulong.Parse(announceChannelOption.Value)) as SocketGuildChannel;
				if (guildChannel == null)
				{
					// todo: log "channel not found" error
					return;
				}

				if (guildChannel is ISocketMessageChannel channel)
				{
					// build embed for previous day's results & post
					if (todaysGoals.Count > 0)
					{
						var eb = new EmbedBuilder()
							.WithTitle("Yesterday's Word Tracker Results");

						var fields = new List<EmbedFieldBuilder>();

						foreach (var res in todaysGoals)
						{
							fields.Add(new EmbedFieldBuilder()
								.WithName(res.User.Username)
								.WithValue($"{res.Progress} / {res.Goal}"));
						}

						eb.WithFields(fields);
						await channel.SendMessageAsync(embed: eb.Build());
					}

					// build embed for new day's goals & post
					if (tomorrowsGoals.Count > 0)
					{
						var eb = new EmbedBuilder()
							.WithTitle("Today's Word Tracker Goals");

						var fields = new List<EmbedFieldBuilder>();

						foreach (var res in tomorrowsGoals)
						{
							fields.Add(new EmbedFieldBuilder()
								.WithName(res.User.Username)
								.WithValue($"{res.Progress} / {res.Goal}"));
						}

						eb.WithFields(fields);
						await channel.SendMessageAsync(embed: eb.Build());
					}
				}
			}
		}

		/// <summary>
		/// Enables the Word Tracker module
		/// </summary>
		/// <returns></returns>
		[Command("enable")]
		[Alias("on", "true")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task EnableModule()
		{

		}

		/// <summary>
		/// Disables the Word Tracker module
		/// </summary>
		/// <returns></returns>
		[Command("disable")]
		[Alias("off", "false")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task DisableModule()
		{

		}

		/// <summary>
		/// Subscribes the user to daily word goals
		/// </summary>
		/// <returns></returns>
		[Command("subscribe")]
		[Alias("sub")]
		public async Task Subscribe()
		{

		}

		/// <summary>
		/// Unsubscribes the user from daily word goals
		/// </summary>
		/// <returns></returns>
		[Command("unsubscribe")]
		[Alias("unsub")]
		public async Task Unsubscribe()
		{

		}

		/// <summary>
		/// Removes non-present guild members from the subscriber list
		/// </summary>
		/// <returns></returns>
		[Command("scrubsubscribers")]
		[Alias("scrub")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public async Task ScrubSubscribers()
		{

		}

		/// <summary>
		/// Adds to the user's daily word tracker goal progress
		/// </summary>
		/// <param name="words"></param>
		/// <returns></returns>
		[Command("add")]
		[Alias("a")]
		public async Task AddToWordCount(int words)
		{

		}

		/// <summary>
		/// Edits the user's daily word tracker goal progress
		/// </summary>
		/// <param name="words"></param>
		/// <returns></returns>
		[Command("edit")]
		[Alias("e")]
		public async Task EditWordCount(int words)
		{

		}

		/// <summary>
		/// Lists current subscribers as well as their goals and progress
		/// </summary>
		/// <returns></returns>
		[Command("list")]
		[Alias("ls")]
		public async Task ListWordCounts()
		{

		}

		/// <summary>
		/// Command module containing commands for setting WordTrackerModule options
		/// </summary>
		[Group("option")]
		[Alias("set", "o")]
		[RequireUserPermission(GuildPermission.Administrator)]
		public class ModuleOptions : ModuleBase<SocketCommandContext>
		{
			/// <summary>
			/// This module's DataBaseService reference
			/// </summary>
			public DataBaseService DataBaseService;

			/// <summary>
			/// This module's SchedulerService reference
			/// </summary>
			public SchedulerService SchedulerService;

			[Command("announcechannel")]
			[Alias("announce")]
			public async Task SetAnnounceChannel(IChannel channel)
			{

			}

			[Command("reportchannel")]
			[Alias("report")]
			public async Task SetReportChannel(IChannel channel)
			{

			}
		}
	}
}