// Mugs/Services/ScriptCacheService.cs

using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;

namespace Mugs.Services
{
    public static class ScriptCacheService
    {
        private static readonly Dictionary<string, Script> _scriptCache = new();
        private static readonly Dictionary<string, Assembly> _assemblyCache = new();
        private static readonly object _lock = new();

        public static void AddScript(string filePath, Script script)
        {
            lock (_lock)
            {
                var normalizedPath = NormalizePath(filePath);
                _scriptCache[normalizedPath] = script;
            }
        }

        public static bool TryGetScript(string filePath, out Script script)
        {
            lock (_lock)
            {
                var normalizedPath = NormalizePath(filePath);
                return _scriptCache.TryGetValue(normalizedPath, out script);
            }
        }

        public static void AddAssembly(string filePath, Assembly assembly)
        {
            lock (_lock)
            {
                var normalizedPath = NormalizePath(filePath);
                _assemblyCache[normalizedPath] = assembly;
            }
        }

        public static bool TryGetAssembly(string filePath, out Assembly assembly)
        {
            lock (_lock)
            {
                var normalizedPath = NormalizePath(filePath);
                return _assemblyCache.TryGetValue(normalizedPath, out assembly);
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _scriptCache.Clear();
                _assemblyCache.Clear();
            }
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).ToLowerInvariant();
        }
    }
}