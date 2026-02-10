using kido_teacher_app.Config;
using kido_teacher_app.Model;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Services
{
    public static class VersionCheckService
    {
        private static readonly HttpClient http = new HttpClient();

        public static async Task<bool> CheckAsync()
        {
            try
            {
                var json = await LoadLocalVersionJsonAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return true;
                }

                var server = ExtractVersionInfo(json);
                if (server == null || string.IsNullOrWhiteSpace(server.LatestVersion))
                {
                    return true;
                }

                var currentVersion = GetCurrentVersion();
                if (currentVersion == null)
                {
                    return true;
                }

                if (Version.TryParse(server.LatestVersion, out var latest))
                {
                    var latestNormalized = NormalizeVersion(latest);
                    var currentNormalized = NormalizeVersion(currentVersion);
                    if (currentNormalized < latestNormalized)
                    {
                    var msg = server.ForceUpdate
                        ? "Có phiên bản mới, vui lòng cập nhật để tiếp tục sử dụng."
                        : "Có phiên bản mới, bạn có muốn cập nhật không?";

                    var buttons = server.ForceUpdate ? MessageBoxButtons.OK : MessageBoxButtons.OKCancel;
                    var result = MessageBox.Show(
                        msg,
                        "Update",
                        buttons,
                        MessageBoxIcon.Warning
                    );

                        if (server.ForceUpdate || result == DialogResult.OK)
                        {
                            OpenUrl(server.DownloadUrl);
                            Application.Exit();
                            return false;
                        }
                    }
                }
            }
            catch
            {
                // Nếu không check được version thì cho vào app (an toàn cho offline).
                return true;
            }

            return true;
        }

        private static Task<string?> LoadLocalVersionJsonAsync()
        {
            try
            {
                var path = Path.Combine(Application.StartupPath, "version.json");
                if (!File.Exists(path))
                {
                    return Task.FromResult<string?>(null);
                }

                return Task.FromResult<string?>(File.ReadAllText(path));
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }

        private static VersionInfo? ExtractVersionInfo(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var data))
                {
                    return JsonSerializer.Deserialize<VersionInfo>(data.GetRawText());
                }
                return JsonSerializer.Deserialize<VersionInfo>(json);
            }
            catch
            {
                return null;
            }
        }

        private static Version? GetCurrentVersion()
        {
            try
            {
                return Assembly.GetExecutingAssembly().GetName().Version;
            }
            catch
            {
                return null;
            }
        }

        private static void OpenUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // ignore
            }
        }

        private static Version NormalizeVersion(Version v)
        {
            var major = v.Major;
            var minor = v.Minor < 0 ? 0 : v.Minor;
            var build = v.Build < 0 ? 0 : v.Build;
            var rev = v.Revision < 0 ? 0 : v.Revision;
            return new Version(major, minor, build, rev);
        }
    }
}
