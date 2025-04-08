// Mugs/Services/MetadataCacheService.cs

using Mugs.Models;

using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Mugs.Services
{
    public static class MetadataCacheService
    {
        private const string CacheFile = "command_cache.json";
        private static readonly string CachePath = Path.Combine(AppContext.BaseDirectory, CacheFile);
        private static Dictionary<string, CommandMetadata> _cache = new();

        public static void Initialize()
        {
            if (File.Exists(CachePath))
            {
                try
                {
                    var json = File.ReadAllText(CachePath);
                    _cache = JsonConvert.DeserializeObject<Dictionary<string, CommandMetadata>>(json)
                        ?? new Dictionary<string, CommandMetadata>();
                }
                catch
                {
                    _cache = new Dictionary<string, CommandMetadata>();
                }
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_cache, Formatting.Indented);
                File.WriteAllText(CachePath, json);
            }
            catch (Exception ex)
            {
                OutputService.WriteError("cache_save_error", ex.Message);
            }
        }

        public static void Clear()
        {
            _cache.Clear();
            Save();
        }

        public static bool TryGetFromCache(string filePath, out CommandMetadata metadata)
        {
            var fileHash = CalculateFileHash(filePath);
            var lastModified = File.GetLastWriteTimeUtc(filePath);

            if (_cache.TryGetValue(filePath, out metadata) &&
                metadata.Hash == fileHash &&
                metadata.LastModified == lastModified)
            {
                return true;
            }
            return false;
        }

        public static void UpdateCache(string filePath, CommandMetadata metadata)
        {
            metadata.Hash = CalculateFileHash(filePath);
            metadata.LastModified = File.GetLastWriteTimeUtc(filePath);
            _cache[filePath] = metadata;
        }

        private static string CalculateFileHash(string filePath)
        {
            using var sha = SHA256.Create();
            using var stream = File.OpenRead(filePath);
            return Convert.ToBase64String(sha.ComputeHash(stream));
        }
    }
}