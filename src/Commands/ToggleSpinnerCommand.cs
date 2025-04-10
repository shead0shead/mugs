// Mugs/Commands/ToggleSpinnerCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class ToggleSpinnerCommand : ICommand
    {
        public string Name => "spinner";
        public string Description => LocalizationService.GetString("toggle_spinner_description");
        public IEnumerable<string> Aliases => new[] { "tsp" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "spinner";

        public Task ExecuteAsync(string[] args)
        {
            AppSettings.EnableSpinnerAnimation = !AppSettings.EnableSpinnerAnimation;
            OutputService.WriteResponse(AppSettings.EnableSpinnerAnimation
                ? LocalizationService.GetString("spinner_enabled")
                : LocalizationService.GetString("spinner_disabled"));
            return Task.CompletedTask;
        }
    }
}