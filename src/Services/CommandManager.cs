// Mugs/Services/CommandManager.cs

using Mugs.Interfaces;
using Mugs.Models;
using Mugs.Commands;

using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Text;

namespace Mugs.Services
{
    public class CommandManager
    {
        private readonly Dictionary<string, ICommand> _commands = new();
        private readonly string _extensionsPath;

        public CommandManager(string extensionsPath)
        {
            _extensionsPath = extensionsPath;
            Directory.CreateDirectory(extensionsPath);
        }

        public async Task LoadCommandsAsync()
        {
            _commands.Clear();
            ScriptCacheService.Clear();
            MetadataCacheService.Clear();
            RegisterBuiltInCommands();
            await LoadExternalCommandsAsync();
        }

        private void RegisterBuiltInCommands()
        {
            RegisterCommand(new HelpCommand(this));
            RegisterCommand(new ListCommandsCommand(this, _extensionsPath));
            RegisterCommand(new ReloadCommandsCommand(this));
            RegisterCommand(new ClearCommand(this));
            RegisterCommand(new RestartCommand());
            RegisterCommand(new TimeCommand());
            RegisterCommand(new UpdateCommand());
            RegisterCommand(new NewCommand(_extensionsPath));
            RegisterCommand(new DebugCommand(this));
            RegisterCommand(new EnableCommand(this, _extensionsPath));
            RegisterCommand(new DisableCommand(this, _extensionsPath));
            RegisterCommand(new ImportCommand(this, _extensionsPath));
            RegisterCommand(new LanguageCommand());
            RegisterCommand(new ScriptCommand());
            RegisterCommand(new ToggleSuggestionsCommand());
            RegisterCommand(new AliasCommand());
            RegisterCommand(new ScanCommand(_extensionsPath));
            RegisterCommand(new HistoryCommand());
            RegisterCommand(new VersionCommand(this));
        }

