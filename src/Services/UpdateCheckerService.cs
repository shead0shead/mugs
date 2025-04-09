// Mugs/Services/UpdateCheckerService.cs

using Mugs.Models;

using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;

namespace Mugs.Services
{
    public class UpdateCheckerService
    {
        private const string GitHubRepoOwner = "shead0shead";
        private const string GitHubRepoName = "mugs";
        private const string ManifestUrl = $"https://raw.githubusercontent.com/{GitHubRepoOwner}/{GitHubRepoName}/main/update_manifest.json";
        private static readonly HttpClient _httpClient = new HttpClient();
        public static readonly Version CurrentVersion = new Version("1.2.0");

        static UpdateCheckerService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "MugsUpdater");
            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        public static async Task<UpdateManifest> GetUpdateManifestAsync()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(ManifestUrl);
                return JsonConvert.DeserializeObject<UpdateManifest>(response);
            }
            catch
            {
                return null;
            }
        }

        public static async Task CheckForUpdatesAsync(bool notifyIfNoUpdate = false, bool automaticCheck = false)
        {
            try
            {
                var manifest = await GetUpdateManifestAsync();
                if (manifest == null)
                {
                    if (!automaticCheck)
                        OutputService.WriteError("update_manifest_error");
                    return;
                }

                var latestVersion = new Version(manifest.LatestVersion);
                if (latestVersion > CurrentVersion)
                {
                    OutputService.WriteResponse("update_available",
                        latestVersion,
                        CurrentVersion,
                        manifest.Changelog,
                        manifest.Critical ? "CRITICAL" : "regular");

                    if (manifest.Critical)
                    {
                        OutputService.WriteResponse("critical_update_warning");
                    }
                }
                else if (notifyIfNoUpdate)
                {
                    OutputService.WriteResponse("no_update_available", CurrentVersion);
                }
            }
            catch (Exception ex)
            {
                if (!automaticCheck)
                    OutputService.WriteError("update_error", ex.Message);
            }
        }

        public static async Task InstallUpdateAsync(UpdateManifest manifest = null)
        {
            try
            {
                manifest ??= await GetUpdateManifestAsync();
                if (manifest == null)
                {
                    OutputService.WriteError("update_manifest_error");
                    return;
                }

                OutputService.WriteResponse("starting_update");

                string tempDir = Path.Combine(Path.GetTempPath(), "MugsUpdate");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                foreach (var asset in manifest.Assets)
                {
                    await DownloadFileWithProgress(asset.DownloadUrl,
                        Path.Combine(tempDir, asset.FileName),
                        asset.SHA256);
                }

                OutputService.WriteResponse("creating_backup");
                string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                string currentDir = Path.GetDirectoryName(currentExePath);
                string backupDir = Path.Combine(currentDir, $"Backup_{DateTime.Now:yyyyMMddHHmmss}");
                Directory.CreateDirectory(backupDir);

                foreach (var asset in manifest.Assets)
                {
                    string targetPath = Path.Combine(currentDir, asset.FileName);
                    if (File.Exists(targetPath))
                    {
                        File.Copy(targetPath, Path.Combine(backupDir, asset.FileName));
                    }
                }

                OutputService.WriteResponse("installing_update");
                foreach (var asset in manifest.Assets)
                {
                    string sourcePath = Path.Combine(tempDir, asset.FileName);
                    string targetPath = Path.Combine(currentDir, asset.FileName);

                    if (File.Exists(targetPath))
                        File.Delete(targetPath);

                    File.Move(sourcePath, targetPath);
                }

                OutputService.WriteResponse("finishing_update");
                Process.Start(new ProcessStartInfo
                {
                    FileName = currentExePath,
                    Arguments = "--updated",
                    UseShellExecute = true
                });

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                OutputService.WriteError("update_failed", ex.Message);
            }
        }

        private static async Task DownloadFileWithProgress(string url, string filePath, string expectedHash)
        {
            using var client = new WebClient();
            var progress = new Progress<float>(p =>
                OutputService.WriteResponse("download_progress", Path.GetFileName(filePath), (int)(p * 100)));

            await client.DownloadFileTaskAsync(new Uri(url), filePath);

            if (!string.IsNullOrEmpty(expectedHash))
            {
                var actualHash = await CalculateFileHash(filePath);
                if (!actualHash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(filePath);
                    throw new Exception($"Hash mismatch for {Path.GetFileName(filePath)}");
                }
            }
        }

        private static async Task<string> CalculateFileHash(string filePath)
        {
            using var sha = SHA256.Create();
            await using var stream = File.OpenRead(filePath);
            var hash = await sha.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}