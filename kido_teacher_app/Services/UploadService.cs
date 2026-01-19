using kido_teacher_app.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using System.Text;
//using System.Text.Json;
using System.Net.Http.Headers;

namespace kido_teacher_app.Services
{
    public static class UploadService
    {
        // ===================== HTTP CLIENTS =====================
        private static readonly HttpClient uploadHttp = new HttpClient();
        private static readonly HttpClient downloadHttp = new HttpClient();
        private static readonly HttpClient zipHttp = new HttpClient();

        // ========================================================
        // ======================= UPLOAD IMAGE ===================
        // ========================================================
        private const string FORM_FIELD_NAME = "file";
        // ví dụ backend: @UploadedFile('image') → đổi thành "image"

        public static async Task<string?> UploadImageAsync(string filePath)
        {
            try
            {
                // ========= CHECK FILE =========
                if (!System.IO.File.Exists(filePath))
                {
                    
                    return null;
                }

                using var form = new MultipartFormDataContent();
                using var fs = System.IO.File.OpenRead(filePath);
                using var fileContent = new StreamContent(fs);

                // ========= MIME =========
                string ext = Path.GetExtension(filePath).ToLower();
                string mime =
                    ext == ".jpg" || ext == ".jpeg" ? "image/jpeg" :
                    ext == ".png" ? "image/png" :
                    "application/octet-stream";

                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(mime);

                // ⚠️ HARD-CODE FIELD NAME ĐỂ DEBUG
                string fieldName = "file";

                form.Add(
                    fileContent,
                    fieldName,
                    Path.GetFileName(filePath)
                );

                var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.UPLOAD_SINGLE}";

                using var req = new HttpRequestMessage(HttpMethod.Post, url);
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

                req.Content = form;

                // ========= SEND =========
                var res = await uploadHttp.SendAsync(req);

                // ========= SERVER ERROR =========
                if (!res.IsSuccessStatusCode)
                {
                    var errBody = await res.Content.ReadAsStringAsync();

                    return null;
                }

                // ========= READ RESPONSE =========
                var json = await res.Content.ReadAsStringAsync();

                var obj = JObject.Parse(json);

                var data = obj["data"];
                if (data == null)
                {
                    return null;
                }

                // ⭐ ƯU TIÊN LẤY PATH TỪ API
                var path = data.Value<string>("path");
                
                if (!string.IsNullOrWhiteSpace(path))
                {
                    System.Diagnostics.Debug.WriteLine($"[Upload] Got path from API: {path}");
                    return path;
                }

                // ⭐ FALLBACK: Nếu API không trả path, tự build từ filename
                var filename =
                    data.Value<string>("filename") ??
                    data.Value<string>("fileName") ??
                    data.Value<string>("name");

                if (string.IsNullOrWhiteSpace(filename))
                {
                    return null;
                }

                // Build full path
                var fullPath = $"/upload/downloads/{filename}/image";
                System.Diagnostics.Debug.WriteLine($"[Upload] Built path from filename: {fullPath}");
                
                return fullPath;
            }
            catch (Exception ex)
            {               
                return null;
            }
        }


        // ========================================================
        // ======================= DOWNLOAD IMAGE =================
        // ========================================================
        public static async Task<Image?> DownloadImageAsync(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return null;

            var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.UploadDownloadImage(filename)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await downloadHttp.SendAsync(req);
            if (!res.IsSuccessStatusCode)
                return null;

            var bytes = await res.Content.ReadAsByteArrayAsync();

            using var ms = new MemoryStream(bytes);
            using var tmp = Image.FromStream(ms);

            return (Image)tmp.Clone();
        }

