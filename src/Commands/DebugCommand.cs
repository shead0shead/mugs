// Mugs/Commands/DebugCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using Newtonsoft.Json;
using System.Diagnostics;

namespace Mugs.Commands
{
    public class DebugCommand : ICommand
    {
        private readonly CommandManager _manager;

        public DebugCommand(CommandManager manager) => _manager = manager;
        public string Name => "debug";
        public string Description => LocalizationService.GetString("debug_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "debug mycommand --args \"test\"";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelperService.WriteError("missing_debug_command");
                return;
            }

            var commandName = args[0];
            var commandArgs = ParseDebugArgs(args.Skip(1).ToArray());

            var command = _manager.GetCommand(commandName);
            if (command == null)
            {
                ConsoleHelperService.WriteError("command_not_found", commandName);
                return;
            }

            ConsoleHelperService.WriteDebug(LocalizationService.GetString("debug_start", commandName, string.Join(" ", commandArgs)));
            ConsoleHelperService.WriteDebug(LocalizationService.GetString("debug_vars", JsonConvert.SerializeObject(commandArgs)));

            try
            {
                var stopwatch = Stopwatch.StartNew();
                await command.ExecuteAsync(commandArgs);
                stopwatch.Stop();

                ConsoleHelperService.WriteDebug(LocalizationService.GetString("debug_completed", stopwatch.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteDebug(LocalizationService.GetString("debug_error", ex.GetType().Name, ex.Message));
                throw;
            }
        }

        private string[] ParseDebugArgs(string[] args)
        {
            var result = new List<string>();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--args" && i + 1 < args.Length)
                {
                    var argValue = args[i + 1];
                    if (argValue.StartsWith("\"") && argValue.EndsWith("\""))
                    {
                        argValue = argValue.Substring(1, argValue.Length - 2);
                    }
                    result.Add(argValue);
                    i++;
                }
                else
                {
                    result.Add(args[i]);
                }
            }
            return result.ToArray();
        }
    }
}