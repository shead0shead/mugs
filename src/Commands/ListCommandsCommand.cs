// Mugs/Commands/ListCommandsCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

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
            bool useTable = args.Length > 0 && args.Contains("--table");

            await VerifiedExtensionsService.EnsureHashesLoadedAsync();

            if (useTable || AppSettings.AlwaysUseTabularView)
            {
                await ShowCommandsAsTable();
            }
            else
            {
                await ShowCommandsAsList();
            }
        }

        private async Task ShowCommandsAsList()
        {
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

        private async Task ShowCommandsAsTable()
        {
            var allCommands = _manager.GetAllCommands()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            var builtIn = allCommands
                .Where(c => _manager._builtInCommands.Contains(c.Name))
                .ToList();

            if (builtIn.Any())
            {
                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("builtin_commands"),
                    new List<IEnumerable<string>> {
                        builtIn.Select(c => c.Name),
                        builtIn.Select(c => c.Description),
                        builtIn.Select(c => string.Join(", ", c.Aliases))
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases")
                    }
                );
            }

            var verified = allCommands
                .Where(c => !_manager._builtInCommands.Contains(c.Name) &&
                           VerifiedExtensionsService.IsExtensionVerified($"{c.Name.ToLower()}.csx"))
                .ToList();

            if (verified.Any())
            {
                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("verified_commands"),
                    new List<IEnumerable<string>> {
                        verified.Select(c => c.Name),
                        verified.Select(c => c.Description),
                        verified.Select(c => string.Join(", ", c.Aliases)),
                        verified.Select(c => c.Author),
                        verified.Select(c => "✅")
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases"),
                        LocalizationService.GetString("author"),
                        LocalizationService.GetString("verification")
                    }
                );
            }

            var external = allCommands
                .Where(c => !_manager._builtInCommands.Contains(c.Name) &&
                           !VerifiedExtensionsService.IsExtensionVerified($"{c.Name.ToLower()}.csx"))
                .ToList();

            if (external.Any())
            {
                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("external_commands"),
                    new List<IEnumerable<string>> {
                        external.Select(c => c.Name),
                        external.Select(c => c.Description),
                        external.Select(c => string.Join(", ", c.Aliases)),
                        external.Select(c => c.Author)
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases"),
                        LocalizationService.GetString("author")
                    }
                );
            }

            var disabledFiles = Directory.GetFiles(_extensionsPath, "*.csx.disable");
            if (disabledFiles.Any())
            {
                var disabledCommands = disabledFiles
                    .Select(f => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f)))
                    .ToList();

                var verifiedStatus = disabledCommands
                    .Select(c => VerifiedExtensionsService.IsExtensionVerified($"{c}.csx") ? "✅" : "")
                    .ToList();

                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("disabled_extensions"),
                    new List<IEnumerable<string>> {
                        disabledCommands,
                        verifiedStatus
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("verification")
                    }
                );

                OutputService.WriteResponse(LocalizationService.GetString("enable_usage"));
            }
        }
    }
}