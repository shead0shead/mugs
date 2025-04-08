// Mugs/Commands/AliasCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;

namespace Mugs.Commands
{
    public class AliasCommand : ICommand
    {
        public string Name => "alias";
        public string Description => LocalizationService.GetString("alias_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => LocalizationService.GetString("alias_usage");

        public Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0 || args[0] == "list")
            {
                var aliases = AliasManagerService.GetAllAliases();
                if (!aliases.Any())
                {
                    OutputService.WriteResponse("alias_no_aliases");
                    return Task.CompletedTask;
                }

                var response = new StringBuilder(LocalizationService.GetString("alias_header") + "\n");
                foreach (var alias in aliases)
                {
                    response.AppendLine($"- {alias.Key} => {alias.Value}");
                }
                OutputService.WriteResponse(response.ToString().TrimEnd());
                return Task.CompletedTask;
            }

            switch (args[0].ToLower())
            {
                case "add" when args.Length >= 3:
                    AliasManagerService.AddAlias(args[1], args[2]);
                    OutputService.WriteResponse("alias_added", args[2], args[1]);
                    break;

                case "remove" when args.Length >= 2:
                    if (AliasManagerService.RemoveAlias(args[1]))
                        OutputService.WriteResponse("alias_removed", args[1]);
                    else
                        OutputService.WriteError("alias_not_found");
                    break;

                default:
                    OutputService.WriteError("alias_invalid_syntax");
                    break;
            }
            return Task.CompletedTask;
        }
    }
}