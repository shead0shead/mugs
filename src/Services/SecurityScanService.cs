// Mugs/Services/SecurityScanService.cs

using Mugs.Interfaces;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Mugs.Services
{
    public static class SecurityScanService
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

        public static async Task CheckCommandsSafety(IEnumerable<ICommand> commands, string extensionsPath, HashSet<string> builtInCommands)
        {
            var unverifiedCommands = commands
                .GroupBy(c => c.Name)
                .Select(g => g.First())
                .Where(c => !builtInCommands.Contains(c.Name) &&
                           !VerifiedExtensionsService.IsExtensionVerified($"{c.Name.ToLower()}.csx"))
                .ToList();

            if (!unverifiedCommands.Any()) return;

            foreach (var cmd in unverifiedCommands)
            {
                var fileName = $"{cmd.Name.ToLower()}.csx";
                var filePath = Path.Combine(extensionsPath, fileName);

                if (File.Exists(filePath))
                {
                    try
                    {
                        var issues = await ScanFileForDangerousCode(filePath);
                        if (issues.Any())
                        {
                            OutputService.WriteError("scan_issues_found", fileName);
                            foreach (var issue in issues)
                            {
                                OutputService.WriteError($"- {issue}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerService.LogError($"Error scanning {fileName}", ex);
                    }
                }
            }
        }

        public static async Task<List<string>> ScanFileForDangerousCode(string filePath)
        {
            var code = await File.ReadAllTextAsync(filePath);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var root = await syntaxTree.GetRootAsync();

            var walker = new DangerousCodeWalker();
            walker.Visit(root);

            return walker.DangerousCalls;
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