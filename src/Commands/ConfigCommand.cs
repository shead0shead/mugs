// Mugs/Commands/ConfigCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Mugs.Models;

using System.Text;
using System.Security.Cryptography;
using System.Text.Json;

namespace Mugs.Commands
{
    public class ConfigCommand : ICommand
    {
        private static readonly Dictionary<string, Action<string>> ConfigSetters = new()
        {
            // Core settings
            ["core.language"] = (value) => AppSettings.Language = value,
            ["core.enable_suggestions"] = (value) => AppSettings.EnableSuggestions = bool.Parse(value),
            ["core.enable_console_logging"] = (value) => AppSettings.EnableConsoleLogging = bool.Parse(value),
            ["core.auto_update_check"] = (value) => AppSettings.AutoCheckEnabled = bool.Parse(value),
            ["core.auto_update_interval"] = (value) => AppSettings.AutoCheckIntervalHours = int.Parse(value),
            ["core.enable_spinner_animation"] = (value) => AppSettings.EnableSpinnerAnimation = bool.Parse(value),
            ["core.command_history_limit"] = (value) => AppSettings.CommandHistoryLimit = int.Parse(value),
            ["core.prompt_style"] = (value) => AppSettings.PromptStyle = value,
            ["core.always_use_tabular_view"] = (value) => AppSettings.AlwaysUseTabularView = bool.Parse(value),
            ["core.enable_boxed_output"] = (value) => AppSettings.EnableBoxedOutput = bool.Parse(value),
            ["core.enable_boxed_output_title"] = (value) => AppSettings.EnableBoxedOutputTitle = bool.Parse(value),

            // Color settings
            ["color.response"] = (value) => AppSettings.ResponseColor = Enum.Parse<ConsoleColor>(value),
            ["color.error"] = (value) => AppSettings.ErrorColor = Enum.Parse<ConsoleColor>(value),
            ["color.warning"] = (value) => AppSettings.WarningColor = Enum.Parse<ConsoleColor>(value),
            ["color.success"] = (value) => AppSettings.SuccessColor = Enum.Parse<ConsoleColor>(value),
            ["color.info"] = (value) => AppSettings.InfoColor = Enum.Parse<ConsoleColor>(value),
            ["color.debug"] = (value) => AppSettings.DebugColor = Enum.Parse<ConsoleColor>(value),
        };

        private static readonly Dictionary<string, Func<string>> ConfigGetters = new()
        {
            // Core settings
            ["core.language"] = () => AppSettings.Language,
            ["core.enable_suggestions"] = () => AppSettings.EnableSuggestions.ToString(),
            ["core.enable_console_logging"] = () => AppSettings.EnableConsoleLogging.ToString(),
            ["core.auto_update_check"] = () => AppSettings.AutoCheckEnabled.ToString(),
            ["core.auto_update_interval"] = () => AppSettings.AutoCheckIntervalHours.ToString(),
            ["core.enable_spinner_animation"] = () => AppSettings.EnableSpinnerAnimation.ToString(),
            ["core.command_history_limit"] = () => AppSettings.CommandHistoryLimit.ToString(),
            ["core.prompt_style"] = () => AppSettings.PromptStyle,
            ["core.always_use_tabular_view"] = () => AppSettings.AlwaysUseTabularView.ToString(),
            ["core.enable_boxed_output"] = () => AppSettings.EnableBoxedOutput.ToString(),
            ["core.enable_boxed_output_title"] = () => AppSettings.EnableBoxedOutputTitle.ToString(),

            // Color settings
            ["color.response"] = () => AppSettings.ResponseColor.ToString(),
            ["color.error"] = () => AppSettings.ErrorColor.ToString(),
            ["color.warning"] = () => AppSettings.WarningColor.ToString(),
            ["color.success"] = () => AppSettings.SuccessColor.ToString(),
            ["color.info"] = () => AppSettings.InfoColor.ToString(),
            ["color.debug"] = () => AppSettings.DebugColor.ToString(),

            // Advanced settings
            ["app.version"] = () => UpdateCheckerService.CurrentVersion.ToString()
        };

