// Mugs/Commands/UpdateCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class UpdateCommand : ICommand
    {
        public string Name => "update";
        public string Description => LocalizationService.GetString("update_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "update install";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length > 0 && args[0].Equals("install", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleHelperService.WriteResponse("confirm_update");
                var response = Console.ReadLine();
                if (response.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    await UpdateCheckerService.InstallUpdateAsync();
                }
                else
                {
                    ConsoleHelperService.WriteResponse("update_cancelled");
                }
            }
            else
            {
                await UpdateCheckerService.CheckForUpdatesAsync(true);
            }
        }
    }
}