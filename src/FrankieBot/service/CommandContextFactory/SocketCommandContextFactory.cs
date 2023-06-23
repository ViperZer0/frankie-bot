using System;
using Discord.Commands;
using Discord;
using FrankieBot.Discord.Services.DiscordClient;

namespace FrankieBot.Discord.Services.CommandContextFactory
{
    ///<summary>
    ///This version of the factory is for live/production environment.
    ///Expects a DiscordSocketClientWrapper and creates a CommandContext
    ///</summary>
    ///<inheritdoc cref="ICommandContextFactory"/>
    public class SocketCommandContextFactory : ICommandContextFactory
    {
        ///<exception cref="System.InvalidOperationException">
        ///Thrown when client is not a DiscordSocketClientWrapper
        ///</exception>
        public ICommandContext CreateContext(IDiscordClientService client, IUserMessage message)
        {
            if(client is DiscordSocketClientWrapper socketClient)
            {
                return new CommandContext(socketClient.Client, message);
            }
            else
            {
                /* This should mean that the ICommandContextFactory
                 * services was a SocketCommandContextFactory (production/live)
                 * but the client was NOT a DiscordSOcketClientWrapper (i.e testing environment
                 */
                throw new InvalidOperationException("Attempted to create a context requiring a DiscordSocketClient but client was not one.");
            }
        }
    }
}
