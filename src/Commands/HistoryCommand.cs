// Mugs/Commands/HistoryCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;

namespace Mugs.Commands
{
    public class HistoryCommand : ICommand
    {
        public string Name => "history";
        public string Description => LocalizationService.GetString("history_description");
        public IEnumerable<string> Aliases => new[] { "hist" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "history 10\nhistory --search \"update\"";

        public Task ExecuteAsync(string[] args)
        {
            var history = InputService.GetCommandHistory();
            var response = new StringBuilder();

            if (args.Length > 0 && args[0] == "--search")
            {
                if (args.Length < 2)
                {
                    OutputService.WriteError("missing_search_term");
                    return Task.CompletedTask;
                }

                var searchTerm = string.Join(" ", args.Skip(1)).ToLowerInvariant();
                var results = history.Where(cmd => cmd.ToLowerInvariant().Contains(searchTerm)).ToList();

                if (results.Any())
                {
                    response.AppendLine(LocalizationService.GetString("history_search_results", searchTerm));
                    foreach (var cmd in results)
                    {
                        response.AppendLine($"- {cmd}");
                    }
                }
                else
                {
                    response.AppendLine(LocalizationService.GetString("history_no_results", searchTerm));
                }
            }
            else
            {
                int count = 10;
                if (args.Length > 0 && int.TryParse(args[0], out int requestedCount) && requestedCount > 0)
                {
                    count = requestedCount;
                }
                else if (args.Length > 0)
                {
                    response.AppendLine(LocalizationService.GetString("history_invalid_count"));
                }

                var commandsToShow = history.TakeLast(count).ToList();

                if (count >= history.Count)
                {
                    response.AppendLine(LocalizationService.GetString("history_full"));
                }
                else
                {
                    response.AppendLine(LocalizationService.GetString("history_showing", count));
                }

                foreach (var cmd in commandsToShow)
                {
                    response.AppendLine($"- {cmd}");
                }
            }

            OutputService.WriteResponse(response.ToString().TrimEnd());
            return Task.CompletedTask;
        }
    }
}