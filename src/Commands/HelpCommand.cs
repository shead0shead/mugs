// Mugs/Commands/HelpCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

using System.Text;

namespace Mugs.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly CommandManager _manager;

        public HelpCommand(CommandManager manager) => _manager = manager;
        public string Name => "help";
        public string Description => LocalizationService.GetString("help_description");
        public IEnumerable<string> Aliases => new[] { "?" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => LocalizationService.GetString("help_usage");

        public async Task ExecuteAsync(string[] args)
        {
            bool useTable = args.Length > 0 && args.Contains("--table");
            var commandArgs = args.Where(a => a != "--table").ToArray();

            if (commandArgs.Length > 0)
            {
                var commandName = commandArgs[0].ToLowerInvariant();
                var command = _manager.GetCommand(commandName);

                if (command != null)
                {
                    if (useTable || AppSettings.AlwaysUseTabularView)
                    {
                        await ShowCommandDetailsAsTable(command);
                    }
                    else
                    {
                        await ShowCommandDetails(command);
                    }
                    return;
                }

                OutputService.WriteError("command_not_found", commandName);
            }

            if (useTable || AppSettings.AlwaysUseTabularView)
            {
                await ShowAllCommandsAsTable();
            }
            else
            {
                await ShowAllCommands();
            }
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

        private async Task ShowCommandDetailsAsTable(ICommand command)
        {
            var fileName = $"{command.Name.ToLower()}.csx";
            var isVerified = VerifiedExtensionsService.IsExtensionVerified(fileName);

            var rows = new List<List<string>>();

            rows.Add(new List<string> {
                LocalizationService.GetString("command"),
                command.Name
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("description"),
                command.Description
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("author"),
                command.Author
            });
            rows.Add(new List<string> {
                LocalizationService.GetString("version"),
                command.Version
            });
            
            rows.Add(new List<string> {
                LocalizationService.GetString("aliases"),
                command.Aliases.Any() ? string.Join(", ", command.Aliases) : "-"
            });

            rows.Add(new List<string> {
                LocalizationService.GetString("verification"),
                isVerified ? "✅ " + LocalizationService.GetString("verified_safe") : "-"
            });

            if (!string.IsNullOrEmpty(command.UsageExample))
            {
                var examples = command.UsageExample.Split('\n');
                for (int i = 0; i < examples.Length; i++)
                {
                    rows.Add(new List<string> {
                        i == 0 ? LocalizationService.GetString("usage_examples") : "",
                        examples[i].Trim()
                    });
                }
            }

            var commandName = char.ToUpper(command.Name[0]) + command.Name.Substring(1);
            OutputService.WriteTableColumnsHighlight(
                LocalizationService.GetString("details_title", commandName),
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


        private async Task ShowAllCommands()
        {
            await VerifiedExtensionsService.EnsureHashesLoadedAsync();

            var response = new StringBuilder();
            var allCommands = _manager.GetAllCommands()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            response.AppendLine(LocalizationService.GetString("system_commands"));
            foreach (var cmd in allCommands.Where(c => _manager._systemCommands.Contains(c.Name)))
            {
                response.AppendLine(FormatCommandLine(cmd));
            }

            response.AppendLine();
            response.AppendLine(LocalizationService.GetString("settings_commands"));
            foreach (var cmd in allCommands.Where(c => _manager._settingsCommands.Contains(c.Name)))
            {
                response.AppendLine(FormatCommandLine(cmd));
            }

            var verifiedCommands = new List<ICommand>();
            foreach (var cmd in allCommands.Where(c => !_manager._builtInCommands.Contains(c.Name)))
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
                .Where(c => !_manager._builtInCommands.Contains(c.Name) &&
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

        private async Task ShowAllCommandsAsTable()
        {
            await VerifiedExtensionsService.EnsureHashesLoadedAsync();
            var allCommands = _manager.GetAllCommands()
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            var system = allCommands
                .Where(c => _manager._systemCommands.Contains(c.Name))
                .ToList();

            if (system.Any())
            {
                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("system_commands"),
                    new List<IEnumerable<string>> {
                        system.Select(c => c.Name),
                        system.Select(c => c.Description),
                        system.Select(c => string.Join(", ", c.Aliases))
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases")
                    }
                );
            }

            var settings = allCommands
                .Where(c => _manager._settingsCommands.Contains(c.Name))
                .ToList();

            if (settings.Any())
            {
                OutputService.WriteTableColumnsHighlight(
                    LocalizationService.GetString("settings_commands"),
                    new List<IEnumerable<string>> {
                        settings.Select(c => c.Name),
                        settings.Select(c => c.Description),
                        settings.Select(c => string.Join(", ", c.Aliases))
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
                        verified.Select(c => "✅")
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases"),
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
                        external.Select(c => string.Join(", ", c.Aliases))
                    },
                    new List<string> {
                        LocalizationService.GetString("command"),
                        LocalizationService.GetString("description"),
                        LocalizationService.GetString("aliases")
                    }
                );
            }
        }
    }
}