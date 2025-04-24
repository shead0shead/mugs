// Mugs/Commands/BoxedCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class BoxedCommand : ICommand
    {
        public string Name => "boxed";
        public string Description => LocalizationService.GetString("boxed_description");
        public IEnumerable<string> Aliases => new[] { "box" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "boxed on\nboxed off\nboxed toggle\nboxed title [on|off|toggle]";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteResponse("boxed_state",
                    AppSettings.EnableBoxedOutput ?
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
                    AppSettings.EnableBoxedOutput = true;
                    OutputService.WriteResponse("boxed_enabled");
                    break;

                case "off":
                case "disable":
                case "false":
                    AppSettings.EnableBoxedOutput = false;
                    OutputService.WriteResponse("boxed_disabled");
                    break;

                case "toggle":
                    AppSettings.EnableBoxedOutput = !AppSettings.EnableBoxedOutput;
                    OutputService.WriteResponse(AppSettings.EnableBoxedOutput ?
                        "boxed_enabled" : "boxed_disabled");
                    break;

                case "title":
                    HandleTitleCommand(args.Skip(1).ToArray());
                    break;

                default:
                    OutputService.WriteError("boxed_invalid_arg");
                    break;
            }

            return Task.CompletedTask;
        }

        private void HandleTitleCommand(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteResponse("boxed_title_state",
                    AppSettings.EnableBoxedOutputTitle ?
                    LocalizationService.GetString("enabled") :
                    LocalizationService.GetString("disabled"));
                return;
            }

            var subCommand = args[0].ToLower();

            switch (subCommand)
            {
                case "on":
                case "enable":
                case "true":
                    AppSettings.EnableBoxedOutputTitle = true;
                    OutputService.WriteResponse("boxed_title_enabled");
                    break;

                case "off":
                case "disable":
                case "false":
                    AppSettings.EnableBoxedOutputTitle = false;
                    OutputService.WriteResponse("boxed_title_disabled");
                    break;

                case "toggle":
                    AppSettings.EnableBoxedOutputTitle = !AppSettings.EnableBoxedOutputTitle;
                    OutputService.WriteResponse(AppSettings.EnableBoxedOutputTitle ?
                        "boxed_title_enabled" : "boxed_title_disabled");
                    break;

                default:
                    OutputService.WriteError("boxed_title_invalid_arg");
                    break;
            }
        }
    }
}