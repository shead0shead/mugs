// Mugs/Commands/PromptStyleCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

namespace Mugs.Commands
{
    public class PromptStyleCommand : ICommand
    {
        public string Name => "promptstyle";
        public string Description => LocalizationService.GetString("prompt_description");
        public IEnumerable<string> Aliases => new[] { "prompt" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "promptstyle >\npromptstyle $\npromptstyle reset";

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteResponse("prompt_current", AppSettings.PromptStyle);
                return Task.CompletedTask;
            }

            if (args[0].Equals("reset", StringComparison.OrdinalIgnoreCase))
            {
                AppSettings.PromptStyle = ">";
                OutputService.WriteResponse("prompt_reset");
                return Task.CompletedTask;
            }

            if (args[0].Length > 3)
            {
                OutputService.WriteError("prompt_invalid_length");
                return Task.CompletedTask;
            }

            AppSettings.PromptStyle = args[0];
            OutputService.WriteResponse("prompt_changed", AppSettings.PromptStyle);
            return Task.CompletedTask;
        }
    }
}