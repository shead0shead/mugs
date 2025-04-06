// Mugs/Services/LocalizationService.cs

using Mugs.Models;

using System.Globalization;
using Newtonsoft.Json;

namespace Mugs.Services
{
    public static class LocalizationService
    {
        private static Dictionary<string, string> _currentLanguage = new Dictionary<string, string>();
        private static Dictionary<string, LanguageData> _allLanguages = new Dictionary<string, LanguageData>();
        private static string _currentLanguageCode = "en";
        private const string LanguagesFolder = "Languages";

        public static void Initialize()
        {
            Directory.CreateDirectory(LanguagesFolder);
            LoadAllLanguages();

            var savedLanguage = AppSettings.Language;
            var systemLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower();

            if (!string.IsNullOrEmpty(savedLanguage))
            {
                SetLanguage(savedLanguage);
            }
            else if (_allLanguages.ContainsKey(systemLanguage))
            {
                SetLanguage(systemLanguage);
                AppSettings.Language = systemLanguage;
            }
            else
            {
                SetLanguage("en");
                AppSettings.Language = "en";
            }
        }

        public static void LoadAllLanguages()
        {
            _allLanguages.Clear();

            foreach (var file in Directory.GetFiles(LanguagesFolder, "*.json"))
            {
                try
                {
                    var languageCode = Path.GetFileNameWithoutExtension(file);
                    var json = File.ReadAllText(file);
                    var languageData = JsonConvert.DeserializeObject<LanguageData>(json);

                    if (languageData?.Translations != null)
                    {
                        _allLanguages[languageCode] = languageData;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleHelperService.WriteError("Error loading language file {0}: {1}", Path.GetFileName(file), ex.Message);
                }
            }

            if (!_allLanguages.ContainsKey("en"))
            {
                _allLanguages["en"] = new LanguageData
                {
                    LanguageName = "English",
                    Translations = CreateDefaultEnglishTranslations()
                };
            }
        }

        private static Dictionary<string, string> CreateDefaultEnglishTranslations()
        {
            return new Dictionary<string, string>
            {
                // General
                ["app_title"] = "Mugs",
                ["welcome_message"] = "Console application with dynamic command loading\nType 'help' for command list or 'exit' to quit",
                ["checking_updates"] = "Checking for updates...",
                ["update_available"] = "Update available {0} (current version {1})\nDownload: {2}\nDescription: {3}\nTo install type: update install",
                ["no_update_available"] = "You have the latest version {0}",
                ["update_error"] = "Error checking for updates:\n{0}",
                ["update_success"] = "Application successfully updated!",
                ["command_not_found"] = "Command '{0}' not found. Type 'help' for command list",
                ["command_error"] = "Command execution error: {0}",
                ["exit_confirmation"] = "Are you sure you want to exit? (y/n)",
                ["invalid_input"] = "Invalid input",

                // Help command
                ["builtin_commands"] = "Built-in commands:",
                ["verified_commands"] = "Verified commands (✅ safe):",
                ["external_commands"] = "Third-party commands (use with caution):",
                ["command_help"] = "For detailed help type: help <command>",
                ["help_command"] = "help",
                ["help_description"] = "Shows command help",
                ["help_usage"] = "help update, help new",

                // Language command
                ["language_description"] = "Sets or shows the current language",
                ["current_language"] = "Current language: {0}",
                ["available_languages"] = "Available languages: {0}",
                ["language_changed"] = "Language changed to {0}",
                ["invalid_language"] = "Invalid language code: {0}",
                ["language_usage"] = "language en\nlanguage ru",

                // List command
                ["list_description"] = "Lists all available commands and their status",
                ["available_commands"] = "Available commands:",
                ["disabled_extensions"] = "Disabled extensions:",
                ["example"] = "Usage example",
                ["enable_usage"] = "To enable use: enable <command_name>",
                ["verified"] = "Verified",

                // Reload command
                ["reload_description"] = "Reloads all commands from files",
                ["reloading_commands"] = "Reloading commands...",
                ["commands_reloaded"] = "Commands successfully reloaded",

                // Clear command
                ["clear_description"] = "Clears the console",

                // Restart command
                ["restart_description"] = "Completely restarts the application",
                ["restarting"] = "Restarting application...",

                // Time command
                ["time_description"] = "Shows current time",
                ["current_time"] = "Current time: {0}",

                // Update command
                ["update_description"] = "Checks for and installs application updates",
                ["confirm_update"] = "Are you sure you want to install the update? (y/n)",
                ["update_cancelled"] = "Update cancelled",
                ["starting_update"] = "Starting update process...",
                ["downloading_update"] = "Downloading update...",
                ["extracting_update"] = "Extracting update...",
                ["creating_backup"] = "Creating backup...",
                ["installing_update"] = "Installing update...",
                ["finishing_update"] = "Finishing installation...",
                ["update_failed"] = "Error installing update: {0}",

                // New command
                ["new_description"] = "Creates a new extension script template in Extensions folder",
                ["missing_command_name"] = "Specify command name (e.g.: new mycommand)",
                ["file_exists"] = "File {0} already exists!",
                ["template_created"] = "Command template created: {0}",
                ["reload_usage"] = "To use execute: reload",

                // Enable/Disable commands
                ["enable_description"] = "Enables a disabled extension",
                ["disable_description"] = "Disables an extension",
                ["missing_extension_name"] = "Specify command name or extension file to enable (e.g.: enable mycommand or enable myextension.csx.disable)",
                ["extension_not_found"] = "File '{0}' not found",
                ["multiple_extensions"] = "Found multiple disabled extensions for command '{0}':",
                ["specify_filename"] = "Specify filename to enable",
                ["no_disabled_extensions"] = "No disabled extensions found for command/file '{0}'",
                ["extension_enabled"] = "Extension '{0}' enabled",
                ["extension_disabled"] = "Extension '{0}' disabled",
                ["command_not_found_disable"] = "Command/file '{0}' not found",

                // Import command
                ["import_description"] = "Downloads and installs an extension from the specified URL",
                ["missing_url"] = "Specify extension URL to download (e.g.: import https://example.com/extension.csx)",
                ["downloading_extension"] = "Downloading extension from URL: {0}",
                ["extension_downloaded"] = "Extension successfully downloaded: {0}",
                ["download_error"] = "Error downloading extension: {0}",

                // Debug command
                ["debug_description"] = "Runs a command in debug mode",
                ["missing_debug_command"] = "Specify command to debug (e.g.: debug mycommand --args \"test\")",
                ["debug_start"] = "Running {0} with arguments: {1}",
                ["debug_vars"] = "Variables: args = {0}",
                ["debug_completed"] = "Command completed in {0} ms",
                ["debug_error"] = "Execution error: {0}: {1}",

                // Command details
                ["command"] = "Command",
                ["description"] = "Description",
                ["aliases"] = "Aliases",
                ["author"] = "Author",
                ["version"] = "Version",
                ["usage_examples"] = "Usage examples",
                ["verification"] = "Verification",
                ["verified_safe"] = "This command is verified and safe",

                // Script command
                ["script_description"] = "Executes commands from a text file",
                ["missing_script_file"] = "Specify script file to execute (e.g.: script commands.txt)",
                ["script_file_not_found"] = "Script file '{0}' not found",
                ["executing_command"] = "Executing: {0}",
                ["command_output"] = "Command output: {0}",
                ["script_completed"] = "Script execution completed",
                ["script_error"] = "Script execution error: {0}",

                // Toggle suggestions
                ["toggle_suggestions"] = "Toggles command suggestions display",
                ["suggestions_enabled"] = "Command suggestions enabled",
                ["suggestions_disabled"] = "Command suggestions disabled",

                // Command Metadata Cashe
                ["cache_save_error"] = "Error saving metadata cache: {0}",
                ["command_requires_recompile"] = "Command '{0}' requires recompilation. Use 'reload' command",
                ["cache_cleared"] = "Metadata cache cleared",

                // Alias command
                ["alias_description"] = "Manage command aliases",
                ["alias_usage"] = "alias add <command> <alias>, alias remove <alias>, alias list",
                ["alias_no_aliases"] = "No custom aliases defined",
                ["alias_header"] = "Custom aliases:",
                ["alias_added"] = "Alias '{0}' added for command '{1}'",
                ["alias_removed"] = "Alias '{0}' removed",
                ["alias_not_found"] = "Alias not found",
                ["alias_invalid_syntax"] = "Invalid alias command syntax",

                // Scan command
                ["scan_description"] = "Scans script for potentially dangerous code",
                ["scan_missing_file"] = "Specify script file to scan (e.g.: scan mycommand.csx)",
                ["scan_file_not_found"] = "File '{0}' not found",
                ["scan_issues_found"] = "Potential security issues found in {0}:",
                ["scan_no_issues"] = "No dangerous code patterns found in {0}",
                ["scan_total_issues"] = "Total issues found: {0}",
                ["scan_error"] = "Scan error: {0}",
                ["full_path_display"] = "Full path: {0}",

                // History command
                ["history_description"] = "Shows command history or searches in history",
                ["history_showing"] = "Showing last {0} commands:",
                ["history_search_results"] = "Search results for \"{0}\":",
                ["history_no_results"] = "No commands found matching \"{0}\"",
                ["history_invalid_count"] = "Invalid count specified. Using default value.",
                ["history_full"] = "Full command history:",

                // Version command
                ["version_description"] = "Shows application version and information",
                ["application"] = "Application",
                ["repo"] = "Repo",
                ["commands"] = "Commands",
                ["extensions"] = "Extensions",
                ["available"] = "available",
                ["loaded"] = "loaded",

                // Settings
                ["verified_load_error"] = "Error loading verified hashes: {0}",
                ["settings_error"] = "Error saving settings: {0}"
            };
        }

        public static void SetLanguage(string languageCode)
        {
            if (_allLanguages.TryGetValue(languageCode, out var languageData))
            {
                _currentLanguage = languageData.Translations;
                _currentLanguageCode = languageCode;
            }
            else
            {
                ConsoleHelperService.WriteError("Language '{0}' not found. Using default 'en'", languageCode);
                _currentLanguage = _allLanguages["en"].Translations;
                _currentLanguageCode = "en";
            }
        }

        public static string GetString(string key, params object[] args)
        {
            if (_currentLanguage.TryGetValue(key, out var value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }

            if (_allLanguages.TryGetValue("en", out var english) && english.Translations.TryGetValue(key, out var englishValue))
            {
                return args.Length > 0 ? string.Format(englishValue, args) : englishValue;
            }

            return key;
        }

        public static string GetLanguageName(string languageCode)
        {
            return _allLanguages.TryGetValue(languageCode, out var languageData)
                ? languageData.LanguageName
                : languageCode.ToUpper();
        }

        public static IEnumerable<string> GetAvailableLanguages()
        {
            return _allLanguages.Keys.OrderBy(k => k);
        }

        public static string CurrentLanguage => _currentLanguageCode;
    }
}