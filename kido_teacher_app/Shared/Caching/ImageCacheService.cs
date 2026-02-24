using kido_teacher_app.Config;
using kido_teacher_app.Shared.Network;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Service d�ng chung ?? cache ?nh cho Class, Course, Lecture, etc.
    /// </summary>
    public static class ImageCacheService
    {
        private static readonly HttpClient client = new HttpClient();

        // =========================
        // GET OR DOWNLOAD IMAGE
        // =========================
        public static async Task<Image?> GetOrDownloadImageAsync(
            string entityId, 
            string? imageFilename, 
            string cacheFolder)
        {
            System.Diagnostics.Debug.WriteLine($"[ImageCache] GetOrDownload: entityId={entityId}, filename={imageFilename}");

            // ===== 1?? KI?M TRA CACHE =====
            var cachedImage = LoadFromCache(entityId, imageFilename, cacheFolder);
            if (cachedImage != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Loaded from cache");
                return cachedImage;
            }

            // ===== 2?? KH�NG C� CACHE ? DOWNLOAD =====
            if (string.IsNullOrWhiteSpace(imageFilename))
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Filename is null/empty");
                return null;
            }

            if (OfflineState.IsOffline())
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Offline - skip download");
                return null;
            }

            var downloadedBytes = await DownloadBytesFromServerAsync(imageFilename);
            if (downloadedBytes == null || downloadedBytes.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Download failed");
                return null;
            }

            Image? downloadedImage = null;
            try
            {
                using var ms = new MemoryStream(downloadedBytes);
                using var tmp = Image.FromStream(ms);
                downloadedImage = (Image)tmp.Clone();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Image decode failed: {ex.Message}");
                return null;
            }

            // ===== 3?? L?U CACHE =====
            SaveBytesToCache(entityId, downloadedBytes, imageFilename, cacheFolder);
            System.Diagnostics.Debug.WriteLine($"[ImageCache] Saved to cache: {cacheFolder}");

            return downloadedImage;
        }

        // =========================
        // LOAD FROM CACHE
        // =========================
        private static Image? LoadFromCache(string entityId, string? imageFilename, string cacheFolder)
        {
            try
            {
                var exts = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
                foreach (var ext in exts)
                {
                    var path = Path.Combine(cacheFolder, $"{entityId}{ext}");
                    if (File.Exists(path))
                        return Image.FromFile(path);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // =========================
        // DOWNLOAD FROM SERVER
        // =========================
        private static async Task<byte[]?> DownloadBytesFromServerAsync(string filename)
        {
            try
            {
                string url;

                // ? N?U PATH B?T ??U B?NG / HO?C HTTP ? D�NG TR?C TI?P
                if (filename.StartsWith("http"))
                {
                    // Full URL
                    url = filename;
                }
                else if (filename.StartsWith("/") || filename.StartsWith("uploads/"))
                {
                    // Path tuy?t ??i t? API (VD: /uploads/xxx.jpg ho?c uploads/xxx.jpg)
                    // Th�m / n?u ch?a c�
                    url = filename.StartsWith("/") 
                        ? $"{AppConfig.ApiBaseUrl}{filename}"
                        : $"{AppConfig.ApiBaseUrl}/{filename}";
                }
                else
                {
                    // Ch? c� filename ? d�ng route chu?n
                    url = $"{AppConfig.ApiBaseUrl}/upload/downloads/{filename}/image";
                }

                System.Diagnostics.Debug.WriteLine($"[ImageCache] Downloading: {url}");

                using var req = new HttpRequestMessage(HttpMethod.Get, url);

                var res = await client.SendAsync(req);
                
                System.Diagnostics.Debug.WriteLine($"[ImageCache] HTTP {(int)res.StatusCode}: {res.StatusCode}");

                if (!res.IsSuccessStatusCode)
                {
                    var errorBody = await res.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"[ImageCache] Error: {errorBody}");
                    return null;
                }

                var bytes = await res.Content.ReadAsByteArrayAsync();
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Downloaded {bytes.Length} bytes");
                return bytes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Exception: {ex.Message}");
                return null;
            }
        }

        // =========================
        // SAVE TO CACHE
        // =========================
        private static void SaveBytesToCache(string entityId, byte[] bytes, string? imageFilename, string cacheFolder)
        {
            try
            {
                Directory.CreateDirectory(cacheFolder);

                var path = Path.Combine(cacheFolder, $"{entityId}{GetImageExtension(imageFilename)}");
                File.WriteAllBytes(path, bytes);
            }
            catch
            {
                // Fail silently
            }
        }

        private static string? GetOriginalFilename(string? imageFilename)
        {
            if (string.IsNullOrWhiteSpace(imageFilename))
                return null;

            try
            {
                var name = imageFilename;
                if (Uri.TryCreate(imageFilename, UriKind.Absolute, out var uri))
                    name = uri.AbsolutePath;

                var file = Path.GetFileName(name);
                return string.IsNullOrWhiteSpace(file) ? null : file;
            }
            catch
            {
                return null;
            }
        }

        private static string GetImageExtension(string? imageFilename)
        {
            if (string.IsNullOrWhiteSpace(imageFilename))
                return ".png";

            try
            {
                var name = imageFilename;
                if (Uri.TryCreate(imageFilename, UriKind.Absolute, out var uri))
                    name = uri.AbsolutePath;

                var ext = Path.GetExtension(name);
                if (string.IsNullOrWhiteSpace(ext))
                    return ".png";

                ext = ext.ToLowerInvariant();
                return ext switch
                {
                    ".jpg" => ".jpg",
                    ".jpeg" => ".jpg",
                    ".png" => ".png",
                    ".gif" => ".gif",
                    ".bmp" => ".bmp",
                    ".webp" => ".png",
                    _ => ".png"
                };
            }
            catch
            {
                return ".png";
            }
        }

        // ImageFormat mapping no longer needed when writing raw bytes.

        // =========================
        // DELETE CACHE
        // =========================
        public static void DeleteCache(string entityId, string cacheFolder)
        {
            try
            {
                var exts = new[] { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };
                foreach (var ext in exts)
                {
                    var path = Path.Combine(cacheFolder, $"{entityId}{ext}");
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
