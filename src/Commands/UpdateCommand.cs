// Mugs/Commands/UpdateCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using System.Text;
using Mugs.Models;

namespace Mugs.Commands
{
    public class UpdateCommand : ICommand
    {
        public string Name => "update";
        public string Description => LocalizationService.GetString("update_description");
        public IEnumerable<string> Aliases => Enumerable.Empty<string>();
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "update install\nupdate check\nupdate settings";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                await ShowUpdateStatus();
                return;
            }

            switch (args[0].ToLower())
            {
                case "install":
                    await InstallUpdate(args);
                    break;

                case "check":
                    await UpdateCheckerService.CheckForUpdatesAsync(true);
                    break;

                case "settings":
                    await ConfigureAutoUpdate(args.Skip(1).ToArray());
                    break;

                default:
                    OutputService.WriteError("update_invalid_command");
                    break;
            }
        }

        private async Task ShowUpdateStatus()
        {
            var manifest = await UpdateCheckerService.GetUpdateManifestAsync();
            if (manifest == null)
            {
                OutputService.WriteError("update_manifest_error");
                return;
            }

            var response = new StringBuilder();
            response.AppendLine(LocalizationService.GetString("update_status"));
            response.AppendLine($"{LocalizationService.GetString("current_version")}: {UpdateCheckerService.CurrentVersion}");
            response.AppendLine($"{LocalizationService.GetString("latest_version")}: {manifest.LatestVersion}");
            response.AppendLine($"{LocalizationService.GetString("auto_update_status")}: " +
                (AppSettings.AutoCheckEnabled ?
                    LocalizationService.GetString("enabled") :
                    LocalizationService.GetString("disabled")));

            if (AppSettings.AutoCheckEnabled)
            {
                response.AppendLine($"{LocalizationService.GetString("check_interval")}: " +
                    $"{AppSettings.AutoCheckIntervalHours} {LocalizationService.GetString("hours")}");
            }

            OutputService.WriteResponse(response.ToString());
        }

        private async Task InstallUpdate(string[] args)
        {
            bool force = args.Length > 1 && args[1] == "--force";
            var manifest = await UpdateCheckerService.GetUpdateManifestAsync();

            if (manifest == null)
            {
                OutputService.WriteError("update_manifest_error");
                return;
            }

            var latestVersion = new Version(manifest.LatestVersion);
            if (latestVersion <= UpdateCheckerService.CurrentVersion && !force)
            {
                OutputService.WriteResponse("no_update_available", UpdateCheckerService.CurrentVersion);
                return;
            }

            OutputService.WriteResponse("confirm_update");
            var response = Console.ReadLine();
            if (response.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                await UpdateCheckerService.InstallUpdateAsync(manifest);
            }
            else
            {
                OutputService.WriteResponse("update_cancelled");
            }
        }

        private async Task ConfigureAutoUpdate(string[] args)
        {
            if (args.Length == 0)
            {
                var status = new StringBuilder();
                status.AppendLine($"{LocalizationService.GetString("auto_update_status")}: " +
                    (AppSettings.AutoCheckEnabled ?
                        LocalizationService.GetString("enabled") :
                        LocalizationService.GetString("disabled")));
                status.AppendLine($"{LocalizationService.GetString("check_interval")}: " +
                    $"{AppSettings.AutoCheckIntervalHours} {LocalizationService.GetString("hours")}");
                status.AppendLine($"{LocalizationService.GetString("last_check_time")}: " +
                    $"{AppSettings.LastUpdateCheck:g}");

                OutputService.WriteResponse(status.ToString());
                return;
            }

            switch (args[0].ToLower())
            {
                case "enable":
                    AppSettings.AutoCheckEnabled = true;
                    OutputService.WriteResponse("auto_update_enabled");
                    break;

                case "disable":
                    AppSettings.AutoCheckEnabled = false;
                    OutputService.WriteResponse("auto_update_disabled");
                    break;

                case "interval":
                    if (args.Length > 1 && int.TryParse(args[1], out int hours) && hours > 0)
                    {
                        AppSettings.AutoCheckIntervalHours = hours;
                        OutputService.WriteResponse("auto_update_interval_set", hours);
                    }
                    else
                    {
                        OutputService.WriteError("invalid_interval");
                    }
                    break;

                default:
                    OutputService.WriteError("invalid_auto_update_command");
                    break;
            }
        }
    }
}