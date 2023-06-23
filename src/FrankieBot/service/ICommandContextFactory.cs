using Discord.Commands;
using Discord;

namespace FrankieBot.Discord.Services
{
    /// <summary>
    /// Allows us to construct different versions
    /// of ICommandContext depending on the environment
    /// </summary>
    public interface ICommandContextFactory
    {
        /// <summary>
        /// Constructs a context,
        /// either a SocketCommandContext for a live environment,
        /// or a ? for a test environment.
        /// </summary>
        ICommandContext CreateContext(IDiscordClientService client, IUserMessage message);
    }
}
