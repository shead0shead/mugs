// Mugs/Commands/ClearCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Runtime.InteropServices;

namespace Mugs.Commands
{
    public class ClearCommand : ICommand
    {
        private readonly CommandManager _manager;

        public ClearCommand(CommandManager manager) => _manager = manager;
        public string Name => "clear";
        public string Description => LocalizationService.GetString("clear_description");
        public IEnumerable<string> Aliases => new[] { "cls", "clean" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => null;

        public Task ExecuteAsync(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.Clear();
                Console.Write("\x1b[3J");
                Console.SetCursorPosition(0, 0);
            }
            else
            {
                Console.Write("\x1b[2J\x1b[H");
            }

            ConsoleHelperService.Initialize();
            return Task.CompletedTask;
        }
    }
}