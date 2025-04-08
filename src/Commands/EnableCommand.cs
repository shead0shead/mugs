// Mugs/Commands/EnableCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class EnableCommand : ICommand
    {
        private readonly CommandManager _manager;
        private readonly string _extensionsPath;
        private readonly ExtensionManager _extensionManager;

        public EnableCommand(CommandManager manager, string extensionsPath)
        {
            _manager = manager;
            _extensionsPath = extensionsPath;
            _extensionManager = new ExtensionManager(_extensionsPath);
        }

        public string Name => "enable";
        public string Description => LocalizationService.GetString("enable_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "enable mycommand";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteError("missing_extension_name");
                return;
            }

            await _extensionManager.EnableExtensionAsync(args[0]);
            await _manager.LoadCommandsAsync();
        }
    }
}