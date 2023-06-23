using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FrankieBot.Discord.Services
{
    ///<summary>
    ///A wrapper around a DiscordSocketClient
    ///so it inherits a testable interface that we use.
    ///</summary>
    public class DiscordSocketClientWrapper : IDiscordClientService
    {
        private DiscordSocketClient _client;

        public DiscordSocketClient Client 
        {
            get => _client;
        }

        public DiscordSocketClientWrapper(DiscordSocketConfig config)
        {
            _client = new DiscordSocketClient(config);
            _client.Ready += OnReady;

        }

        public SocketSelfUser CurrentUser => _client.CurrentUser;

        public async Task Login() {
            await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("FRANKIE_TOKEN"));
        }

        public async Task StartAsync()
        {
            await _client.StartAsync();
        }

        public SocketGuild GetGuild(ulong guildID)
        {
            return _client.GetGuild(guildID);
        }

        public event Func<Task> Ready;

        public event Func<SocketMessage, Task> MessageReceived;

        private async Task OnReady()
        {
            await Ready?.Invoke();
        }

        private async Task OnMessageReceived(SocketMessage sourceMessage)
        {
            await MessageReceived?.Invoke(sourceMessage);
        }
    }
}
