using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace FrankieBot.Discord.Services.CommandService
{
    /// <summary>
    /// Interface that defines the service that deals with commands.
    /// This will replace Discord's CommandService from the Discord.NET library.
    /// </summary>
    public interface IDiscordCommandService
    {
        /// <summary>
        /// Add command modules from an assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="services"></param>
        /// <returns>
        /// A list of ModuleInfos that represents each command module
        /// </returns>
        Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services);

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="argPos"></param>
        /// <param name="services"></param>
        /// <returns>
        /// An IResult that indicates whether a command was successful or not.
        /// </returns>
        Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services);

        /// <summary>
        /// This event is fired when a command
        /// has been executed, successfully or not.
        /// Replaces the event of the same name from CommandService.
        /// </summary>
        event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted;
    }
}

