// Mugs/Commands/VersionCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;

namespace Mugs.Commands
{
    public class VersionCommand : ICommand
    {
        private readonly CommandManager _manager;

        public VersionCommand(CommandManager manager)
        {
            _manager = manager;
        }

        public string Name => "version";
        public string Description => LocalizationService.GetString("version_description");
        public IEnumerable<string> Aliases => new[] { "ver" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "version";

        public async Task ExecuteAsync(string[] args)
        {
            bool useTable = args.Length > 0 && args.Contains("--table");

            if (useTable)
            {
                await ShowVersionAsTable();
            }
            else
            {
                await ShowVersionAsDefault();
            }
        }
        
        private async Task ShowVersionAsDefault()
        {
            var asciiArt = new[]
{
                "░░░░░░░░░░░░░     ",
                "▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒ ",
                "▓▓▓▓▓▓▓▓▓▓▓▓▓   ▓▓",
                "▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒ ",
                "░░░░░░░░░░░░░     ",
                " ░░░░░░░░░░░      "
            };

            var extensionsPath = Path.Combine(AppContext.BaseDirectory, "Extensions");
            var extensionsCount = Directory.Exists(extensionsPath)
                ? Directory.GetFiles(extensionsPath, "*.csx").Length
                : 0;

            var info = new[]
            {
                $"{$"{LocalizationService.GetString("application")}:",-15} Mugs Console Add-on Platform",
                $"{$"{LocalizationService.GetString("version")}:",-15} {UpdateCheckerService.CurrentVersion}",
                $"{$"{LocalizationService.GetString("author")}:",-15} Shead (https://github.com/shead0shead)",
                $"{$"{LocalizationService.GetString("repo")}:",-15} https://github.com/shead0shead/mugs",
                $"{$"{LocalizationService.GetString("commands")}:",-15} {_manager.GetAllCommands().Count()} {LocalizationService.GetString("available")}",
                $"{$"{LocalizationService.GetString("extensions")}:",-15} {extensionsCount} {LocalizationService.GetString("loaded")}"
            };

            var maxArtLength = asciiArt.Max(line => line.Length);
            var output = new StringBuilder();

            for (int i = 0; i < Math.Max(asciiArt.Length, info.Length); i++)
            {
                var artLine = i < asciiArt.Length ? asciiArt[i] : new string(' ', maxArtLength);
                var infoLine = i < info.Length ? info[i] : "";

                output.AppendLine($"{artLine}  {infoLine}");
            }

            OutputService.WriteResponse(output.ToString().TrimEnd());
        }

        private async Task ShowVersionAsTable()
        {
            var extensionsPath = Path.Combine(AppContext.BaseDirectory, "Extensions");
            var extensionsCount = Directory.Exists(extensionsPath)
                ? Directory.GetFiles(extensionsPath, "*.csx").Length
                : 0;

            var rows = new List<List<string>>();

            rows.Add(new List<string> {
                LocalizationService.GetString("application"),
                "Mugs Console Add-on Platform"
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("version"),
                $"{UpdateCheckerService.CurrentVersion}"
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("author"),
                "Shead (https://github.com/shead0shead)"
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("repo"),
                "https://github.com/shead0shead/mugs"
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("commands"),
                $"{_manager.GetAllCommands().Count()} {LocalizationService.GetString("available")}"
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("extensions"),
                $"{extensionsCount} {LocalizationService.GetString("loaded")}"
            });

            OutputService.WriteTableColumnsHighlight(
                LocalizationService.GetString("version_table_title"),
                new List<IEnumerable<string>> {
                    rows.Select(r => r[0]),
                    rows.Select(r => r[1])
                },
                new List<string> {
                    LocalizationService.GetString("property"),
                    LocalizationService.GetString("value")
                }
            );
        }
    }
}