// Mugs/Services/SharedDataService.cs

using System.Collections.Concurrent;
using System.Reflection;

namespace Mugs.Services
{
    public static class SharedDataService
    {
        private static readonly ConcurrentDictionary<string, object> _data = new();
        private static readonly ConcurrentDictionary<string, Assembly> _loadedScripts = new();

        public static void Set(string key, object value) => _data[key] = value;
        public static T Get<T>(string key) => _data.TryGetValue(key, out var value) ? (T)value : default;
        public static bool Contains(string key) => _data.ContainsKey(key);

        public static void RegisterScript(string scriptName, Assembly assembly)
        {
            _loadedScripts[scriptName.ToLowerInvariant()] = assembly;
        }

        public static Assembly GetScriptAssembly(string scriptName)
        {
            _loadedScripts.TryGetValue(scriptName.ToLowerInvariant(), out var assembly);
            return assembly;
        }
    }
}