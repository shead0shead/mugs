// Mugs/Commands/SpinnerCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class SpinnerCommand : ICommand
    {
        public string Name => "spinner";
        public string Description => LocalizationService.GetString("spinner_description");
        public IEnumerable<string> Aliases => new[] { "spin" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "spinner on\nspinner off\nspinner toggle";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteResponse("spinner_state",
                    AppSettings.EnableSpinnerAnimation ?
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
                    AppSettings.EnableSpinnerAnimation = true;
                    OutputService.WriteResponse("spinner_enabled");
                    break;

                case "off":
                case "disable":
                case "false":
                    AppSettings.EnableSpinnerAnimation = false;
                    OutputService.WriteResponse("spinner_disabled");
                    break;

                case "toggle":
                    AppSettings.EnableSpinnerAnimation = !AppSettings.EnableSpinnerAnimation;
                    OutputService.WriteResponse(AppSettings.EnableSpinnerAnimation ?
                        "spinner_enabled" : "spinner_disabled");
                    break;

                default:
                    OutputService.WriteError("spinner_invalid_arg");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}