using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

        private static void EnsureAuthorized()
        {
            if (string.IsNullOrEmpty(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token không tồn tại");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }

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
        public static List<LectureDto> NormalizeLectures(IEnumerable<LectureDto>? lectures)
        {
            return (lectures ?? Enumerable.Empty<LectureDto>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.id))
                .GroupBy(x => x.id)
                .Select(g => g.First())
                .OrderBy(x => x.orderColumn)
                .ThenBy(x => x.code)
                .ToList();
        }

        public static async Task<List<LectureDto>> GetAllAsync()
        {
            EnsureAuthorized();

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
            EnsureAuthorized();

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
            EnsureAuthorized();

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
        // lấy bài học theo mã lớp học và mã khóa học 
        public static async Task<List<LectureDto>> GetByClassCourseAsync(
            string classId, 
            string courseId)
        {
            EnsureAuthorized();
            var url = $"{AppConfig.ApiBaseUrl}/lecture?page=1&size=1000" +
                      $"&courseId={courseId}&classId={classId}";
            var res = await client.GetAsync(url);
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
            // Lấy tên file từ path (trước khi encode URL)
            var zipFilename = Path.GetFileName(resourcePath);
            // Xác định URL: nếu đã là full URL thì dùng luôn, nếu là path thì gắn ApiBaseUrl
            string url;
            if (resourcePath.StartsWith("http://") || resourcePath.StartsWith("https://"))
            {
                // Đã là full URL
                url = resourcePath;
            }
            else
            {
                // Là relative path - cần gắn ApiBaseUrl
                var baseUrl = AppConfig.ApiBaseUrl.TrimEnd('/');
                // Đảm bảo có dấu / ở đầu path
                if (!resourcePath.StartsWith("/"))
                    resourcePath = "/" + resourcePath;
                // Encode các ký tự đặc biệt trong path (nhưng giữ nguyên dấu /)
                var segments = resourcePath.Split('/');
                var encodedSegments = segments.Select(s => 
                    string.IsNullOrEmpty(s) ? s : Uri.EscapeDataString(s)
                );
                var encodedPath = string.Join("/", encodedSegments);
                url = $"{baseUrl}{encodedPath}";
            }
            // Đảm bảo thư mục Downloads tồn tại
            if (!Directory.Exists(AppConfig.DownloadFolder))
                Directory.CreateDirectory(AppConfig.DownloadFolder);
            // Lưu file ZIP vào thư mục Downloads
            var tempZip = Path.Combine(AppConfig.DownloadFolder, zipFilename);
            // ⭐ Giải nén vào thư mục Lectures/{lectureId}
            var extractRoot = Path.Combine(
                AppConfig.LectureExtractFolder,
                lectureId
            );
            if (Directory.Exists(extractRoot))
                Directory.Delete(extractRoot, true);
            Directory.CreateDirectory(extractRoot);
            // ======================
            // 1️⃣ DOWNLOAD (0–50%)
            // ======================
            using (var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                res.EnsureSuccessStatusCode();
                var total = res.Content.Headers.ContentLength ?? 0;
                var buffer = new byte[81920];
                long read = 0;
                await using var input = await res.Content.ReadAsStreamAsync();
                await using var output = new FileStream(tempZip, FileMode.Create, FileAccess.Write);
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
            // ======================
            // 2️⃣ EXTRACT (50–100%) - BỎ QUA THỨ MỤC GỐC
            // ======================
            using (var zip = ZipFile.OpenRead(tempZip))
            {
                int total = zip.Entries.Count;
                int current = 0;
                // Tìm thư mục gốc chung (nếu có)
                string? commonRoot = null;
                var firstEntry = zip.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.Name));
                if (firstEntry != null)
                {
                    var parts = firstEntry.FullName.Split('/');
                    if (parts.Length > 1)
                    {
                        // Kiểm tra xem tất cả entries có cùng thư mục gốc không
                        commonRoot = parts[0] + "/";
                        bool allHaveCommonRoot = zip.Entries
                            .Where(e => !string.IsNullOrEmpty(e.Name))
                            .All(e => e.FullName.StartsWith(commonRoot));
                        if (!allHaveCommonRoot)
                            commonRoot = null;
                    }
                }
                foreach (var entry in zip.Entries)
                {
                    // bỏ thư mục rỗng
                    if (string.IsNullOrEmpty(entry.Name))
                        continue;
                    // Bỏ qua thư mục gốc nếu tìm thấy
                    var relativePath = entry.FullName;
                    if (!string.IsNullOrEmpty(commonRoot) && relativePath.StartsWith(commonRoot))
                    {
                        relativePath = relativePath.Substring(commonRoot.Length);
                    }
                    // Bỏ qua nếu path rỗng sau khi remove root
                    if (string.IsNullOrWhiteSpace(relativePath))
                        continue;
                    var destinationPath = Path.GetFullPath(
                        Path.Combine(extractRoot, relativePath)
                    );
                    // 🔐 bảo vệ path traversal
                    if (!destinationPath.StartsWith(extractRoot, StringComparison.OrdinalIgnoreCase))
                        continue;
                    var dir = Path.GetDirectoryName(destinationPath);
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir!);
                    entry.ExtractToFile(destinationPath, true);
                    current++;
                    int percent = 50 + (int)(current * 50.0 / total);
                    progress?.Report(percent);
                    await Task.Yield();
                }
            }

            progress?.Report(100);
            // ======================
            // 3️⃣ XÓA FILE ZIP SAU KHI GIẢI NÉN XONG
            // ======================
            try
            {
                if (File.Exists(tempZip))
                {
                    File.Delete(tempZip);
                }
            }
            catch (Exception ex)
            {
            }
            return extractRoot;
        }
        public static async Task<LectureDto?> GetByIdAsync(string lectureId)
        {
            EnsureAuthorized();
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

        // =====================================================
        // GET MAX CODE
        // =====================================================
        public static async Task<string> GetMaxCodeAsync()
        {
            EnsureAuthorized();
            // ⭐ Dùng Service chung
            return await Shared.Common.GetMaxCodeService.GetMaxCodeAsync(client, ApiRoutes.LECTURES_MAX_CODE);
        }
    }
}
