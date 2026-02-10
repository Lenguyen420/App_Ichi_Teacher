using System.Text.Json.Serialization;

namespace kido_teacher_app.Model
{
    public class VersionInfo
    {
        [JsonPropertyName("latestVersion")]
        public string? LatestVersion { get; set; }

        [JsonPropertyName("forceUpdate")]
        public bool ForceUpdate { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string? DownloadUrl { get; set; }
    }
}
