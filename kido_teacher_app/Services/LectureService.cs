using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class LectureService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };

        // =====================================================
        // CREATE LECTURE
        // =====================================================
        public static async Task<bool> CreateAsync(LectureCreateDto dto)
        {
            EnsureAuthorized();

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiRoutes.LECTURES, content);
            var text = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {text}");

            return true;
        }

        // =====================================================
        // GET ALL LECTURES
        // =====================================================
        public static async Task<List<LectureDto>> GetAllAsync()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.LECTURES}?page=1&size=1000";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<Wrapper<List<LectureDto>>>>
                (json);

            return api?.data?.data ?? new();
        }

        // =====================================================
        // GET LECTURES BY COURSE (OVERLOAD)
        // =====================================================
        public static async Task<List<LectureDto>> GetAllAsync(
            string? courseId = null, 
            string? groupId = null, 
            string? search = null)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var queryParams = new List<string> { "page=1", "size=1000" };

            if (!string.IsNullOrEmpty(courseId))
                queryParams.Add($"courseId={courseId}");

            if (!string.IsNullOrEmpty(groupId))
                queryParams.Add($"groupId={groupId}");

            if (!string.IsNullOrEmpty(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            var query = string.Join("&", queryParams);
            var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.LECTURES}?{query}";

            System.Diagnostics.Debug.WriteLine($"[LectureService] GetAllAsync URL: {url}");

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[LectureService] HTTP {(int)res.StatusCode}: {res.StatusCode}");
                return new();
            }

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<Wrapper<List<LectureDto>>>>
                (json);

            var result = api?.data?.data ?? new();
            System.Diagnostics.Debug.WriteLine($"[LectureService] Loaded {result.Count} lectures");

            return result;
        }


        // lấy chi tiết bài học 
        public static async Task<LessonDto?> GetLectureByIdAsync(string id)
        {
            var res = await client.GetAsync($"/lecture/{id}");
            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<ApiWrapper>(json);

            return wrapper?.data;
        }

        class ApiWrapper
        {
            public LessonDto data { get; set; }
        }

        // cập nhật bài học 
        public static async Task<bool> UpdateAsync(string id, LectureCreateDto dto)
        {
            EnsureAuthorized();   // ⭐ thêm dòng này

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PutAsync($"/lecture/{id}", content);

            return res.IsSuccessStatusCode;
        }
        // xóa lớp học
        public static async Task<bool> DeleteAsync(string id)
        {
            var res = await client.DeleteAsync($"/lecture/{id}");
            return res.IsSuccessStatusCode;
        }
        // =====================================================
        // AUTH CHECK (GIỐNG UserService)
        // =====================================================
        private static void EnsureAuthorized()
        {
            if (string.IsNullOrEmpty(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token không tồn tại");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }

        // lấy bài học theo mã lớp học và mã khóa học 
        public static async Task<List<LectureDto>> GetByClassCourseAsync(
    string classId, string courseId)
        {
            //MessageBox.Show("ĐÃ VÀO HÀM GetByClassCourseAsync");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var url = $"{AppConfig.ApiBaseUrl}/lecture?page=1&size=1000" +
                      $"&courseId={courseId}&classId={classId}";

            var res = await client.GetAsync(url);

            //MessageBox.Show("STATUS: " + res.StatusCode);

            if (!res.IsSuccessStatusCode)
                return new();

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<PagedResult<LectureDto>>
            >(json);

            return api?.data?.data ?? new();
        }


        // giải nén file zip bài học 
        // =====================================================
        // 🔥 DOWNLOAD & EXTRACT ZIP (SỬ DỤNG PATH TỪ API)
        // =====================================================
        public static async Task<string?> DownloadAndExtractZipAsync(
            string resourcePath,
            string lectureId,
            IProgress<int>? progress = null)
        {
            EnsureAuthorized();

            string url = BuildResourceUrl(resourcePath);
            System.Diagnostics.Debug.WriteLine($"[LectureService] Download URL: {url}");

            if (!Directory.Exists(AppConfig.DownloadFolder))
                Directory.CreateDirectory(AppConfig.DownloadFolder);

            var extractRoot = Path.Combine(
                AppConfig.LectureExtractFolder,
                lectureId
            );
            System.Diagnostics.Debug.WriteLine($"[LectureService] Extract to: {extractRoot}");

            Directory.CreateDirectory(extractRoot);

            string? tempFile = null;
            string? downloadFileName = null;
            string? mediaType = null;

            using (var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                res.EnsureSuccessStatusCode();

                mediaType = res.Content.Headers.ContentType?.MediaType;
                downloadFileName = ResolveDownloadFileName(resourcePath, res.Content.Headers);
                tempFile = Path.Combine(AppConfig.DownloadFolder, downloadFileName);
                System.Diagnostics.Debug.WriteLine($"[LectureService] Download to: {tempFile}");

                var total = res.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[81920];
                long read = 0;

                await using var input = await res.Content.ReadAsStreamAsync();
                await using var output = new FileStream(tempFile, FileMode.Create, FileAccess.Write);

                int len;
                while ((len = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, len);
                    read += len;

                    if (total > 0)
                    {
                        int percent = (int)(read * 50 / total);
                        progress?.Report(percent);
                    }
                }
            }

            if (string.IsNullOrEmpty(tempFile) || string.IsNullOrEmpty(downloadFileName))
            {
                return null;
            }

            if (IsZipResource(downloadFileName, mediaType))
            {
                await ExtractZipAsync(tempFile, extractRoot, progress);
                System.Diagnostics.Debug.WriteLine($"[LectureService] Extract completed: {extractRoot}");
            }
            else
            {
                var destinationPath = Path.Combine(extractRoot, downloadFileName);
                File.Copy(tempFile, destinationPath, true);
                progress?.Report(100);
                System.Diagnostics.Debug.WriteLine($"[LectureService] Saved file: {destinationPath}");
            }

            progress?.Report(100);

            try
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                    System.Diagnostics.Debug.WriteLine($"[LectureService] Deleted temp file: {tempFile}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LectureService] Failed to delete temp file: {ex.Message}");
            }

            return extractRoot;
        }

        private static string BuildResourceUrl(string resourcePath)
        {
            if (resourcePath.StartsWith("http://") || resourcePath.StartsWith("https://"))
            {
                return resourcePath;
            }

            var baseUrl = AppConfig.ApiBaseUrl.TrimEnd('/');

            if (!resourcePath.StartsWith("/"))
                resourcePath = "/" + resourcePath;

            var segments = resourcePath.Split('/');
            var encodedSegments = segments.Select(s =>
                string.IsNullOrEmpty(s) ? s : Uri.EscapeDataString(s)
            );
            var encodedPath = string.Join("/", encodedSegments);

            return $"{baseUrl}{encodedPath}";
        }

        private static string ResolveDownloadFileName(
            string resourcePath,
            HttpContentHeaders headers)
        {
            var fileName =
                headers.ContentDisposition?.FileNameStar ??
                headers.ContentDisposition?.FileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                fileName = fileName.Trim('"');
            }
            else
            {
                fileName = Path.GetFileName(resourcePath.Split('?')[0]);
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = "resource";
            }

            if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
            {
                var extension = GetExtensionFromMediaType(headers.ContentType?.MediaType);
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    fileName += extension;
                }
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        private static string? GetExtensionFromMediaType(string? mediaType)
        {
            return mediaType?.ToLowerInvariant() switch
            {
                "application/zip" => ".zip",
                "application/x-zip-compressed" => ".zip",
                "application/pdf" => ".pdf",
                "video/mp4" => ".mp4",
                "text/html" => ".html",
                _ => null
            };
        }

        private static bool IsZipResource(string fileName, string? mediaType)
        {
            return Path.GetExtension(fileName)
                    .Equals(".zip", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mediaType, "application/zip", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mediaType, "application/x-zip-compressed", StringComparison.OrdinalIgnoreCase);
        }

        private static async Task ExtractZipAsync(
            string zipPath,
            string extractRoot,
            IProgress<int>? progress)
        {
            string extractRootFullPath = Path.GetFullPath(extractRoot);
            if (!extractRootFullPath.EndsWith(Path.DirectorySeparatorChar))
            {
                extractRootFullPath += Path.DirectorySeparatorChar;
            }

            using var zip = ZipFile.OpenRead(zipPath);
            int total = zip.Entries.Count;
            int current = 0;

            string? commonRoot = null;
            var firstEntry = zip.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.Name));
            if (firstEntry != null)
            {
                var parts = firstEntry.FullName.Split('/');
                if (parts.Length > 1)
                {
                    commonRoot = parts[0] + "/";
                    bool allHaveCommonRoot = zip.Entries
                        .Where(e => !string.IsNullOrEmpty(e.Name))
                        .All(e => e.FullName.StartsWith(commonRoot, StringComparison.Ordinal));

                    if (!allHaveCommonRoot)
                        commonRoot = null;
                }
            }

            System.Diagnostics.Debug.WriteLine($"[LectureService] Common root folder: {commonRoot ?? "(none)"}");

            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                var relativePath = entry.FullName;
                if (!string.IsNullOrEmpty(commonRoot) &&
                    relativePath.StartsWith(commonRoot, StringComparison.Ordinal))
                {
                    relativePath = relativePath.Substring(commonRoot.Length);
                }

                if (string.IsNullOrWhiteSpace(relativePath))
                    continue;

                var destinationPath = Path.GetFullPath(
                    Path.Combine(extractRootFullPath, relativePath)
                );

                if (!destinationPath.StartsWith(extractRootFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                var dir = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                entry.ExtractToFile(destinationPath, true);

                current++;
                int percent = total == 0
                    ? 100
                    : 50 + (int)(current * 50.0 / total);
                progress?.Report(percent);

                await Task.Yield();
            }
        }

        

        public static async Task<LectureDto?> GetByIdAsync(string lectureId)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.LectureById(lectureId)}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<LectureDto>
            >(json);

            return api?.data;
        }

        // =====================================================
        // BULK ASSIGN LECTURES TO USERS
        // =====================================================
        public static async Task<bool> BulkAssignToUsersAsync(
            List<string> userIds, 
            List<string> lectureIds, 
            DateTime startDate, 
            DateTime endDate)
        {
            try
            {
                EnsureAuthorized();

                var payload = new
                {
                    userIds = userIds,
                    lectureIds = lectureIds,
                    startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(ApiRoutes.LECTURE_BULK_ASSIGN_USERS, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[LectureService] BulkAssign failed: HTTP {(int)response.StatusCode}: {responseText}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LectureService] BulkAssign exception: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // BULK ASSIGN LECTURES TO GROUPS
        // =====================================================
        public static async Task<bool> BulkAssignToGroupsAsync(
            List<string> groupIds, 
            List<string> lectureIds, 
            DateTime startDate, 
            DateTime endDate)
        {
            try
            {
                EnsureAuthorized();

                var payload = new
                {
                    groupIds = groupIds,
                    lectureIds = lectureIds,
                    startDate = startDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    endDate = endDate.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(ApiRoutes.LECTURE_BULK_ASSIGN_GROUPS, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[LectureService] BulkAssignToGroups failed: HTTP {(int)response.StatusCode}: {responseText}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LectureService] BulkAssignToGroups exception: {ex.Message}");
                throw;
            }
        }
    }
}
