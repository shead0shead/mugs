// Mugs/Models/Settings.cs

using Mugs.Services;

using Newtonsoft.Json;

namespace Mugs.Models
{
    public static class AppSettings
    {
        private const string SettingsFile = "settings.json";
        private static Settings _currentSettings;

        public class Settings
        {
            public string Language { get; set; } = "en";
            public bool EnableSuggestions { get; set; } = true;
            public bool EnableConsoleLogging { get; set; } = false;
        }

        public static void Initialize()
        {
            if (File.Exists(SettingsFile))
            {
                try
                {
                    var json = File.ReadAllText(SettingsFile);
                    _currentSettings = JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
                catch
                {
                    _currentSettings = new Settings();
                }
            }
            else
            {
                _currentSettings = new Settings();
                SaveSettings();
            }
        }

        public static string Language
        {
            get => _currentSettings.Language;
            set
            {
                _currentSettings.Language = value;
                SaveSettings();
            }
        }

        public static bool EnableSuggestions
        {
            get => _currentSettings.EnableSuggestions;
            set
            {
                _currentSettings.EnableSuggestions = value;
                SaveSettings();
            }
        }

        public static bool EnableConsoleLogging
        {
            get => _currentSettings.EnableConsoleLogging;
            set
            {
                _currentSettings.EnableConsoleLogging = value;
                SaveSettings();
            }
        }

        private static void SaveSettings()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_currentSettings, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                OutputService.WriteError("settings_error", ex.Message);
            }
        }
    }
}