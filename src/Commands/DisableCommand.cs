// Mugs/Commands/DisableCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class DisableCommand : ICommand
    {
        private readonly CommandManager _manager;
        private readonly string _extensionsPath;
        private readonly ExtensionManager _extensionManager;

        public DisableCommand(CommandManager manager, string extensionsPath)
        {
            _manager = manager;
            _extensionsPath = extensionsPath;
            _extensionManager = new ExtensionManager(_extensionsPath);
        }

        public string Name => "disable";
        public string Description => LocalizationService.GetString("disable_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "disable mycommand";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelperService.WriteError("missing_extension_name");
                return;
            }

            await _extensionManager.DisableExtensionAsync(args[0]);
            await _manager.LoadCommandsAsync();
        }
    }
}