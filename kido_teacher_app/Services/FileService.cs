using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Services
{
    /// <summary>
    /// Service x? lý upload/download files
    /// </summary>
    public static class FileService
    {
        private static readonly HttpClient client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(30)
        };

        // =====================================================
        // UPLOAD LECTURE ZIP
        // =====================================================
        // =====================================================
        // UPLOAD IMAGE
        // =====================================================
        public static async Task<string?> UploadImageAsync(string localPath)
        {
            if (!File.Exists(localPath))
                throw new FileNotFoundException($"File không t?n t?i: {localPath}");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            using var form = new MultipartFormDataContent();
            using var fs = File.OpenRead(localPath);
            using var fileContent = new StreamContent(fs);

            fileContent.Headers.ContentType =
                MediaTypeHeaderValue.Parse("image/*");

            form.Add(fileContent, "file", Path.GetFileName(localPath));

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/upload/large",
                form
            );

            if (!res.IsSuccessStatusCode)
                return null;

            var body = await res.Content.ReadAsStringAsync();
            var json = JObject.Parse(body);

            return json["data"]?["path"]?.ToString();
        }

        // =====================================================
        // VALIDATE FILE TYPE
        // =====================================================
        public static bool ValidateFileExtension(string filePath, string expectedExtension)
        {
            var ext = Path.GetExtension(filePath).ToLower();
            return ext == expectedExtension.ToLower();
        }

        // =====================================================
        // GET FILE SIZE STRING
        // =====================================================
        public static string GetFileSizeString(string filePath)
        {
            if (!File.Exists(filePath))
                return "0 B";

            long bytes = new FileInfo(filePath).Length;

            if (bytes < 1024)
                return $"{bytes} B";
            if (bytes < 1024 * 1024)
                return $"{bytes / 1024.0:F2} KB";
            if (bytes < 1024 * 1024 * 1024)
                return $"{bytes / (1024.0 * 1024):F2} MB";

            return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
        }

        // =====================================================
        // CHUNKED UPLOAD - INIT SESSION
        // =====================================================
        public static async Task<InitUploadResponse> InitChunkedUploadAsync(
            string fileName,
            long fileSize,
            int totalChunks,
            string? mimeType = null,
            string? fileType = null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var request = new InitUploadRequest
            {
                fileName = fileName,
                fileSize = fileSize,
                totalChunks = totalChunks,
                mimeType = mimeType,
                fileType = fileType
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/upload/init",
                content
            );

            var body = await res.Content.ReadAsStringAsync();


            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Init upload failed: HTTP {(int)res.StatusCode}\n{body}");
            }

            // Parse response - có thể là { data: { uploadId, ... } } hoặc { uploadId, ... }
            var jsonObj = JObject.Parse(body);
            
            string uploadId = null;
            string responseFileName = null;
            int responseTotalChunks = 0;

            // Thử parse từ data.uploadId trước
            if (jsonObj["data"] != null)
            {
                uploadId = jsonObj["data"]["uploadId"]?.ToString();
                responseFileName = jsonObj["data"]["fileName"]?.ToString();
                responseTotalChunks = jsonObj["data"]["totalChunks"]?.Value<int>() ?? 0;
            }
            // Nếu không có data, thử parse trực tiếp
            else if (jsonObj["uploadId"] != null)
            {
                uploadId = jsonObj["uploadId"]?.ToString();
                responseFileName = jsonObj["fileName"]?.ToString();
                responseTotalChunks = jsonObj["totalChunks"]?.Value<int>() ?? 0;
            }

            if (string.IsNullOrWhiteSpace(uploadId))
            {
                throw new Exception("Invalid init upload response: missing uploadId. Response: " + body);
            }

            var response = new InitUploadResponse
            {
                uploadId = uploadId,
                fileName = responseFileName ?? fileName,
                totalChunks = responseTotalChunks > 0 ? responseTotalChunks : totalChunks
            };
            return response;
        }

        // =====================================================
        // CHUNKED UPLOAD - UPLOAD CHUNK
        // =====================================================
        public static async Task<UploadChunkResponse> UploadChunkAsync(
            string uploadId,
            int chunkIndex,
            byte[] chunkData,
            int maxRetries = 3)
        {
            if (string.IsNullOrWhiteSpace(uploadId))
            {
                throw new ArgumentException("uploadId cannot be null or empty", nameof(uploadId));
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            Exception lastException = null;

            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var form = new MultipartFormDataContent();
                    using var chunkContent = new ByteArrayContent(chunkData);

                    chunkContent.Headers.ContentType =
                        MediaTypeHeaderValue.Parse("application/octet-stream");

                    // 🔥 QUAN TRỌNG: Field name phải là "chunk" (giống code mẫu)
                    // Tên file là "chunk.bin" để đơn giản
                    form.Add(chunkContent, "chunk", "chunk.bin");

                    var url = $"{AppConfig.ApiBaseUrl}/upload/chunk/{uploadId}/{chunkIndex}";
                    var res = await client.PostAsync(url, form);
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {

                        // Nếu là lỗi 4xx (client error), không retry
                        if ((int)res.StatusCode >= 400 && (int)res.StatusCode < 500)
                        {
                            throw new Exception($"Upload chunk {chunkIndex} failed: HTTP {(int)res.StatusCode}\nURL: {url}\nResponse: {body}");
                        }

                        // Nếu là lỗi 5xx (server error), retry
                        lastException = new Exception($"Upload chunk {chunkIndex} failed: HTTP {(int)res.StatusCode}\n{body}");
                        
                        if (attempt < maxRetries)
                        {
                            int delayMs = (attempt + 1) * 1000; // 1s, 2s, 3s
                            System.Diagnostics.Debug.WriteLine($"[FileService] Retrying chunk {chunkIndex} after {delayMs}ms...");
                            await Task.Delay(delayMs);
                            continue;
                        }
                        
                        throw lastException;
                    }
                    // Parse response - có thể là { data: { received, total } } hoặc { received, total }
                    var jsonObj = JObject.Parse(body);
                    
                    int received = 0;
                    int total = 0;

                    // Thử parse từ data trước
                    if (jsonObj["data"] != null)
                    {
                        received = jsonObj["data"]["received"]?.Value<int>() ?? 0;
                        total = jsonObj["data"]["total"]?.Value<int>() ?? 0;
                    }
                    // Nếu không có data, thử parse trực tiếp
                    else
                    {
                        received = jsonObj["received"]?.Value<int>() ?? 0;
                        total = jsonObj["total"]?.Value<int>() ?? 0;
                    }

                    var response = new UploadChunkResponse
                    {
                        received = received,
                        total = total
                    };
                    return response;
                }
                catch (HttpRequestException ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        int delayMs = (attempt + 1) * 1000;
                        await Task.Delay(delayMs);
                        continue;
                    }
                    
                    throw new Exception($"Network error uploading chunk {chunkIndex} after {maxRetries + 1} attempts: {ex.Message}", ex);
                }
                catch (TaskCanceledException ex)
                {
                    lastException = ex;
                    if (attempt < maxRetries)
                    {
                        int delayMs = (attempt + 1) * 1000;
                        await Task.Delay(delayMs);
                        continue;
                    }
                    
                    throw new Exception($"Timeout uploading chunk {chunkIndex} after {maxRetries + 1} attempts", ex);
                }
                catch (Exception ex) when (!(ex is ArgumentException))
                {
                    throw;
                }
            }

            throw lastException ?? new Exception($"Failed to upload chunk {chunkIndex} after {maxRetries + 1} attempts");
        }

        // =====================================================
        // CHUNKED UPLOAD - COMPLETE
        // =====================================================
        public static async Task<CompleteUploadResponse> CompleteChunkedUploadAsync(string uploadId)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            // Tạo JSON content với uploadId
            var requestBody = new
            {
                uploadId = uploadId
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/upload/complete",
                content
            );

            var body = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception($"Complete upload failed: HTTP {(int)res.StatusCode}\n{body}");
            }
            var apiResponse = JObject.Parse(body);

            // Parse response - có thể là { success, id, filename, ... } hoặc { data: { success, ... } }
            JToken dataNode;
            
            if (apiResponse["data"] != null)
            {
                // Trường hợp có wrapper "data"
                dataNode = apiResponse["data"];
            }
            else
            {
                // Trường hợp response trực tiếp (theo DTO extends UploadFileResponseDto)
                dataNode = apiResponse;
            }

            // Kiểm tra success flag
            var success = dataNode["success"]?.Value<bool>() ?? false;
            if (!success)
            {
                throw new Exception("Complete upload returned success=false");
            }

            // Lấy thông tin file (theo UploadFileResponseDto)
            var fileId = dataNode["id"]?.ToString();
            var filename = dataNode["filename"]?.ToString();
            var filePath = dataNode["path"]?.ToString();
            var fileSize = dataNode["size"]?.Value<long>() ?? 0;
            var url = dataNode["url"]?.ToString();
            var mimetype = dataNode["mimetype"]?.ToString() ?? dataNode["mimeType"]?.ToString();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new Exception("Complete upload response missing file path");
            }

            var response = new CompleteUploadResponse
            {
                success = success,
                path = filePath,
                url = url ?? "",
                fileName = filename ?? "",
                fileSize = fileSize,
                mimeType = mimetype ?? ""
            };
            return response;
        }

        // =====================================================
        // CHUNKED UPLOAD - FULL FLOW WITH PROGRESS
        // =====================================================
        public static async Task<string> UploadLargeFileAsync(
            string filePath,
            string? fileType = null,
            IProgress<int>? progress = null,
            int chunkSizeMB = 5)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File không tồn tại: {filePath}");

            var fileInfo = new FileInfo(filePath);
            var fileName = fileInfo.Name;
            var fileSize = fileInfo.Length;
            var chunkSize = chunkSizeMB * 1024 * 1024; // Convert to bytes
            var totalChunks = (int)Math.Ceiling((double)fileSize / chunkSize);
            progress?.Report(0);

            try
            {
                // =====================================================
                // BƯỚC 1: INIT UPLOAD SESSION
                // =====================================================
                var initResponse = await InitChunkedUploadAsync(
                    fileName,
                    fileSize,
                    totalChunks,
                    GetMimeType(fileName),
                    fileType
                );
                // =====================================================
                // BƯỚC 2: UPLOAD TỪNG CHUNK
                // =====================================================
                using (var fs = File.OpenRead(filePath))
                {
                    var buffer = new byte[chunkSize];
                    int chunkIndex = 0;

                    while (chunkIndex < totalChunks)
                    {
                        int bytesRead = await fs.ReadAsync(buffer, 0, chunkSize);

                        var chunkData = new byte[bytesRead];
                        Array.Copy(buffer, chunkData, bytesRead);
                        await UploadChunkAsync(initResponse.uploadId, chunkIndex, chunkData);

                        chunkIndex++;

                        // Report progress
                        int percent = (int)((double)chunkIndex / totalChunks * 100);
                        progress?.Report(percent);
                    }
                }
                // =====================================================
                // BƯỚC 3: COMPLETE UPLOAD
                // =====================================================
                var completeResponse = await CompleteChunkedUploadAsync(initResponse.uploadId);

                if (!completeResponse.success)
                {
                    throw new Exception("Complete upload failed: Server returned success=false");
                }
                progress?.Report(100);

                return completeResponse.path;
            }
            catch (Exception ex)
            {
                throw new Exception($"Loi upload file: {ex.Message}", ex);
            }
        }

        // =====================================================
        // HELPER - GET MIME TYPE
        // =====================================================
        private static string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".zip" => "application/zip",
                ".pdf" => "application/pdf",
                ".mp4" => "video/mp4",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                _ => "application/octet-stream"
            };
        }
    }

    // =====================================================
    // PROGRESS STREAM CONTENT
    // =====================================================
    internal class ProgressStreamContent : HttpContent
    {
        private readonly Stream _content;
        private readonly IProgress<int>? _progress;
        private const int BufferSize = 81920;

        public ProgressStreamContent(Stream content, IProgress<int>? progress)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
            _progress = progress;
        }

        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext context)
        {
            return Task.Run(async () =>
            {
                var buffer = new byte[BufferSize];
                long totalBytes = _content.Length;
                long uploadedBytes = 0;

                int bytesRead;
                while ((bytesRead = await _content.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await stream.WriteAsync(buffer, 0, bytesRead);
                    uploadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        int percent = (int)(uploadedBytes * 100 / totalBytes);
                        _progress?.Report(percent);
                    }
                }
            });
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _content?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