        private async Task LoadExternalCommandsAsync()
        {
            var csFiles = Directory.GetFiles(_extensionsPath, "*.cs");
            var csxFiles = Directory.GetFiles(_extensionsPath, "*.csx")
                .Where(f => !f.EndsWith(".disable"));
            var allFiles = csFiles.Concat(csxFiles).Distinct();

            foreach (var file in allFiles)
            {
                try
                {
                    var commands = await CompileAndLoadCommandsAsync(file);
                    foreach (var command in commands)
                    {
                        RegisterCommand(command);
                        MetadataCacheService.UpdateCache(file, new CommandMetadata
                        {
                            Name = command.Name,
                            Description = command.Description,
                            Aliases = command.Aliases.ToArray(),
                            Author = command.Author,
                            Version = command.Version,
                            FilePath = file
                        });
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelperService.WriteError("Error loading command: {0}", ex.Message);
                }
            }

            MetadataCacheService.Save();
        }

        private async Task<IEnumerable<ICommand>> CompileAndLoadCommandsAsync(string filePath)
        {
            var code = await File.ReadAllTextAsync(filePath);
            var isScript = Path.GetExtension(filePath).Equals(".csx", StringComparison.OrdinalIgnoreCase);

            if (isScript)
            {
                if (ScriptCacheService.TryGetScript(filePath, out var cachedScript))
                {
                    try
                    {
                        var result = await cachedScript.RunAsync(new CommandGlobals(_extensionsPath) { Manager = this });
                        if (result.Exception != null) throw result.Exception;
                        var command = result.ReturnValue as ICommand;
                        return command != null ? new[] { command } : Enumerable.Empty<ICommand>();
                    }
                    catch
                    {
                        ScriptCacheService.Clear();
                        return await LoadFromScriptAsync(code, filePath);
                    }
                }

                return await LoadFromScriptAsync(code, filePath);
            }
            else
            {
                if (ScriptCacheService.TryGetAssembly(filePath, out var cachedAssembly))
                {
                    return cachedAssembly.GetTypes()
                        .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                        .Select(type => (ICommand)Activator.CreateInstance(type));
                }

                var commands = await LoadFromClassFileAsync(code, filePath);
                return commands;
            }
        }

        private async Task<IEnumerable<ICommand>> LoadFromScriptAsync(string code, string filePath)
        {
            try
            {
                var defaultUsings = new[]
                {
                    "Mugs.Interfaces",
                    "Mugs.Models",
                    "Mugs.Services",
                    "System",
                    "System.IO",
                    "System.Linq",
                    "System.Collections",
                    "System.Collections.Generic",
                    "System.Threading",
                    "System.Threading.Tasks",
                    "System.Text",
                    "System.Text.RegularExpressions",
                    "System.Net",
                    "System.Net.Http",
                    "System.Dynamic",
                    "System.Xml",
                    "System.Xml.Linq"
                };

                var defaultAssemblies = new[]
                {
                    typeof(object).Assembly,
                    typeof(Enumerable).Assembly,
                    typeof(System.ComponentModel.Component).Assembly,
                    typeof(System.Diagnostics.Process).Assembly,
                    typeof(System.Dynamic.DynamicObject).Assembly,
                    typeof(System.IO.File).Assembly,
                    typeof(System.Net.WebClient).Assembly,
                    typeof(System.Text.RegularExpressions.Regex).Assembly,
                    typeof(System.Xml.XmlDocument).Assembly,
                    Assembly.GetExecutingAssembly()
                };

                var processedCode = ProcessScriptCode(code, filePath, defaultUsings);

                if (ScriptCacheService.TryGetScript(filePath, out var cachedScript))
                {
                    try
                    {
                        var scriptResult = await cachedScript.RunAsync(new CommandGlobals(_extensionsPath) { Manager = this });
                        if (scriptResult.Exception != null) throw scriptResult.Exception;
                        var cmd = scriptResult.ReturnValue as ICommand;
                        return cmd != null ? new[] { cmd } : Enumerable.Empty<ICommand>();
                    }
                    catch
                    {
                        ScriptCacheService.Clear();
                    }
                }

                var scriptOptions = ScriptOptions.Default
                    .WithReferences(defaultAssemblies)
                    .WithImports(defaultUsings);

                var script = CSharpScript.Create(processedCode, scriptOptions, typeof(CommandGlobals));

                var compilation = script.GetCompilation();
                var diagnostics = compilation.GetDiagnostics()
                    .Where(d => d.Severity == DiagnosticSeverity.Error)
                    .ToList();

                if (diagnostics.Any())
                {
                    throw new InvalidOperationException(
                        string.Join(Environment.NewLine, diagnostics.Select(d => d.GetMessage())));
                }

                ScriptCacheService.AddScript(filePath, script);

                var result = await script.RunAsync(new CommandGlobals(_extensionsPath) { Manager = this });

                if (result.Exception != null)
                {
                    throw result.Exception;
                }

                var command = result.ReturnValue as ICommand;
                return command != null ? new[] { command } : Enumerable.Empty<ICommand>();
            }
            catch (CompilationErrorException ex)
            {
                ConsoleHelperService.WriteError($"Script compilation error: {string.Join(Environment.NewLine, ex.Diagnostics)}");
                return Enumerable.Empty<ICommand>();
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError($"Script execution error in '{Path.GetFileName(filePath)}': {ex.Message}");
                return Enumerable.Empty<ICommand>();
            }
        }

        private string ProcessScriptCode(string code, string filePath, string[] defaultUsings)
        {
            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new StringBuilder();
            var loadedScripts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var existingUsings = new HashSet<string>();

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("using ") && line.Contains(";"))
                {
                    var usingDirective = line.Trim();
                    existingUsings.Add(usingDirective.Substring(6, usingDirective.Length - 7).Trim());
                }
            }

            foreach (var usingNamespace in defaultUsings)
            {
                if (!existingUsings.Contains(usingNamespace))
                {
                    result.AppendLine($"using {usingNamespace};");
                }
            }

