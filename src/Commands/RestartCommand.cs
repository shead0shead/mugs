// Mugs/Commands/RestartCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Diagnostics;

namespace Mugs.Commands
{
    public class RestartCommand : ICommand
    {
        public string Name => "restart";
        public string Description => LocalizationService.GetString("restart_description");
        public IEnumerable<string> Aliases => new[] { "reboot" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => null;

        public Task ExecuteAsync(string[] args)
        {
            ConsoleHelperService.WriteResponse("restarting");

            var currentProcess = Process.GetCurrentProcess();
            var startInfo = new ProcessStartInfo
            {
                FileName = currentProcess.MainModule.FileName,
                Arguments = Environment.CommandLine,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            Environment.Exit(0);

            return Task.CompletedTask;
        }
    }
}