        // ========================================================
        // ======================= UPLOAD ZIP =====================
        // ========================================================
        public static async Task<string> UploadLectureZipAsync(
    string lectureId,
    string zipPath,
    IProgress<int>? progress = null
)
        {
            //MessageBox.Show(
            //    "[UPLOAD - START]\n" +
            //    $"lectureId = {lectureId}\n" +
            //    $"zipPath = {zipPath}"
            //);

            zipHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            using var form = new MultipartFormDataContent();
            using var fs = System.IO.File.OpenRead(zipPath);

            var fileContent = new ProgressStreamContent(fs, progress);
            fileContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("application/octet-stream");

            form.Add(fileContent, "file", Path.GetFileName(zipPath));
            form.Add(new StringContent("LECTURE"), "fileType");
            form.Add(new StringContent(lectureId), "description");

            //MessageBox.Show("[UPLOAD] Đã tạo form-data, bắt đầu POST");

            var res = await zipHttp.PostAsync(
                $"{AppConfig.ApiBaseUrl}/upload/large",
                form
            );

            //MessageBox.Show(
            //    $"[UPLOAD] HTTP STATUS = {(int)res.StatusCode} {res.StatusCode}"
            //);

            var body = await res.Content.ReadAsStringAsync();

            //MessageBox.Show(
            //    "[UPLOAD] RESPONSE BODY:\n\n" + body
            //);

            if (!res.IsSuccessStatusCode)
            {
                MessageBox.Show(
                    $" Upload lỗi!\nHTTP {(int)res.StatusCode}\n\n{body}",
                    "Upload Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                throw new Exception(body);
            }

            var json = JObject.Parse(body);

            //MessageBox.Show(
            //    "[UPLOAD] PARSE JSON OK\n" +
            //    $"success = {json["success"]}\n" +
            //    $"data = {json["data"]}"
            //);

            if (json["success"]?.Value<bool>() != true)
                throw new Exception("Upload không thành công");

            var path = json["data"]?["path"]?.ToString();

            //MessageBox.Show(
            //    $"[UPLOAD - RESULT]\npath = '{path}'"
            //);

            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("❌ Không có data.path trong response");

           // MessageBox.Show("✅ UploadLectureZipAsync KẾT THÚC THÀNH CÔNG");

            return path;
        }

        // lưu file sau khi giải nén màn giáo án 
        public static async Task<bool> LogLectureDownloadAsync(
    string lectureId,
    string filePath,
    string fileType
)
        {
            var url = $"{AppConfig.ApiBaseUrl}/lecture/download-log";

            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var body = new
            {
                lectureId = lectureId,
                path = filePath,
                type = fileType
            };

            string json = JsonConvert.SerializeObject(body);
            req.Content = new StringContent(
    JsonConvert.SerializeObject(body),
    Encoding.UTF8,
    "application/json"
);

            var res = await uploadHttp.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        // ========================================================
        // =========== GET DOWNLOAD LOGS BY LECTURE ID ===========
        // ========================================================
        public static async Task<System.Collections.Generic.List<LectureDownloadLogDto>?> GetDownloadLogsByLectureIdAsync(
            string lectureId
        )
        {
            try
            {
                var url = $"{AppConfig.ApiBaseUrl}/lecture/download-log?lectureId={lectureId}";

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                req.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

                var res = await uploadHttp.SendAsync(req);

                if (!res.IsSuccessStatusCode)
                    return null;

                var json = await res.Content.ReadAsStringAsync();
                var obj = JObject.Parse(json);

                var data = obj["data"];
                if (data == null)
                    return null;

                var logs = data.ToObject<System.Collections.Generic.List<LectureDownloadLogDto>>();
                return logs;
            }
            catch
            {
                return null;
            }
        }


        // ========================================================
        // ================= UPDATE LECTURE ZIP ==================
        // ========================================================

        public class LectureDownloadLogDto
        {
            public string lectureId { get; set; } = default!;
            public string path { get; set; } = default!;
            public string type { get; set; } = default!;
        }
        public static async Task<bool> UpdateLectureZipAsync(
            string lectureId,
            string zipFilename
        )
        {
            zipHttp.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var body = new
            {
                zipFile = zipFilename
            };

            var json = JsonConvert.SerializeObject(body);
            var content =
                new StringContent(json, Encoding.UTF8, "application/json");

            var res = await zipHttp.PutAsync(
                $"{AppConfig.ApiBaseUrl}/lecture/{lectureId}",
                content
            );

            return res.IsSuccessStatusCode;
        }

        // ========================================================
        // =============== PROGRESS STREAM CONTENT ===============
        // ========================================================
        private class ProgressStreamContent : HttpContent
        {
            private readonly Stream _stream;
            private readonly int _bufferSize;
            private readonly IProgress<int>? _progress;

            public ProgressStreamContent(
                Stream stream,
                IProgress<int>? progress,
                int bufferSize = 81920
            )
            {
                _stream = stream;
                _bufferSize = bufferSize;
                _progress = progress;
                Headers.ContentLength = _stream.Length;
            }

            protected override async Task SerializeToStreamAsync(
                Stream target,
                TransportContext context
            )
            {
                var buffer = new byte[_bufferSize];
                long totalRead = 0;
                int read;

                while ((read = await _stream.ReadAsync(
                    buffer, 0, buffer.Length)) > 0)
                {
                    await target.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    int percent =
                        (int)(totalRead * 100 / _stream.Length);
                    _progress?.Report(percent);
                }
            }

            protected override bool TryComputeLength(out long length)
            {
                length = _stream.Length;
                return true;
            }
        }
    }
}