        public string Name => "config";
        public string Description => LocalizationService.GetString("config_description");
        public IEnumerable<string> Aliases => new[] { "cfg", "settings" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => LocalizationService.GetString("config_usage");

        public Task ExecuteAsync(string[] args)
        {
            bool useTable = args.Length > 0 && args.Contains("--table");
            var cleanArgs = args.Where(a => a != "--table").ToArray();

            if (cleanArgs.Length == 0 || cleanArgs[0] == "list")
            {
                return useTable || AppSettings.AlwaysUseTabularView
                    ? ListConfigsAsTable()
                    : ListConfigs();
            }

            switch (cleanArgs[0].ToLower())
            {
                case "get" when cleanArgs.Length > 1:
                    return GetConfig(cleanArgs[1]);
                case "set" when cleanArgs.Length > 2:
                    return SetConfig(cleanArgs[1], string.Join(" ", cleanArgs.Skip(2)));
                case "export":
                    return ExportConfig(cleanArgs.Skip(1).ToArray());
                case "import":
                    return ImportConfig(cleanArgs.Skip(1).ToArray());
                default:
                    OutputService.WriteError("config_invalid_command");
                    return Task.CompletedTask;
            }
        }

        private Task ListConfigs()
        {
            var configGroups = new Dictionary<string, List<(string Key, string Value, bool ReadOnly)>>()
            {
                [LocalizationService.GetString("config_group_core")] = new(),
                [LocalizationService.GetString("config_group_color")] = new(),
                [LocalizationService.GetString("config_group_advanced")] = new(),
                [LocalizationService.GetString("config_group_system")] = new()
            };

            foreach (var key in ConfigGetters.Keys)
            {
                var group = key.StartsWith("color.") ? LocalizationService.GetString("config_group_color") :
                           key.StartsWith("advanced.") ? LocalizationService.GetString("config_group_advanced") :
                           key.StartsWith("app.") ? LocalizationService.GetString("config_group_system") :
                           LocalizationService.GetString("config_group_core");

                configGroups[group].Add((
                    key,
                    ConfigGetters[key](),
                    !ConfigSetters.ContainsKey(key)
                ));
            }

            var output = new StringBuilder();

            foreach (var group in configGroups)
            {
                if (group.Value.Count > 0)
                {
                    output.AppendLine(group.Key + ":");

                    foreach (var config in group.Value)
                    {
                        output.Append("  ")
                              .Append(config.Key.Replace($"{group.Key.Split(' ')[0].ToLower()}.", "").PadRight(30))
                              .Append(" = ")
                              .Append(config.Value);

                        if (config.ReadOnly)
                        {
                            output.Append(" ").Append("(" + LocalizationService.GetString("config_readonly").ToLower() + ")");
                        }

                        output.AppendLine();
                    }

                    output.AppendLine();
                }
            }

            OutputService.WriteResponse(output.ToString().TrimEnd());
            return Task.CompletedTask;
        }

        private Task ListConfigsAsTable()
        {
            var configGroups = new Dictionary<string, List<(string Key, string Value, bool ReadOnly)>>()
            {
                [LocalizationService.GetString("config_group_core")] = new(),
                [LocalizationService.GetString("config_group_color")] = new(),
                [LocalizationService.GetString("config_group_advanced")] = new(),
                [LocalizationService.GetString("config_group_system")] = new()
            };

            foreach (var key in ConfigGetters.Keys)
            {
                var group = key.StartsWith("color.") ? LocalizationService.GetString("config_group_color") :
                           key.StartsWith("advanced.") ? LocalizationService.GetString("config_group_advanced") :
                           key.StartsWith("app.") ? LocalizationService.GetString("config_group_system") :
                           LocalizationService.GetString("config_group_core");

                configGroups[group].Add((
                    key,
                    ConfigGetters[key](),
                    !ConfigSetters.ContainsKey(key)
                ));
            }

            foreach (var group in configGroups)
            {
                if (group.Value.Count > 0)
                {
                    OutputService.WriteTableColumnsHighlight(
                        group.Key,
                        new List<IEnumerable<string>> {
                            group.Value.Select(c => c.Key.Replace($"{group.Key.Split(' ')[0].ToLower()}.", "")),
                            group.Value.Select(c => c.Value),
                            group.Value.Select(c => c.ReadOnly ?
                                LocalizationService.GetString("yes") :
                                LocalizationService.GetString("no"))
                        },
                        new List<string> {
                            LocalizationService.GetString("config_setting"),
                            LocalizationService.GetString("config_value"),
                            LocalizationService.GetString("config_readonly")
                        }
                    );
                }
            }

            return Task.CompletedTask;
        }

        private Task GetConfig(string key)
        {
            if (ConfigGetters.TryGetValue(key, out var getter))
            {
                OutputService.WriteResponse($"{key} = {getter()}");
            }
            else
            {
                OutputService.WriteError("config_unknown_key", key);
            }
            return Task.CompletedTask;
        }

        private Task SetConfig(string key, string value)
        {
            try
            {
                if (ConfigSetters.TryGetValue(key, out var setter))
                {
                    setter(value);
                    OutputService.WriteResponse("config_updated", key, ConfigGetters[key]());
                    LoggerService.LogInfo(LocalizationService.GetString("config_changed_log", key, value));
                }
                else
                {
                    OutputService.WriteError("config_readonly_error", key);
                }
            }
            catch (Exception ex)
            {
                OutputService.WriteError("config_set_error", key, ex.Message);
                LoggerService.LogError(LocalizationService.GetString("config_set_error_log"), ex);
            }
            return Task.CompletedTask;
        }

        private Task ExportConfig(string[] args)
        {
            try
            {
                var exportData = ConfigGetters.Keys
                    .Where(key => ConfigSetters.ContainsKey(key))
                    .ToDictionary(key => key, key => ConfigGetters[key]());

                var json = JsonSerializer.Serialize(exportData);
                var encrypted = EncryptString(json, GetMachineKey());

                OutputService.WriteResponse("config_export_success");
                OutputService.WriteResponse(encrypted);
                LoggerService.LogInfo("Configuration exported successfully");
            }
            catch (Exception ex)
            {
                OutputService.WriteError("config_export_error", ex.Message);
                LoggerService.LogError("Configuration export failed", ex);
            }
            return Task.CompletedTask;
        }

        private Task ImportConfig(string[] args)
        {
            if (args.Length == 0)
            {
                OutputService.WriteError("config_import_missing_data");
                return Task.CompletedTask;
            }

            try
            {
                var encryptedData = string.Join(" ", args);
                var json = DecryptString(encryptedData, GetMachineKey());
                var importData = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (importData == null)
                {
                    OutputService.WriteError("config_import_invalid_data");
                    return Task.CompletedTask;
                }

                int appliedCount = 0;
                foreach (var item in importData)
                {
                    if (ConfigSetters.TryGetValue(item.Key, out var setter))
                    {
                        setter(item.Value);
                        appliedCount++;
                    }
                }

                OutputService.WriteResponse("config_import_success", appliedCount);
                LoggerService.LogInfo($"Imported {appliedCount} configuration settings");
            }
            catch (Exception ex)
            {
                OutputService.WriteError("config_import_error", ex.Message);
                LoggerService.LogError("Configuration import failed", ex);
            }
            return Task.CompletedTask;
        }

        private static string GetMachineKey()
        {
            using var sha = SHA256.Create();
            var machineId = $"{Environment.MachineName}{Environment.OSVersion}{Environment.ProcessorCount}";
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(machineId)))[..32];
        }

        private static string EncryptString(string plainText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16];

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private static string DecryptString(string cipherText, string key)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = new byte[16];

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }
    }
}