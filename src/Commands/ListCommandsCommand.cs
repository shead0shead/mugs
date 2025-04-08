// Mugs/Commands/ListCommandsCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;

namespace Mugs.Commands
{
    public class ListCommandsCommand : ICommand
    {
        private readonly CommandManager _manager;
        private readonly string _extensionsPath;

        public ListCommandsCommand(CommandManager manager, string extensionsPath)
        {
            _manager = manager;
            _extensionsPath = extensionsPath;
        }

        public string Name => "list";
        public string Description => LocalizationService.GetString("list_description");
        public IEnumerable<string> Aliases => new[] { "ls", "dir" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => null;

        public async Task ExecuteAsync(string[] args)
        {
            await VerifiedExtensionsService.EnsureHashesLoadedAsync();

            var response = new StringBuilder();
            response.AppendLine(LocalizationService.GetString("available_commands"));

            foreach (var cmd in _manager.GetAllCommands()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name))
            {
                var fileName = $"{cmd.Name.ToLower()}.csx";
                var isVerified = VerifiedExtensionsService.IsExtensionVerified(fileName);
                var verifiedMark = isVerified ? " ✅" : "";

                response.AppendLine($"- {cmd.Name}{(cmd.Aliases.Any() ? $" ({LocalizationService.GetString("aliases")}: {string.Join(", ", cmd.Aliases)})" : "")}{verifiedMark}");
                response.AppendLine($"  {LocalizationService.GetString("version")}: {cmd.Version}, {LocalizationService.GetString("author")}: {cmd.Author}");
                if (isVerified)
                {
                    response.AppendLine($"  {LocalizationService.GetString("verified")}");
                }
                if (!string.IsNullOrEmpty(cmd.UsageExample))
                {
                    response.AppendLine($"  {LocalizationService.GetString("example")}: {cmd.UsageExample}");
                }
                response.AppendLine();
            }

            var disabledFiles = Directory.GetFiles(_extensionsPath, "*.csx.disable");
            if (disabledFiles.Any())
            {
                response.AppendLine(LocalizationService.GetString("disabled_extensions"));
                foreach (var file in disabledFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var isVerified = VerifiedExtensionsService.IsExtensionVerified($"{fileName}.csx");
                    var verifiedMark = isVerified ? " ✅" : "";
                    response.AppendLine($"- {fileName}{verifiedMark}");
                }
                response.AppendLine("\n" + LocalizationService.GetString("enable_usage"));
            }

            OutputService.WriteResponse(response.ToString().TrimEnd());
        }
    }
}