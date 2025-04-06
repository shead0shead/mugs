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

        public Task ExecuteAsync(string[] args)
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
                $"{$"{LocalizationService.GetString("version")}:",-15} 1.0.0",
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

            ConsoleHelperService.WriteResponse(output.ToString().TrimEnd());
            return Task.CompletedTask;
        }
    }
}