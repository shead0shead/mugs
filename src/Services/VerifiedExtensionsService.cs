// Mugs/Services/VerifiedExtensionsService.cs

using Newtonsoft.Json;

namespace Mugs.Services
{
    public class VerifiedExtensionsService
    {
        private const string VerifiedHashesUrl = "https://raw.githubusercontent.com/shead0shead/mugs/main/verified_hashes.json";
        private static readonly HttpClient _httpClient = new HttpClient();
        private static Dictionary<string, string> _verifiedHashes = new Dictionary<string, string>();
        private static bool _hashesLoaded = false;

        static VerifiedExtensionsService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ConsoleAppVerifiedChecker");
        }

        public static async Task EnsureHashesLoadedAsync()
        {
            if (_hashesLoaded) return;

            try
            {
                var response = await _httpClient.GetStringAsync(VerifiedHashesUrl);
                _verifiedHashes = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                _hashesLoaded = true;
            }
            catch (Exception ex)
            {
                ConsoleHelperService.WriteError("verified_load_error", ex.Message);
                _verifiedHashes = new Dictionary<string, string>();
                _hashesLoaded = true;
            }
        }

        public static bool IsExtensionVerified(string fileName)
        {
            if (!_hashesLoaded || !_verifiedHashes.Any())
                return false;

            var normalizedFileName = fileName.ToLowerInvariant();
            return _verifiedHashes.Any(kv => kv.Key.ToLowerInvariant() == normalizedFileName);
        }

        public static string? GetVerifiedHash(string fileName)
        {
            var normalizedFileName = fileName.ToLowerInvariant();
            return _verifiedHashes.FirstOrDefault(kv => kv.Key.ToLowerInvariant() == normalizedFileName).Value;
        }
    }
}