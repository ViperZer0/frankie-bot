using System;
using Discord.Commands;
using Discord;

namespace FrankieBot.Discord.Services
{
    public class SocketCommandContextFactory : ICommandContextFactory
    {
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
