using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FrankieBot.Discord.Services.DiscordClient
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
            _client.MessageReceived += OnMessageReceived;
            _client.Log += OnLog;

        }

        public SocketSelfUser CurrentUser => _client.CurrentUser;

        public async Task Login() {
            await OnLog(new LogMessage(LogSeverity.Debug, "Login()", $"Discord Token: {Environment.GetEnvironmentVariable("FRANKIE_TOKEN")}"));
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

        public event Func<LogMessage, Task> Log;

        private async Task OnReady()
        {
            await Ready?.Invoke();
        }

        private async Task OnMessageReceived(SocketMessage sourceMessage)
        {
            await OnLog(new LogMessage(LogSeverity.Debug, "OnMessageReceived:", $"Message: {sourceMessage.ToString()}"));
            await MessageReceived?.Invoke(sourceMessage);
        }

        private async Task OnLog(LogMessage message)
        {
            await Log?.Invoke(message);
        }
    }
}
