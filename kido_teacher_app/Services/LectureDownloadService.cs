using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class LectureDownloadService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };

        // =====================================================
        // CREATE DOWNLOAD LOG
        // =====================================================
        public static async Task<bool> CreateDownloadLogAsync(LectureDownloadLogDto dto)
        {
            EnsureAuthorized();

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/lecture/download-log", content);

            return response.IsSuccessStatusCode;
        }

        // =====================================================
        // SCAN FILES AND CREATE DOWNLOAD LOGS
        // =====================================================
        public static async Task ScanAndLogFilesAsync(string extractPath, string lectureId)
        {
            if (!Directory.Exists(extractPath))
                return;

            var allFiles = Directory.GetFiles(extractPath, "*.*", SearchOption.AllDirectories);

            foreach (var filePath in allFiles)
            {
                var extension = Path.GetExtension(filePath).ToLower();
                string fileType = null;

                if (extension == ".pdf")
                    fileType = "pdf";
                else if (extension == ".html" || extension == ".htm")
                    fileType = "html";
                else if (extension == ".mp4")
                    fileType = "mp4";

                if (fileType != null)
                {
                    try
                    {
                        await CreateDownloadLogAsync(new LectureDownloadLogDto
                        {
                            lectureId = lectureId,
                            path = filePath,
                            type = fileType
                        });
                    }
                    catch
                    {
                        // B? qua l?i n?u không t?o ???c log
                    }
                }
            }
        }

        // =====================================================
        // DOWNLOAD, EXTRACT & LOG (WITH LECTURE ID)
        // =====================================================
        public static async Task<string?> DownloadExtractAndLogAsync(
            string zipFilename,
            string lectureId,
            IProgress<int>? progress = null)
        {
            // T?i và gi?i nén
            var extractPath = await LectureService.DownloadAndExtractZipAsync(zipFilename, lectureId, progress);

            if (string.IsNullOrEmpty(extractPath))
                return null;

            // Quét file và t?o log
            await ScanAndLogFilesAsync(extractPath, lectureId);

            return extractPath;
        }

        private static void EnsureAuthorized()
        {
            if (string.IsNullOrEmpty(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token không t?n t?i");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }
    }
}
