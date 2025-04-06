// Mugs/Commands/ToggleSuggestionsCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class ToggleSuggestionsCommand : ICommand
    {
        public string Name => "suggestions";
        public string Description => LocalizationService.GetString("toggle_suggestions");
        public IEnumerable<string> Aliases => new[] { "ts" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "toggle-suggestions";

        public Task ExecuteAsync(string[] args)
        {
            AppSettings.EnableSuggestions = !AppSettings.EnableSuggestions;
            ConsoleHelperService.WriteResponse(AppSettings.EnableSuggestions
                ? LocalizationService.GetString("suggestions_enabled")
                : LocalizationService.GetString("suggestions_disabled"));
            return Task.CompletedTask;
        }
    }
}