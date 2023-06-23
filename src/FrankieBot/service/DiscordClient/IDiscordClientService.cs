using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FrankieBot.Discord.Services.DiscordClient
{
    /// <summary>
    /// Interface that defines the service that mediates between the bot and Discord.
    /// This will replace DiscordSocketClient from the Discord.NET library.
    /// </summary>
    public interface IDiscordClientService
    {
        /// <summary>
        /// Get the current user. I believe this is represents
        /// the bot itself?
        /// </summary>
        /// <returns>
        /// The current user
        /// </returns>
        ISelfUser CurrentUser { get; }

        /// <summary>
        /// Authenticate/connect the bot.
        /// </summary>
        Task Login();

        /// <summary>
        /// Starts the bot once it's been connected.
        /// </summary>
        /// <remarks>
        /// Implementation might be able to bundle the 
        /// authentication -> starting steps?
        /// </remarks>
        Task StartAsync();

        ///<summary>
        ///Gets the guild that corresponds to a certain guildID
        ///</summary>
        ///<param name="guildID"></param>
        ///<returns>
        ///The guild (server) in the form of an IGuild.
        ///</returns>
        IGuild GetGuild(ulong guildID);

        /// <summary>
        /// Fires when guild data has finished downloading.
        /// </summary>
        event Func<Task> Ready;

        /// <summary>
        /// Fires when a message is recieved.
        /// </summary>
        /// <remarks>
        /// I'm not sure if we want to use SocketMessage
        /// but right now I am verbatim just copying anything that is used
        /// from DiscordSocketClient.
        /// </remarks>
        event Func<IMessage, Task> MessageReceived;

        /// <summary>
        /// Fires when a log message is generated.
        /// </summary>
        event Func<LogMessage, Task> Log;
    }
}
