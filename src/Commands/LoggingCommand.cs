// Mugs/Commands/LoggingCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

public class LoggingCommand : ICommand
{
    public string Name => "logging";
    public string Description => LocalizationService.GetString("logging_description");
    public IEnumerable<string> Aliases => new[] { "log" };
    public string Author => "System";
    public string Version => "1.0";
    public string? UsageExample => LocalizationService.GetString("logging_usage");

    public Task ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            var state = AppSettings.EnableConsoleLogging
                ? LocalizationService.GetString("enabled")
                : LocalizationService.GetString("disabled");
            OutputService.WriteResponse("logging_state", state);
            return Task.CompletedTask;
        }

        var arg = args[0].ToLower();
        if (arg == "on" || arg == "enable" || arg == "true" || arg == "1")
        {
            AppSettings.EnableConsoleLogging = true;
            OutputService.WriteResponse("logging_enabled");
        }
        else if (arg == "off" || arg == "disable" || arg == "false" || arg == "0")
        {
            AppSettings.EnableConsoleLogging = false;
            OutputService.WriteResponse("logging_disabled");
        }
        else
        {
            OutputService.WriteError("logging_invalid_arg");
        }

        return Task.CompletedTask;
    }
}