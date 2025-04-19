// Mugs/Commands/SuggestionsCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class SuggestionsCommand : ICommand
    {
        public string Name => "suggestions";
        public string Description => LocalizationService.GetString("suggestions_description");
        public IEnumerable<string> Aliases => new[] { "hints" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "suggestions on\nsuggestions off\nsuggestions toggle";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                if (args.Length == 0)
                {
                    OutputService.WriteResponse("suggestions_state",
                        AppSettings.EnableSuggestions ?
                        LocalizationService.GetString("enabled") :
                        LocalizationService.GetString("disabled"));
                    return Task.CompletedTask;
                }
            }

            var arg = args[0].ToLower();
            switch (arg)
            {
                case "on":
                case "enable":
                case "true":
                    AppSettings.EnableSuggestions = true;
                    OutputService.WriteResponse("suggestions_enabled");
                    break;

                case "off":
                case "disable":
                case "false":
                    AppSettings.EnableSuggestions = false;
                    OutputService.WriteResponse("suggestions_disabled");
                    break;

                case "toggle":
                    AppSettings.EnableSuggestions = !AppSettings.EnableSuggestions;
                    OutputService.WriteResponse(AppSettings.EnableSuggestions ?
                        "suggestions_enabled" : "suggestions_disabled");
                    break;

                default:
                    OutputService.WriteError("suggestions_invalid_arg");
                    break;
            }

            return Task.CompletedTask;
        }
    }
}