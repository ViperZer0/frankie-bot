using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace FrankieBot.Discord.Services
{
    /// <summary>
    /// Interface that defines the service that mediates between the bot and Discord.
    /// This will replace DiscordSocketClient from the Discord.NET library.
    /// </summary>
    public interface IDiscordClientService
    {
        /// <summary>
        /// Authenticate/connect the bot.
        /// </summary>
        /// <remarks>
        /// Not sure if we need to pass the token type 
        /// or that can just be a default
        /// </remarks>
        Task LoginAsync(TokenType tokenType, string token);

        /// <summary>
        /// Starts the bot once it's been connected.
        /// </summary>
        /// <remarks>
        /// Implementation might be able to bundle the 
        /// authentication -> starting steps?
        /// </remarks>
        Task StartAsync();

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
        event Func<SocketMessage, Task> MessageRecieved;
    }
}
