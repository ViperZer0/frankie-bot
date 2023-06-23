using FrankieBot.Discord.Services.DiscordClient;
using Discord.Commands;
using Discord;

namespace FrankieBot.Discord.Services.CommandContextFactory
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
        /// <param name="client">The Discord client to create the context for</param>
        /// <param name="message">The message to get the context of</param>
        ICommandContext CreateContext(IDiscordClientService client, IUserMessage message);
    }
}
