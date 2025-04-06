// Mugs/Commands/ScriptCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class ScriptCommand : ICommand
    {
        public string Name => "script";
        public string Description => LocalizationService.GetString("script_description");
        public IEnumerable<string> Aliases => new[] { "batch", "run" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "script commands.txt";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelperService.WriteError("missing_script_file");
                return;
            }

            var fileName = args[0];
            if (!File.Exists(fileName))
            {
                ConsoleHelperService.WriteError("script_file_not_found", fileName);
                return;
            }

            try
            {
                var commands = await File.ReadAllLinesAsync(fileName);
                foreach (var command in commands)
                {
                    if (string.IsNullOrWhiteSpace(command)) continue;
                    if (command.TrimStart().StartsWith("#")) continue;

                    ConsoleHelperService.WriteResponse("executing_command", command);
                    var parts = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var cmdName = parts[0];
                    var cmdArgs = parts.Length > 1 ? parts.Skip(1).ToArray() : Array.Empty<string>();

                    ConsoleHelperService.WriteResponse("command_output", $"Executing: {command}");
                    await Task.Delay(100);
                }
                ConsoleHelperService.WriteResponse("script_completed");
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("script_error", ex.Message);
            }
        }
    }
}