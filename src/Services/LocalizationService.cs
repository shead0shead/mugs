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
            CreateDefaultLanguageFiles();
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

        private static void CreateDefaultLanguageFiles()
        {
            var enPath = Path.Combine(LanguagesFolder, "en.json");
            var ruPath = Path.Combine(LanguagesFolder, "ru.json");

            if (!File.Exists(enPath))
            {
                var enTranslations = CreateDefaultEnglishTranslations();
                var enData = new LanguageData
                {
                    LanguageName = "English",
                    Translations = enTranslations
                };
                File.WriteAllText(enPath, JsonConvert.SerializeObject(enData, Formatting.Indented));
            }

            if (!File.Exists(ruPath))
            {
                var ruTranslations = CreateDefaultRussianTranslations();
                var ruData = new LanguageData
                {
                    LanguageName = "Русский",
                    Translations = ruTranslations
                };
                File.WriteAllText(ruPath, JsonConvert.SerializeObject(ruData, Formatting.Indented));
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
                    OutputService.WriteError($"Error loading language file {Path.GetFileName(file)}: {ex.Message}");
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
                ["update_status"] = "Update status:",
                ["current_version"] = "Current version",
                ["latest_version"] = "Latest version",
                ["auto_update_status"] = "Auto-update",
                ["check_interval"] = "Check interval",
                ["last_check_time"] = "Last check time",
                ["hours"] = "hours",
                ["download_progress"] = "Downloading {0}: {1}% complete",
                ["update_manifest_error"] = "Error getting update information",
                ["critical_update_warning"] = "⚠️ This is a critical update containing important security fixes!",
                ["auto_update_enabled"] = "Automatic update checking enabled",
                ["auto_update_disabled"] = "Automatic update checking disabled",
                ["auto_update_interval_set"] = "Update check interval set to {0} hours",
                ["invalid_interval"] = "Invalid interval (must be positive number)",
                ["invalid_auto_update_command"] = "Invalid auto-update command",
                ["update_invalid_command"] = "Invalid update command. Use: install|check|settings",

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

                // Logging command
                ["logging_description"] = "Enables or disables console logging output",
                ["logging_state"] = "Console logging is currently {0}",
                ["logging_enabled"] = "Console logging enabled",
                ["logging_disabled"] = "Console logging disabled",
                ["logging_invalid_arg"] = "Invalid argument. Use 'on' or 'off'",

                // Settings
                ["verified_load_error"] = "Error loading verified hashes: {0}",
                ["settings_error"] = "Error saving settings: {0}"
            };
        }

        private static Dictionary<string, string> CreateDefaultRussianTranslations()
        {
            return new Dictionary<string, string>
            {
                // Общие
                ["app_title"] = "Mugs",
                ["welcome_message"] = "Консольное приложение с динамической загрузкой команд\nВведите 'help' для списка команд или 'exit' для выхода",
                ["checking_updates"] = "Проверка обновлений...",
                ["update_available"] = "Доступно обновление {0} (текущая версия {1})\nСкачать: {2}\nОписание: {3}\nДля установки введите: update install",
                ["no_update_available"] = "У вас последняя версия {0}",
                ["update_error"] = "Ошибка при проверке обновлений:\n{0}",
                ["update_success"] = "Приложение успешно обновлено!",
                ["command_not_found"] = "Команда '{0}' не найдена. Введите 'help' для списка команд",
                ["command_error"] = "Ошибка выполнения команды: {0}",
                ["exit_confirmation"] = "Вы уверены, что хотите выйти? (y/n)",
                ["invalid_input"] = "Неверный ввод",

                // Команда help
                ["builtin_commands"] = "Встроенные команды:",
                ["verified_commands"] = "Проверенные команды (✅ безопасные):",
                ["external_commands"] = "Сторонние команды (используйте с осторожностью):",
                ["command_help"] = "Для подробной справки введите: help <команда>",
                ["help_command"] = "help",
                ["help_description"] = "Показывает справку по командам",
                ["help_usage"] = "help update, help new",

                // Команда language
                ["language_description"] = "Устанавливает или показывает текущий язык",
                ["current_language"] = "Текущий язык: {0}",
                ["available_languages"] = "Доступные языки: {0}",
                ["language_changed"] = "Язык изменен на {0}",
                ["invalid_language"] = "Неверный код языка: {0}",
                ["language_usage"] = "language en\nlanguage ru",

                // Команда list
                ["list_description"] = "Показывает все доступные команды и их статус",
                ["available_commands"] = "Доступные команды:",
                ["disabled_extensions"] = "Отключенные расширения:",
                ["example"] = "Пример использования",
                ["enable_usage"] = "Для включения используйте: enable <имя_команды>",
                ["verified"] = "Проверено",

                // Команда reload
                ["reload_description"] = "Перезагружает все команды из файлов",
                ["reloading_commands"] = "Перезагрузка команд...",
                ["commands_reloaded"] = "Команды успешно перезагружены",

                // Команда clear
                ["clear_description"] = "Очищает консоль",

                // Команда restart
                ["restart_description"] = "Полностью перезапускает приложение",
                ["restarting"] = "Перезапуск приложения...",

                // Команда time
                ["time_description"] = "Показывает текущее время",
                ["current_time"] = "Текущее время: {0}",

                // Команда update
                ["update_description"] = "Проверяет и устанавливает обновления приложения",
                ["confirm_update"] = "Вы уверены, что хотите установить обновление? (y/n)",
                ["update_cancelled"] = "Обновление отменено",
                ["starting_update"] = "Начало процесса обновления...",
                ["downloading_update"] = "Загрузка обновления...",
                ["extracting_update"] = "Распаковка обновления...",
                ["creating_backup"] = "Создание резервной копии...",
                ["installing_update"] = "Установка обновления...",
                ["finishing_update"] = "Завершение установки...",
                ["update_failed"] = "Ошибка установки обновления: {0}",
                ["update_status"] = "Статус обновления:",
                ["current_version"] = "Текущая версия",
                ["latest_version"] = "Последняя версия",
                ["auto_update_status"] = "Автообновления",
                ["check_interval"] = "Интервал проверки",
                ["last_check_time"] = "Время последней проверки",
                ["hours"] = "часов",
                ["download_progress"] = "Загрузка {0}: {1}% завершено",
                ["update_manifest_error"] = "Ошибка получения информации об обновлении",
                ["critical_update_warning"] = "⚠️ Это критическое обновление содержит важные исправления безопасности!",
                ["auto_update_enabled"] = "Автоматическая проверка обновлений включена",
                ["auto_update_disabled"] = "Автоматическая проверка обновлений отключена",
                ["auto_update_interval_set"] = "Интервал проверки обновлений установлен: {0} часов",
                ["invalid_interval"] = "Недопустимый интервал (должен быть положительным числом)",
                ["invalid_auto_update_command"] = "Недопустимая команда автообновления",
                ["update_invalid_command"] = "Недопустимая команда обновления. Используйте: install|check|settings",

                // Команда new
                ["new_description"] = "Создает шаблон скрипта дополнения в папке Extensions",
                ["missing_command_name"] = "Укажите имя команды (например: new mycommand)",
                ["file_exists"] = "Файл {0} уже существует!",
                ["template_created"] = "Шаблон команды создан: {0}",
                ["reload_usage"] = "Для использования выполните: reload",

                // Команды enable/disable
                ["enable_description"] = "Включает отключенное расширение",
                ["disable_description"] = "Отключает расширение",
                ["missing_extension_name"] = "Укажите имя команды или файла расширения для включения (например: enable mycommand или enable myextension.csx.disable)",
                ["extension_not_found"] = "Файл '{0}' не найден",
                ["multiple_extensions"] = "Найдено несколько отключенных расширений для команды '{0}':",
                ["specify_filename"] = "Укажите имя файла для включения",
                ["no_disabled_extensions"] = "Не найдено отключенных расширений для команды/файла '{0}'",
                ["extension_enabled"] = "Расширение '{0}' включено",
                ["extension_disabled"] = "Расширение '{0}' отключено",
                ["command_not_found_disable"] = "Команда/файл '{0}' не найдена",

                // Команда import
                ["import_description"] = "Загружает и устанавливает расширение из указанного URL",
                ["missing_url"] = "Укажите URL расширения для загрузки (например: import https://example.com/extension.csx)",
                ["downloading_extension"] = "Загрузка расширения из URL: {0}",
                ["extension_downloaded"] = "Расширение успешно загружено: {0}",
                ["download_error"] = "Ошибка загрузки расширения: {0}",

                // Команда debug
                ["debug_description"] = "Запускает команду в режиме отладки",
                ["missing_debug_command"] = "Укажите команду для отладки (например: debug mycommand --args \"test\")",
                ["debug_start"] = "Запуск {0} с аргументами: {1}",
                ["debug_vars"] = "Переменные: args = {0}",
                ["debug_completed"] = "Команда выполнена за {0} мс",
                ["debug_error"] = "Ошибка выполнения: {0}: {1}",

                // Детали команды
                ["command"] = "Команда",
                ["description"] = "Описание",
                ["aliases"] = "Псевдонимы",
                ["author"] = "Автор",
                ["version"] = "Версия",
                ["usage_examples"] = "Примеры использования",
                ["verification"] = "Проверка",
                ["verified_safe"] = "Эта команда проверена и безопасна",

                // Команда script
                ["script_description"] = "Выполняет команды из текстового файла",
                ["missing_script_file"] = "Укажите файл скрипта для выполнения (например: script commands.txt)",
                ["script_file_not_found"] = "Файл скрипта '{0}' не найден",
                ["executing_command"] = "Выполнение: {0}",
                ["command_output"] = "Вывод команды: {0}",
                ["script_completed"] = "Выполнение скрипта завершено",
                ["script_error"] = "Ошибка выполнения скрипта: {0}",

                // Переключение подсказок
                ["toggle_suggestions"] = "Переключает отображение подсказок команд",
                ["suggestions_enabled"] = "Подсказки команд включены",
                ["suggestions_disabled"] = "Подсказки команд отключены",

                // Кэш метаданных команд
                ["cache_save_error"] = "Ошибка сохранения кэша метаданных: {0}",
                ["command_requires_recompile"] = "Команда '{0}' требует перекомпиляции. Используйте команду 'reload'",
                ["cache_cleared"] = "Кэш метаданных очищен",

                // Команда alias
                ["alias_description"] = "Управление псевдонимами команд",
                ["alias_usage"] = "alias add <команда> <псевдоним>, alias remove <псевдоним>, alias list",
                ["alias_no_aliases"] = "Пользовательские псевдонимы не определены",
                ["alias_header"] = "Пользовательские псевдонимы:",
                ["alias_added"] = "Псевдоним '{0}' добавлен для команды '{1}'",
                ["alias_removed"] = "Псевдоним '{0}' удален",
                ["alias_not_found"] = "Псевдоним не найден",
                ["alias_invalid_syntax"] = "Неверный синтаксис команды alias",

                // Команда scan
                ["scan_description"] = "Проверяет скрипт на потенциально опасный код",
                ["scan_missing_file"] = "Укажите файл скрипта для проверки (например: scan mycommand.csx)",
                ["scan_file_not_found"] = "Файл '{0}' не найден",
                ["scan_issues_found"] = "Найдены потенциальные проблемы безопасности в {0}:",
                ["scan_no_issues"] = "Опасных шаблонов кода не найдено в {0}",
                ["scan_total_issues"] = "Всего найдено проблем: {0}",
                ["scan_error"] = "Ошибка проверки: {0}",
                ["full_path_display"] = "Полный путь: {0}",

                // Команда history
                ["history_description"] = "Показывает историю команд или выполняет поиск по ней",
                ["history_showing"] = "Показаны последние {0} команд:",
                ["history_search_results"] = "Результаты поиска для \"{0}\":",
                ["history_no_results"] = "Команды, соответствующие \"{0}\", не найдены",
                ["history_invalid_count"] = "Указано неверное количество. Используется значение по умолчанию.",
                ["history_full"] = "Полная история команд:",

                // Команда version
                ["version_description"] = "Показывает версию приложения и информацию",
                ["application"] = "Приложение",
                ["repo"] = "Репозиторий",
                ["commands"] = "Команды",
                ["extensions"] = "Расширения",
                ["available"] = "доступно",
                ["loaded"] = "загружено",

                // Команда logging
                ["logging_description"] = "Включает или отключает вывод логов в консоль",
                ["logging_state"] = "Вывод логов в консоль сейчас {0}",
                ["logging_enabled"] = "Вывод логов в консоль включен",
                ["logging_disabled"] = "Вывод логов в консоль отключен",
                ["logging_invalid_arg"] = "Неверный аргумент. Используйте 'on' или 'off'",

                // Настройки
                ["verified_load_error"] = "Ошибка загрузки проверенных хешей: {0}",
                ["settings_error"] = "Ошибка сохранения настроек: {0}"
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
                OutputService.WriteError($"Language '{languageCode}' not found. Using default 'en'");
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