using System;
using System.IO; using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FrankieBot.Discord.Modules;
using FrankieBot.Discord.Services;
using FrankieBot.Discord.Services.DiscordClient;
using FrankieBot.Discord.Services.CommandServiceNS;
using FrankieBot.Discord.Services.CommandContextFactory;

namespace FrankieBot.Discord
{
	/// <summary>
	/// FrankieBot's core application
	/// </summary>
	public class Bot
	{
		/// <summary>
		/// Creates a new Bot instance
		/// </summary>
		public Bot()
		{
		}

		/// <summary>
		/// Begins the Bot's execution loop
		/// </summary>
		/// <returns></returns>
		public async Task Run()
		{
			using (var services = ConfigureServices())
			{
                await Run(services);
			}
		}

        /// <summary>
        /// Runs the bot using a custom service provider 
        /// (i.e for running in a testing environment
        /// </summary>
        /// <param name="services">
        /// ServiceProvider to use.
        /// Note that caller is responsible for disposing of the provider
        /// </param>
        public async Task Run(ServiceProvider services)
        {
            var client = services.GetRequiredService<IDiscordClientService>();

            client.Ready += async () =>
            {
                await ProgressReportModule.Initialize(services);
                await WordTrackerModule.Initialize(services);
                await CurrencyModule.Initialize(services);
            };

            client.Log += async (LogMessage msg) =>
            {
                Console.WriteLine(msg.ToString());
            };

            await client.Login();

            await client.StartAsync();

            await services.GetRequiredService<CommandHandlerService>().InitializeAsync();
            await services.GetRequiredService<EavesDropperService>().InitializeAsync();

            // Block task until program close
            await Task.Delay(-1);
        }

		private async Task OnBotJoinedServer(SocketGuild server)
		{
			Console.WriteLine($"Joined Server: {server.Name}"); // test
			await Task.CompletedTask; // test
			// todo: add new server to server meta db, or set to active if already exists
		}

		private async Task OnBotLeftServer(SocketGuild server)
		{
			Console.WriteLine($"Left Server: {server.Name}"); // test
			await Task.CompletedTask; // test
			// todo: set server record in server meta db to inactive
		}

		private ServiceProvider ConfigureServices()
		{
			var config = new DiscordSocketConfig()
				{
				GatewayIntents = GatewayIntents.All,
				AlwaysDownloadUsers = true
				};

            // Wrapper so that this class implements IDiscordClientService.
			var client = new DiscordSocketClientWrapper(config);

			return new ServiceCollection()
				//.AddSingleton<DiscordSocketClient>()
				.AddSingleton<IDiscordClientService>(client)
				.AddSingleton<EavesDropperService>()
				.AddSingleton<IDiscordCommandService>(new CommandServiceWrapper())
				.AddSingleton<CommandHandlerService>()
				.AddSingleton<DataBaseService>()
				.AddSingleton<SchedulerService>()
                .AddSingleton<ICommandContextFactory>(new SocketCommandContextFactory())
				.BuildServiceProvider();
		}
	}
}
