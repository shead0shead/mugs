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
    public string? UsageExample => "logging on\nlogging off\nlogging toggle";

    public Task ExecuteAsync(string[] args)
    {
        if (args.Length == 0)
        {
            OutputService.WriteResponse("logging_state",
                AppSettings.EnableConsoleLogging ?
                LocalizationService.GetString("enabled") :
                LocalizationService.GetString("disabled"));
            return Task.CompletedTask;
        }

        var arg = args[0].ToLower();
        switch (arg)
        {
            case "on":
            case "enable":
            case "true":
                AppSettings.EnableConsoleLogging = true;
                OutputService.WriteResponse("logging_enabled");
                break;

            case "off":
            case "disable":
            case "false":
                AppSettings.EnableConsoleLogging = false;
                OutputService.WriteResponse("logging_disabled");
                break;

            case "toggle":
                AppSettings.EnableConsoleLogging = !AppSettings.EnableConsoleLogging;
                OutputService.WriteResponse(AppSettings.EnableConsoleLogging ?
                    "logging_enabled" : "logging_disabled");
                break;

            default:
                OutputService.WriteError("logging_invalid_arg");
                break;
        }

        return Task.CompletedTask;
    }
}