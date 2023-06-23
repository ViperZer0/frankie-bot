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
        Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services);

        /// <summary>
        /// Executes the command.
        /// </summary>
        Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services);

        /// <summary>
        /// This event is fired when a command
        /// has been executed, successfully or not.
        /// Replaces the event of the same name from CommandService.
        /// </summary>
        event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted;
    }
}

