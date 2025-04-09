// Mugs/Models/UpdateManifest.cs

using Newtonsoft.Json;

namespace Mugs.Models
{
    public class UpdateManifest
    {
        [JsonProperty("latestVersion")]
        public string LatestVersion { get; set; }

        [JsonProperty("critical")]
        public bool Critical { get; set; }

        [JsonProperty("changelog")]
        public string Changelog { get; set; }

        [JsonProperty("assets")]
        public List<UpdateAsset> Assets { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public class UpdateAsset
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("sha256")]
        public string SHA256 { get; set; }
    }
}