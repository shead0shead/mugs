// Mugs/Commands/ToggleTableViewCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class ToggleTableViewCommand : ICommand
    {
        public string Name => "tabular";
        public string Description => LocalizationService.GetString("tabular_description");
        public IEnumerable<string> Aliases => new[] { "ttv" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "tabular on\ntabular off\ntabular toggle";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteResponse("tabular_current_mode",
                    AppSettings.AlwaysUseTabularView ?
                    LocalizationService.GetString("enabled") :
                    LocalizationService.GetString("disabled"));
                return Task.CompletedTask;
            }

            var mode = args[0].ToLower();
            switch (mode)
            {
                case "on":
                case "enable":
                case "true":
                    AppSettings.AlwaysUseTabularView = true;
                    OutputService.WriteResponse("tabular_mode_enabled");
                    break;

                case "off":
                case "disable":
                case "false":
                    AppSettings.AlwaysUseTabularView = false;
                    OutputService.WriteResponse("tabular_mode_disabled");
                    break;

                case "toggle":
                    AppSettings.AlwaysUseTabularView = !AppSettings.AlwaysUseTabularView;
                    OutputService.WriteResponse(AppSettings.AlwaysUseTabularView ?
                        "tabular_mode_enabled" : "tabular_mode_disabled");
                    break;

                default:
                    OutputService.WriteError("tabular_invalid_mode");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}