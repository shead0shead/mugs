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
            public bool AutoUpdateCheck { get; set; } = true;
            public int AutoUpdateInterval { get; set; } = 24;
            public DateTime LastUpdateCheck { get; set; } = DateTime.MinValue;
            public bool EnableSpinnerAnimation { get; set; } = true;
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

        public static bool AutoCheckEnabled
        {
            get => _currentSettings.AutoUpdateCheck;
            set
            {
                _currentSettings.AutoUpdateCheck = value;
                SaveSettings();
            }
        }

        public static int AutoCheckIntervalHours
        {
            get => _currentSettings.AutoUpdateInterval;
            set
            {
                _currentSettings.AutoUpdateInterval = value;
                SaveSettings();
            }
        }

        public static DateTime LastUpdateCheck
        {
            get => _currentSettings.LastUpdateCheck;
            set
            {
                _currentSettings.LastUpdateCheck = value;
                SaveSettings();
            }
        }

        public static bool EnableSpinnerAnimation
        {
            get => _currentSettings.EnableSpinnerAnimation;
            set
            {
                _currentSettings.EnableSpinnerAnimation = value;
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