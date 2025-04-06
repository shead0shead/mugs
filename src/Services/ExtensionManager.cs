// Mugs/Services/ExtensionManager.cs

namespace Mugs.Services
{
    public class ExtensionManager
    {
        private readonly string _extensionsPath;

        public ExtensionManager(string extensionsPath)
        {
            _extensionsPath = extensionsPath;
            Directory.CreateDirectory(extensionsPath);
        }

        public async Task EnableExtensionAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ConsoleHelperService.WriteError("missing_extension_name");
                return;
            }

            string disabledFile;
            string enabledFile;

            if (name.EndsWith(".csx.disable", StringComparison.OrdinalIgnoreCase))
            {
                disabledFile = Path.Combine(_extensionsPath, name);
                if (!File.Exists(disabledFile))
                {
                    ConsoleHelperService.WriteError("extension_not_found", name);
                    return;
                }

                enabledFile = Path.Combine(_extensionsPath, Path.GetFileNameWithoutExtension(name));
            }
            else
            {
                var commandName = name.ToLowerInvariant();
                var disabledFiles = Directory.GetFiles(_extensionsPath, "*.csx.disable")
                    .Where(f => Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f))
                    .Equals(commandName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!disabledFiles.Any())
                {
                    var possibleFile = Path.Combine(_extensionsPath, commandName + ".csx.disable");
                    if (File.Exists(possibleFile))
                    {
                        disabledFiles.Add(possibleFile);
                    }
                    else
                    {
                        ConsoleHelperService.WriteError("no_disabled_extensions", commandName);
                        return;
                    }
                }

                if (disabledFiles.Count > 1)
                {
                    ConsoleHelperService.WriteResponse("multiple_extensions", commandName);
                    foreach (var file in disabledFiles)
                    {
                        ConsoleHelperService.WriteResponse($"- {Path.GetFileName(file)}");
                    }
                    ConsoleHelperService.WriteError("specify_filename");
                    return;
                }

                disabledFile = disabledFiles.First();
                enabledFile = Path.Combine(_extensionsPath, Path.GetFileNameWithoutExtension(disabledFile));
            }

            File.Move(disabledFile, enabledFile);
            ConsoleHelperService.WriteResponse("extension_enabled", Path.GetFileName(enabledFile));
        }

        public async Task DisableExtensionAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                ConsoleHelperService.WriteError("missing_extension_name");
                return;
            }

            string sourceFile;
            string disabledFile;

            if (name.EndsWith(".csx", StringComparison.OrdinalIgnoreCase))
            {
                sourceFile = Path.Combine(_extensionsPath, name);
                if (!File.Exists(sourceFile))
                {
                    ConsoleHelperService.WriteError("extension_not_found", name);
                    return;
                }

                disabledFile = sourceFile + ".disable";
            }
            else
            {
                var commandName = name.ToLowerInvariant();
                var sourceFiles = Directory.GetFiles(_extensionsPath, "*.csx")
                    .Where(f => Path.GetFileNameWithoutExtension(f)
                        .Equals(commandName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (!sourceFiles.Any())
                {
                    var possibleFile = Path.Combine(_extensionsPath, commandName + ".csx");
                    if (File.Exists(possibleFile))
                    {
                        sourceFile = possibleFile;
                        disabledFile = possibleFile + ".disable";
                        File.Move(sourceFile, disabledFile);
                        ConsoleHelperService.WriteResponse("extension_disabled", Path.GetFileName(sourceFile));
                        return;
                    }

                    ConsoleHelperService.WriteError("command_not_found_disable", commandName);
                    return;
                }

                if (sourceFiles.Count > 1)
                {
                    ConsoleHelperService.WriteResponse("multiple_extensions", commandName);
                    foreach (var file in sourceFiles)
                    {
                        ConsoleHelperService.WriteResponse($"- {Path.GetFileName(file)}");
                    }
                    ConsoleHelperService.WriteError("specify_filename");
                    return;
                }

                sourceFile = sourceFiles.First();
                disabledFile = sourceFile + ".disable";
            }

            File.Move(sourceFile, disabledFile);
            ConsoleHelperService.WriteResponse("extension_disabled", Path.GetFileName(sourceFile));
        }

        public async Task ImportFromUrlAsync(string url)
        {
            try
            {
                ConsoleHelperService.WriteResponse("downloading_extension", url);

                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var fileName = Path.GetFileName(url) ?? $"extension_{DateTime.Now:yyyyMMddHHmmss}.csx";
                var filePath = Path.Combine(_extensionsPath, fileName);

                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var fileStream = File.Create(filePath);
                await stream.CopyToAsync(fileStream);

                ConsoleHelperService.WriteResponse("extension_downloaded", fileName);
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("download_error", ex.Message);
            }
        }
    }
}