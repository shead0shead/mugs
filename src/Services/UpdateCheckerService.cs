// Mugs/Services/UpdateCheckerService.cs

using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Net;

namespace Mugs.Services
{
    public class UpdateCheckerService
    {
        private const string GitHubRepoOwner = "shead0shead";
        private const string GitHubRepoName = "mugs";
        private const string GitHubReleasesUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest";
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Version CurrentVersion = new Version("1.0.0");

        static UpdateCheckerService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ConsoleAppUpdater");
        }

        public static async Task CheckForUpdatesAsync(bool notifyIfNoUpdate = false)
        {
            try
            {
                var response = await _httpClient.GetStringAsync(GitHubReleasesUrl);
                dynamic release = JsonConvert.DeserializeObject(response);
                var latestVersion = new Version(release.tag_name.ToString().TrimStart('v'));

                if (latestVersion > CurrentVersion)
                {
                    ConsoleHelperService.WriteResponse("update_available",
                        latestVersion,
                        CurrentVersion,
                        release.html_url,
                        release.body);
                }
                else if (notifyIfNoUpdate)
                {
                    ConsoleHelperService.WriteResponse("no_update_available", CurrentVersion);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("update_error", ex.Message);
            }
        }

        public static async Task InstallUpdateAsync()
        {
            try
            {
                ConsoleHelperService.WriteResponse("starting_update");

                var response = await _httpClient.GetStringAsync(GitHubReleasesUrl);
                dynamic release = JsonConvert.DeserializeObject(response);
                string downloadUrl = release.assets[0].browser_download_url;
                string version = release.tag_name;

                string tempDir = Path.Combine(Path.GetTempPath(), "ConsoleAppUpdate");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                ConsoleHelperService.WriteResponse("downloading_update");
                string zipPath = Path.Combine(tempDir, "update.zip");
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), zipPath);
                }

                ConsoleHelperService.WriteResponse("extracting_update");
                ZipFile.ExtractToDirectory(zipPath, tempDir);

                string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                string currentDir = Path.GetDirectoryName(currentExePath);
                string backupDir = Path.Combine(currentDir, "Backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));

                ConsoleHelperService.WriteResponse("creating_backup");
                Directory.CreateDirectory(backupDir);
                foreach (var file in Directory.GetFiles(currentDir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    if (!file.EndsWith(".dll") && !file.EndsWith(".exe")) continue;
                    File.Copy(file, Path.Combine(backupDir, Path.GetFileName(file)));
                }

                ConsoleHelperService.WriteResponse("installing_update");
                foreach (var file in Directory.GetFiles(tempDir, "*.*", SearchOption.TopDirectoryOnly))
                {
                    string destPath = Path.Combine(currentDir, Path.GetFileName(file));
                    if (File.Exists(destPath)) File.Delete(destPath);
                    File.Move(file, destPath);
                }

                ConsoleHelperService.WriteResponse("finishing_update");
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
                ConsoleHelperService.WriteError("update_failed", ex.Message);
            }
        }
    }
}