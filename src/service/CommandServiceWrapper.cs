using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using Discord;
using Discord.Commands;

namespace FrankieBot.Discord.Services
{
    public class CommandServiceWrapper : IDiscordCommandService
    {
        private CommandService _commandService;

        public CommandServiceWrapper()
        {
            _commandService = new CommandService();
            _commandService.CommandExecuted += OnCommandExecuted;
        }

        public async Task<IEnumerable<ModuleInfo>> AddModulesAsync(Assembly assembly, IServiceProvider services)
        {
            return await _commandService.AddModulesAsync(assembly, services);
        }

        public async Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services)
        {
            return await _commandService.ExecuteAsync(context, argPos, services);
        }

        public event Func<Optional<CommandInfo>, ICommandContext, IResult, Task> CommandExecuted;

        private async Task OnCommandExecuted(Optional<CommandInfo> commandInfo, ICommandContext context, IResult result)
        {
            await CommandExecuted?.Invoke(commandInfo, context, result);
        }
    }
}
