// Mugs/Services/UpdateCheckerService.cs

using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;

namespace Mugs.Services
{
    public class UpdateCheckerService
    {
        private const string GitHubRepoOwner = "shead0shead";
        private const string GitHubRepoName = "mugs";
        private const string GitHubReleasesUrl = $"https://api.github.com/repos/{GitHubRepoOwner}/{GitHubRepoName}/releases/latest";
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly Version CurrentVersion = new Version("1.1.2");

        static UpdateCheckerService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ConsoleAppUpdater");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
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
                    OutputService.WriteResponse("update_available",
                        latestVersion,
                        CurrentVersion,
                        release.html_url,
                        release.body);
                }
                else if (notifyIfNoUpdate)
                {
                    OutputService.WriteResponse("no_update_available", CurrentVersion);
                }
            }
            catch (Exception ex)
            {
                OutputService.WriteError("update_error", ex.Message);
            }
        }

        public static async Task InstallUpdateAsync()
        {
            try
            {
                OutputService.WriteResponse("starting_update");

                var response = await _httpClient.GetStringAsync(GitHubReleasesUrl);
                dynamic release = JsonConvert.DeserializeObject(response);
                string version = release.tag_name;

                string downloadUrl = null;
                foreach (var asset in release.assets)
                {
                    if (asset.name.ToString().Equals("Mugs.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        downloadUrl = asset.browser_download_url.ToString();
                        break;
                    }
                }

                if (downloadUrl == null)
                {
                    throw new Exception("Mugs.exe not found in release assets");
                }

                string tempDir = Path.Combine(Path.GetTempPath(), "MugsUpdate");
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
                Directory.CreateDirectory(tempDir);

                OutputService.WriteResponse("downloading_update");
                string exePath = Path.Combine(tempDir, "Mugs.exe");
                using (var client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(new Uri(downloadUrl), exePath);
                }

                string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                string currentDir = Path.GetDirectoryName(currentExePath);
                string backupPath = Path.Combine(currentDir, $"Mugs_backup_{DateTime.Now:yyyyMMddHHmmss}.exe");

                OutputService.WriteResponse("creating_backup");
                File.Copy(currentExePath, backupPath, true);

                OutputService.WriteResponse("installing_update");
                File.Delete(currentExePath);
                File.Move(exePath, currentExePath);

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
    }
}