            if (defaultUsings.Length > existingUsings.Count)
            {
                result.AppendLine();
            }

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("#load ", StringComparison.OrdinalIgnoreCase))
                {
                    var scriptName = line.Substring(line.IndexOf('"') + 1);
                    scriptName = scriptName.Substring(0, scriptName.IndexOf('"'));

                    if (!loadedScripts.Contains(scriptName))
                    {
                        var scriptPath = Path.Combine(Path.GetDirectoryName(filePath), scriptName);
                        if (File.Exists(scriptPath))
                        {
                            var scriptCode = File.ReadAllText(scriptPath);
                            result.AppendLine(ProcessScriptCode(scriptCode, scriptPath, defaultUsings));
                            loadedScripts.Add(scriptName);
                        }
                        else
                        {
                            throw new FileNotFoundException($"Script file not found: {scriptName}");
                        }
                    }
                }
                else if (!line.TrimStart().StartsWith("using ") || !defaultUsings.Contains(line.Trim().Substring(6, line.Trim().Length - 7).Trim()))
                {
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        private string ProcessLoadDirectives(string code, string currentFilePath)
        {
            var lines = code.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new StringBuilder();
            var loadedScripts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var defaultUsings = new[]
            {
                "using Mugs.Interfaces",
                "using Mugs.Models",
                "using Mugs.Services",
                "using System;",
                "using System.IO;",
                "using System.Linq;",
                "using System.Collections;",
                "using System.Collections.Generic;",
                "using System.Threading;",
                "using System.Threading.Tasks;",
                "using System.Text;",
                "using System.Text.RegularExpressions;",
                "using System.Net;",
                "using System.Net.Http;",
                "using System.Dynamic;",
                "using System.Xml;",
                "using System.Xml.Linq;"
            };

            var existingUsings = new HashSet<string>();
            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("using ") && line.Contains(";"))
                {
                    existingUsings.Add(line.Trim());
                }
            }

            foreach (var usingLine in defaultUsings)
            {
                if (!existingUsings.Contains(usingLine))
                {
                    result.AppendLine(usingLine);
                }
            }

            if (defaultUsings.Length > existingUsings.Count)
            {
                result.AppendLine();
            }

            foreach (var line in lines)
            {
                if (line.TrimStart().StartsWith("#load ", StringComparison.OrdinalIgnoreCase))
                {
                    var scriptName = line.Substring(line.IndexOf('"') + 1);
                    scriptName = scriptName.Substring(0, scriptName.IndexOf('"'));

                    if (!loadedScripts.Contains(scriptName))
                    {
                        var scriptPath = Path.Combine(Path.GetDirectoryName(currentFilePath), scriptName);
                        if (File.Exists(scriptPath))
                        {
                            var scriptCode = File.ReadAllText(scriptPath);
                            result.AppendLine(ProcessLoadDirectives(scriptCode, scriptPath));
                            loadedScripts.Add(scriptName);
                        }
                        else
                        {
                            throw new FileNotFoundException($"Script file not found: {scriptName}");
                        }
                    }
                }
                else if (!line.TrimStart().StartsWith("using ") || !defaultUsings.Contains(line.Trim()))
                {
                    result.AppendLine(line);
                }
            }

            return result.ToString();
        }

        private async Task<IEnumerable<ICommand>> LoadFromClassFileAsync(string code, string filePath)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var references = new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(ICommand).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Task).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location)
            };

            var compilation = CSharpCompilation.Create(
                "DynamicCommands",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            var emitResult = compilation.Emit(ms);

            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine, emitResult.Diagnostics.Select(d => d.GetMessage())));
            }

            ms.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(ms.ToArray());

            ScriptCacheService.AddAssembly(filePath, assembly);

            return assembly.GetTypes()
                .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                .Select(type => (ICommand)Activator.CreateInstance(type));
        }

        public void RegisterCommand(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commands[command.Name.ToLowerInvariant()] = command;

            foreach (var alias in command.Aliases ?? Enumerable.Empty<string>())
            {
                _commands[alias.ToLowerInvariant()] = command;
            }
        }

        public ICommand GetCommand(string name)
        {
            var commandName = name.ToLowerInvariant();

            if (AliasManagerService.GetCommandName(commandName) is string resolvedName)
            {
                commandName = resolvedName;
            }

            return _commands.TryGetValue(commandName, out var command) ? command : null;
        }

        public bool IsValidCommand(string input)
        {
            var commandName = input.Split(' ').FirstOrDefault();
            return !string.IsNullOrEmpty(commandName) && _commands.ContainsKey(commandName.ToLowerInvariant());
        }

        public IEnumerable<ICommand> GetAllCommands() => _commands.Values
            .GroupBy(c => c.Name)
            .Select(g => g.First())
            .OrderBy(c => c.Name);

        public IEnumerable<string> GetCommandNamesStartingWith(string prefix)
        {
            return _commands.Keys
                .Where(cmd => cmd.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd);
        }

        public string GetCommandSuggestion(string prefix)
        {
            var commands = GetCommandNamesStartingWith(prefix).ToList();
            if (commands.Count == 0) return null;
            var firstMatch = commands.First();
            return firstMatch.Length > prefix.Length
                ? firstMatch.Substring(prefix.Length)
                : null;
        }
    }
}