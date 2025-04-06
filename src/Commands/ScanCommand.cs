// Mugs/Commands/ScanCommand.cs

using Mugs.Services;
using Mugs.Interfaces;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Mugs.Commands
{
    public class ScanCommand : ICommand
    {
        private static readonly HashSet<string> DangerousTypes = new()
        {
            "System.IO.File", "System.IO.Directory", "System.Diagnostics.Process",
            "System.Net.WebClient", "System.Net.Http.HttpClient", "System.Reflection",
            "System.Runtime.InteropServices", "System.Security", "System.Management",
            "Microsoft.Win32", "System.Data.SqlClient", "System.Net.Sockets"
        };

        private static readonly HashSet<string> DangerousMethods = new()
        {
            "Delete", "Kill", "Start", "Execute", "Run", "Format",
            "WriteAllText", "WriteAllBytes", "WriteAllLines",
            "Remove", "Move", "Copy", "Create", "OpenWrite",
            "DownloadFile", "UploadFile", "ExecuteNonQuery",
            "ShellExecute", "CreateProcess", "Invoke",
            "GetProcAddress", "LoadLibrary", "SetWindowsHook"
        };

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
                ConsoleHelperService.WriteError("scan_missing_file");
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
                ConsoleHelperService.WriteError("scan_file_not_found", fileName);
                ConsoleHelperService.WriteResponse("full_path_display", Path.GetFullPath(fullPath));
                return;
            }

            try
            {
                var code = await File.ReadAllTextAsync(fullPath);
                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var root = await syntaxTree.GetRootAsync();

                var walker = new DangerousCodeWalker();
                walker.Visit(root);

                if (walker.DangerousCalls.Any())
                {
                    ConsoleHelperService.WriteError("scan_issues_found", fileName);
                    foreach (var call in walker.DangerousCalls.Distinct().OrderBy(c => c))
                    {
                        ConsoleHelperService.WriteError($"- {call}");
                    }
                    ConsoleHelperService.WriteResponse("scan_total_issues", walker.DangerousCalls.Count);
                }
                else
                {
                    ConsoleHelperService.WriteResponse("scan_no_issues", fileName);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("scan_error", ex.Message);
            }
        }

        private class DangerousCodeWalker : CSharpSyntaxWalker
        {
            public List<string> DangerousCalls { get; } = new();

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var methodName = node.ToString();
                if (DangerousMethods.Any(m => methodName.Contains(m)) ||
                    DangerousTypes.Any(t => methodName.StartsWith(t)))
                {
                    DangerousCalls.Add(methodName);
                }

                base.VisitInvocationExpression(node);
            }

            public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
            {
                var typeName = node.Type.ToString();
                if (DangerousTypes.Any(t => typeName.StartsWith(t)))
                {
                    DangerousCalls.Add($"new {typeName}()");
                }

                base.VisitObjectCreationExpression(node);
            }
        }
    }
}