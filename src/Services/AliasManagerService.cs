// Mugs/Services/AliasManagerService.cs

using Newtonsoft.Json;

namespace Mugs.Services
{
    public static class AliasManagerService
    {
        private const string AliasFile = "aliases.json";
        private static Dictionary<string, string> _aliases = new();

        public static void Initialize()
        {
            if (File.Exists(AliasFile))
            {
                try
                {
                    var json = File.ReadAllText(AliasFile);
                    _aliases = JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
                              ?? new Dictionary<string, string>();
                }
                catch
                {
                    _aliases = new Dictionary<string, string>();
                }
            }
        }

        public static void AddAlias(string commandName, string alias)
        {
            _aliases[alias.ToLowerInvariant()] = commandName.ToLowerInvariant();
            SaveAliases();
        }

        public static bool RemoveAlias(string alias)
        {
            bool removed = _aliases.Remove(alias.ToLowerInvariant());
            if (removed) SaveAliases();
            return removed;
        }

        public static string GetCommandName(string alias)
        {
            return _aliases.TryGetValue(alias.ToLowerInvariant(), out var cmd) ? cmd : null;
        }

        public static Dictionary<string, string> GetAllAliases()
        {
            return new Dictionary<string, string>(_aliases);
        }

        private static void SaveAliases()
        {
            File.WriteAllText(AliasFile, JsonConvert.SerializeObject(_aliases, Formatting.Indented));
        }
    }
}