// Mugs/Commands/TimeCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class TimeCommand : ICommand
    {
        public string Name => "time";
        public string Description => LocalizationService.GetString("time_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => null;

        public Task ExecuteAsync(string[] args)
        {
            OutputService.WriteResponse("current_time", DateTime.Now.ToString("T"));
            return Task.CompletedTask;
        }
    }
}