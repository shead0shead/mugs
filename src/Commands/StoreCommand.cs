// Mugs/Commands/StoreCommand.cs

using Mugs.Services;
using Mugs.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Mugs.Commands
{
    public class StoreCommand : ICommand
    {
        private const string StoreRepoOwner = "shead0shead";
        private const string StoreRepoName = "mugs-store";
        private const string StoreApiUrl = $"https://api.github.com/repos/{StoreRepoOwner}/{StoreRepoName}/contents/";
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly CommandManager _manager;
        private readonly string _extensionsPath;

        public StoreCommand(CommandManager manager, string extensionsPath)
        {
            _manager = manager;
            _extensionsPath = extensionsPath;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mugs-Store-Client");
        }

        public string Name => "store";
        public string Description => "Поиск и установка дополнений из официального хранилища";
        public IEnumerable<string> Aliases => new[] { "market", "repo" };
        public string Author => "System";
        public string Version => "1.0";
        public string? UsageExample => "store search <query>\nstore install <package>\nstore list";

        public async Task ExecuteAsync(string[] args)
        {
            if (args.Length == 0)
            {
                ConsoleHelperService.WriteResponse("Доступные подкоманды:\n" +
                    "  search <query> - поиск дополнений\n" +
                    "  install <name> - установка дополнения\n" +
                    "  list - список доступных дополнений");
                return;
            }

            try
            {
                switch (args[0].ToLower())
                {
                    case "search":
                        await SearchExtensions(args.Length > 1 ? args[1] : "");
                        break;
                    case "install":
                        if (args.Length < 2)
                        {
                            ConsoleHelperService.WriteError("Укажите название дополнения для установки");
                            return;
                        }
                        await InstallExtension(args[1]);
                        break;
                    case "list":
                        await ListExtensions();
                        break;
                    default:
                        ConsoleHelperService.WriteError("Неизвестная подкоманда");
                        break;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError($"Ошибка: {ex.Message}");
            }
        }

        private async Task SearchExtensions(string query)
        {
            var extensions = await GetStoreExtensions();
            var results = extensions.Where(e =>
                e.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                e.Description.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (!results.Any())
            {
                ConsoleHelperService.WriteResponse("Дополнения не найдены");
                return;
            }

            var response = new StringBuilder("Результаты поиска:\n");
            foreach (var ext in results)
            {
                response.AppendLine($"- {ext.Name} (v{ext.Version})");
                response.AppendLine($"  {ext.Description}");
                response.AppendLine($"  Установка: store install {ext.Name}");
                response.AppendLine();
            }

            ConsoleHelperService.WriteResponse(response.ToString().TrimEnd());
        }

        private async Task ListExtensions()
        {
            var extensions = await GetStoreExtensions();
            if (!extensions.Any())
            {
                ConsoleHelperService.WriteResponse("Хранилище дополнений пусто");
                return;
            }

            var response = new StringBuilder("Доступные дополнения:\n");
            foreach (var ext in extensions)
            {
                response.AppendLine($"- {ext.Name} (v{ext.Version}) - {ext.Description}");
            }

            ConsoleHelperService.WriteResponse(response.ToString().TrimEnd());
        }

        private async Task InstallExtension(string name)
        {
            var extensions = await GetStoreExtensions();
            var extension = extensions.FirstOrDefault(e =>
                e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (extension == null)
            {
                ConsoleHelperService.WriteError($"Дополнение '{name}' не найдено");
                return;
            }

            ConsoleHelperService.WriteResponse($"Установка {extension.Name}...");
            var downloadUrl = extension.DownloadUrl;

            try
            {
                using var client = new WebClient();
                var fileName = Path.GetFileName(new Uri(downloadUrl).LocalPath);
                var filePath = Path.Combine(_extensionsPath, fileName);

                await client.DownloadFileTaskAsync(downloadUrl, filePath);
                ConsoleHelperService.WriteResponse($"Дополнение {extension.Name} успешно установлено!");
                ConsoleHelperService.WriteResponse("Выполните 'reload' для загрузки новых команд");
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async Task<List<StoreExtension>> GetStoreExtensions()
        {
            try
            {
                var response = await _httpClient.GetStringAsync(StoreApiUrl);
                var files = JsonConvert.DeserializeObject<GitHubFile[]>(response);

                var catalogFile = files.FirstOrDefault(f =>
                    f.Name.Equals("catalog.json", StringComparison.OrdinalIgnoreCase));

                if (catalogFile == null)
                    return new List<StoreExtension>();

                var catalogContent = await _httpClient.GetStringAsync(catalogFile.DownloadUrl);
                return JsonConvert.DeserializeObject<List<StoreExtension>>(catalogContent) ??
                    new List<StoreExtension>();
            }
            catch
            {
                return new List<StoreExtension>();
            }
        }
    }

    public class GitHubFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("download_url")]
        public string DownloadUrl { get; set; }
    }

    public class StoreExtension
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string Author { get; set; }
    }
}