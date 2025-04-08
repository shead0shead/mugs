// Mugs/Commands/ImportCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class ImportCommand : ICommand
    {
        private readonly CommandManager _manager;
        private readonly string _extensionsPath;
        private readonly ExtensionManager _extensionManager;

        public ImportCommand(CommandManager manager, string extensionsPath)
        {
            _manager = manager;
            _extensionsPath = extensionsPath;
            _extensionManager = new ExtensionManager(_extensionsPath);
        }

        public string Name => "import";
        public string Description => LocalizationService.GetString("import_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "import https://example.com/extension.csx";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteError("missing_url");
                return;
            }

            var url = args[0];
            await _extensionManager.ImportFromUrlAsync(url);
            await _manager.LoadCommandsAsync();
        }
    }
}