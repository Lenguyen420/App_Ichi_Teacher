using kido_teacher_app.Config;
using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Service dùng chung ?? cache ?nh cho Class, Course, Lecture, etc.
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
            var cachedImage = LoadFromCache(entityId, cacheFolder);
            if (cachedImage != null)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Loaded from cache");
                return cachedImage;
            }

            // ===== 2?? KHÔNG CÓ CACHE ? DOWNLOAD =====
            if (string.IsNullOrWhiteSpace(imageFilename))
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Filename is null/empty");
                return null;
            }

            var downloadedImage = await DownloadImageFromServerAsync(imageFilename);
            if (downloadedImage == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageCache] Download failed");
                return null;
            }

            // ===== 3?? L?U CACHE =====
            SaveToCache(entityId, downloadedImage, cacheFolder);
            System.Diagnostics.Debug.WriteLine($"[ImageCache] Saved to cache: {cacheFolder}");

            return downloadedImage;
        }

        // =========================
        // LOAD FROM CACHE
        // =========================
        private static Image? LoadFromCache(string entityId, string cacheFolder)
        {
            try
            {
                var pngPath = Path.Combine(cacheFolder, $"{entityId}.png");
                var jpgPath = Path.Combine(cacheFolder, $"{entityId}.jpg");

                if (File.Exists(pngPath))
                    return Image.FromFile(pngPath);

                if (File.Exists(jpgPath))
                    return Image.FromFile(jpgPath);

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
        private static async Task<Image?> DownloadImageFromServerAsync(string filename)
        {
            try
            {
                string url;

                // ? N?U PATH B?T ??U B?NG / HO?C HTTP ? DÙNG TR?C TI?P
                if (filename.StartsWith("http"))
                {
                    // Full URL
                    url = filename;
                }
                else if (filename.StartsWith("/") || filename.StartsWith("uploads/"))
                {
                    // Path tuy?t ??i t? API (VD: /uploads/xxx.jpg ho?c uploads/xxx.jpg)
                    // Thêm / n?u ch?a có
                    url = filename.StartsWith("/") 
                        ? $"{AppConfig.ApiBaseUrl}{filename}"
                        : $"{AppConfig.ApiBaseUrl}/{filename}";
                }
                else
                {
                    // Ch? có filename ? dùng route chu?n
                    url = $"{AppConfig.ApiBaseUrl}/upload/downloads/{filename}/image";
                }

                System.Diagnostics.Debug.WriteLine($"[ImageCache] Downloading: {url}");

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", Config.AuthSession.AccessToken);

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

                using var ms = new MemoryStream(bytes);
                using var tmp = Image.FromStream(ms);

                return (Image)tmp.Clone();
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
        private static void SaveToCache(string entityId, Image image, string cacheFolder)
        {
            try
            {
                Directory.CreateDirectory(cacheFolder);

                var path = Path.Combine(cacheFolder, $"{entityId}.png");
                image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            }
            catch
            {
                // Fail silently
            }
        }

        // =========================
        // DELETE CACHE
        // =========================
        public static void DeleteCache(string entityId, string cacheFolder)
        {
            try
            {
                var pngPath = Path.Combine(cacheFolder, $"{entityId}.png");
                var jpgPath = Path.Combine(cacheFolder, $"{entityId}.jpg");

                if (File.Exists(pngPath))
                    File.Delete(pngPath);

                if (File.Exists(jpgPath))
                    File.Delete(jpgPath);
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
