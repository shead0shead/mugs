// Mugs/Commands/ScanCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

namespace Mugs.Commands
{
    public class ScanCommand : ICommand
    {
        private readonly string _extensionsPath;

        public ScanCommand(string extensionsPath)
        {
            _extensionsPath = extensionsPath;
        }

        public string Name => "scan";
        public string Description => LocalizationService.GetString("scan_description");
        public IEnumerable<string> Aliases => new[] { "analyze" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "scan mycommand.csx";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteError("scan_missing_file");
                return;
            }

            var fileName = args[0];
            if (!fileName.EndsWith(".csx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".csx";
            }

            var fullPath = Path.Combine(_extensionsPath, fileName);

            if (!File.Exists(fullPath))
            {
                OutputService.WriteError("scan_file_not_found", fileName);
                OutputService.WriteResponse("full_path_display", Path.GetFullPath(fullPath));
                return;
            }

            try
            {
                var issues = await SecurityScanService.ScanFileForDangerousCode(fullPath);
                if (issues.Any())
                {
                    OutputService.WriteError("scan_issues_found", fileName);
                    foreach (var issue in issues)
                    {
                        OutputService.WriteError($"- {issue}");
                    }
                    OutputService.WriteResponse("scan_total_issues", issues.Count);
                }
                else
                {
                    OutputService.WriteResponse("scan_no_issues", fileName);
                }
            }
            catch (Exception ex)
            {
                OutputService.WriteError("scan_error", ex.Message);
            }
        }
    }
}