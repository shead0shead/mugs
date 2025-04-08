// Mugs/Models/CommandGlobals.cs

using Mugs.Services;

using System.Dynamic;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace Mugs.Models
{
    public class CommandGlobals
    {
        private readonly string _extensionsPath;

        public CommandGlobals(string extensionsPath)
        {
            _extensionsPath = extensionsPath;
        }

        public void Print(string message) => OutputService.WriteResponse(message);
        public void PrintError(string message) => OutputService.WriteError(message);
        public string ReadLine() => Console.ReadLine();
        public string Version => "1.0";

        public void DebugLog(string message) => OutputService.WriteDebug(message);
        public void DebugVar(string name, object value) => OutputService.WriteDebug($"{name} = {JsonConvert.SerializeObject(value)}");

        public CommandManager Manager { get; set; }

        public void SetSharedData(string key, object value) => SharedDataService.Set(key, value);
        public T GetSharedData<T>(string key) => SharedDataService.Get<T>(key);
        public bool HasSharedData(string key) => SharedDataService.Contains(key);

        public dynamic LoadScript(string scriptName)
        {
            var scriptPath = Path.Combine(_extensionsPath, scriptName);
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script file not found: {scriptName}");
            }

            var cachedAssembly = SharedDataService.GetScriptAssembly(scriptName);
            if (cachedAssembly != null)
            {
                return CreateScriptProxy(cachedAssembly);
            }

            var scriptCode = File.ReadAllText(scriptPath);
            var script = CSharpScript.Create(scriptCode,
                ScriptOptions.Default
                    .WithReferences(Assembly.GetExecutingAssembly())
                    .WithImports("System", "System.Collections.Generic"),
                typeof(CommandGlobals));

            var compilation = script.GetCompilation();
            var diagnostics = compilation.GetDiagnostics();
            if (diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine, diagnostics.Select(d => d.GetMessage())));
            }

            using var peStream = new MemoryStream();
            var emitResult = compilation.Emit(peStream);
            if (!emitResult.Success)
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine, emitResult.Diagnostics.Select(d => d.GetMessage())));
            }

            peStream.Seek(0, SeekOrigin.Begin);
            var assembly = Assembly.Load(peStream.ToArray());
            SharedDataService.RegisterScript(scriptName, assembly);

            return CreateScriptProxy(assembly);
        }

        private dynamic CreateScriptProxy(Assembly assembly)
        {
            dynamic proxy = new ExpandoObject();
            var proxyDict = (IDictionary<string, object>)proxy;

            foreach (var type in assembly.GetTypes().Where(t => t.IsPublic))
            {
                proxyDict[type.Name] = Activator.CreateInstance(type);
            }

            return proxy;
        }
    }
}