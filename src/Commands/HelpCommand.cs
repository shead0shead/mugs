// Mugs/Commands/HelpCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;

namespace Mugs.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly CommandManager _manager;
        private readonly HashSet<string> _builtInCommands = new()
        {
            "help", "list", "reload", "clear", "restart",
            "time", "update", "new", "debug", "enable",
            "disable", "import", "language", "script",
            "suggestions", "alias", "scan", "history",
            "version", "logging"
        };

        public HelpCommand(CommandManager manager) => _manager = manager;
        public string Name => "help";
        public string Description => LocalizationService.GetString("help_description");
        public IEnumerable<string> Aliases => new[] { "?" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => LocalizationService.GetString("help_usage");

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length > 0)
            {
                var commandName = args[0].ToLowerInvariant();
                var command = _manager.GetCommand(commandName);

                if (command != null)
                {
                    await ShowCommandDetails(command);
                    return;
                }

                OutputService.WriteError("command_not_found", commandName);
            }

            await ShowAllCommands();
        }

        private async Task ShowCommandDetails(ICommand command)
        {
            var response = new StringBuilder();

            response.AppendLine($"{LocalizationService.GetString("command")}: {command.Name}\n");
            response.AppendLine($"{LocalizationService.GetString("description")}: {command.Description}");

            if (command.Aliases.Any())
            {
                response.AppendLine($"{LocalizationService.GetString("aliases")}: {string.Join(", ", command.Aliases)}");
            }

            response.AppendLine($"{LocalizationService.GetString("author")}: {command.Author}");
            response.AppendLine($"{LocalizationService.GetString("version")}: {command.Version}");

            if (!string.IsNullOrEmpty(command.UsageExample))
            {
                response.AppendLine();
                response.AppendLine(LocalizationService.GetString("usage_examples") + ":");
                var examples = command.UsageExample.Split('\n');
                foreach (var example in examples)
                {
                    response.AppendLine($"  {example.Trim()}");
                }
            }

            var fileName = $"{command.Name.ToLower()}.csx";
            if (VerifiedExtensionsService.IsExtensionVerified(fileName))
            {
                response.AppendLine();
                response.AppendLine($"{LocalizationService.GetString("verification")}: ✅ {LocalizationService.GetString("verified_safe")}");
            }

            OutputService.WriteResponse(response.ToString().TrimEnd());
        }

        private async Task ShowAllCommands()
        {
            await VerifiedExtensionsService.EnsureHashesLoadedAsync();

            var response = new StringBuilder();
            var allCommands = _manager.GetAllCommands()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            response.AppendLine(LocalizationService.GetString("builtin_commands"));
            foreach (var cmd in allCommands.Where(c => _builtInCommands.Contains(c.Name)))
            {
                response.AppendLine(FormatCommandLine(cmd));
            }

            var verifiedCommands = new List<ICommand>();
            foreach (var cmd in allCommands.Where(c => !_builtInCommands.Contains(c.Name)))
            {
                var cmdFileName = $"{cmd.Name.ToLower()}.csx";
                if (VerifiedExtensionsService.IsExtensionVerified(cmdFileName))
                {
                    verifiedCommands.Add(cmd);
                }
            }

            if (verifiedCommands.Any())
            {
                response.AppendLine();
                response.AppendLine(LocalizationService.GetString("verified_commands"));
                foreach (var cmd in verifiedCommands)
                {
                    response.AppendLine(FormatCommandLine(cmd) + " ✅");
                }
            }

            var externalCommands = allCommands
                .Where(c => !_builtInCommands.Contains(c.Name) &&
                       !verifiedCommands.Contains(c))
                .ToList();

            if (externalCommands.Any())
            {
                response.AppendLine();
                response.AppendLine(LocalizationService.GetString("external_commands"));
                foreach (var cmd in externalCommands)
                {
                    response.AppendLine(FormatCommandLine(cmd));
                }
            }

            response.AppendLine();
            response.Append(LocalizationService.GetString("command_help"));
            OutputService.WriteResponse(response.ToString());
        }

        private string FormatCommandLine(ICommand cmd)
        {
            var aliases = cmd.Aliases.Any()
                ? $" ({string.Join(", ", cmd.Aliases)})"
                : "";

            return $"  {cmd.Name,-12}{aliases,-15} - {cmd.Description}";
        }
    }
}