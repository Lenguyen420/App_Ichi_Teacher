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
                var json = await LoadServerVersionJsonAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return true;
                }

                var server = ExtractVersionInfo(json);
                if (server == null || string.IsNullOrWhiteSpace(server.LatestVersion))
                {
                    return true;
                }

                EnsureLocalVersionFile(server.LatestVersion);
                var currentVersion = GetLocalVersion();
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

        private static async Task<string?> LoadServerVersionJsonAsync()
        {
            try
            {
                var url = AppConfig.UpdateVersionApiUrl;
                if (string.IsNullOrWhiteSpace(url))
                {
                    return null;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var response = await http.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
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

        private static string GetLocalVersionPath()
        {
            return Path.Combine(AppConfig.AppDataRoaming, "version.txt");
        }

        private static void EnsureLocalVersionFile(string? initialVersion)
        {
            try
            {
                Directory.CreateDirectory(AppConfig.AppDataRoaming);
                var path = GetLocalVersionPath();
                if (File.Exists(path))
                {
                    return;
                }

                var versionText = !string.IsNullOrWhiteSpace(initialVersion)
                    ? initialVersion
                    : GetCurrentVersionText();
                if (string.IsNullOrWhiteSpace(versionText))
                {
                    return;
                }

                File.WriteAllText(path, versionText);
            }
            catch
            {
                // ignore
            }
        }

        private static Version? GetLocalVersion()
        {
            try
            {
                var path = GetLocalVersionPath();
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path).Trim();
                    if (Version.TryParse(text, out var v))
                    {
                        return v;
                    }
                }
            }
            catch
            {
                // ignore
            }

            return GetCurrentVersion();
        }

        private static string? GetCurrentVersionText()
        {
            var v = GetCurrentVersion();
            if (v == null)
            {
                return null;
            }

            var normalized = NormalizeVersion(v);
            return $"{normalized.Major}.{normalized.Minor}.{normalized.Build}";
        }
    }
}
