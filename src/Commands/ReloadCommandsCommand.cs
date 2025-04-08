// Mugs/Commands/ReloadCommandsCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class ReloadCommandsCommand : ICommand
    {
        private readonly CommandManager _manager;

        public ReloadCommandsCommand(CommandManager manager) => _manager = manager;
        public string Name => "reload";
        public string Description => LocalizationService.GetString("reload_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => null;

        public async Task ExecuteAsync(string[] args)
        {
            OutputService.WriteResponse("reloading_commands");
            MetadataCacheService.Clear();
            await _manager.LoadCommandsAsync();
            OutputService.WriteResponse(LocalizationService.GetString("commands_reloaded") + "\n" + LocalizationService.GetString("cache_cleared"));
        }
    }
}