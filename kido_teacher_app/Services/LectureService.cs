using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Shared.Caching;
using kido_teacher_app.Shared.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public sealed class DownloadStats
    {
        public long BytesRead { get; set; }
        public long TotalBytes { get; set; }
        public double SpeedMbps { get; set; }
        public int Percent { get; set; }
        public string Phase { get; set; } = "DOWNLOAD";
    }

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
            const string cacheKey = "lectures_all";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode) throw new Exception();

                var json = await res.Content.ReadAsStringAsync();

                var api = JsonConvert.DeserializeObject<
                    ApiResponse<Wrapper<List<LectureDto>>>>
                    (json);

                var data = api?.data?.data ?? new();
                var normalized = CacheImagePathNormalizer.NormalizeLecturesForCache(data);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                return cached ?? new();
            }
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
            var cacheKey = $"lectures_all_{courseId ?? "-"}_{groupId ?? "-"}_{(search ?? "-")}";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"[LectureService] HTTP {(int)res.StatusCode}: {res.StatusCode}");
                    throw new Exception();
                }

                var json = await res.Content.ReadAsStringAsync();

                var api = JsonConvert.DeserializeObject<
                    ApiResponse<Wrapper<List<LectureDto>>>>
                    (json);

                var result = api?.data?.data ?? new();
                System.Diagnostics.Debug.WriteLine($"[LectureService] Loaded {result.Count} lectures");

                var normalized = CacheImagePathNormalizer.NormalizeLecturesForCache(result);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return result;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                return cached ?? new();
            }
        }


        // lấy chi tiết bài học 
        public static async Task<LessonDto?> GetLectureByIdAsync(string id)
        {
            EnsureAuthorized();

            if (OfflineState.IsOffline())
                return null;

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

        // lấy bài học theo mã lớp học và mã khóa học 
        public static async Task<List<LectureDto>> GetByClassCourseAsync(
            string classId, 
            string courseId)
        {
            EnsureAuthorized();
            var url = $"{AppConfig.ApiBaseUrl}/lecture?page=1&size=1000" +
                      $"&courseId={courseId}&classId={classId}&isGetResource=true";
            var cacheKey = $"lectures_class_{classId}_course_{courseId}";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode)
                    throw new Exception();

                var json = await res.Content.ReadAsStringAsync();
                var api = JsonConvert.DeserializeObject<
                    ApiResponse<PagedResult<LectureDto>>
                >(json);

                var data = api?.data?.data ?? new();
                var normalized = CacheImagePathNormalizer.NormalizeLecturesForCache(data);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                return cached ?? new();
            }
        }
        // giải nén file zip bài học 
        // =====================================================
        // 🔥 DOWNLOAD & EXTRACT ZIP (SỬ DỤNG PATH TỪ API)
        // =====================================================
        public static Task<string?> DownloadAndExtractZipAsync(
            string resourcePath,
            string lectureId,
            IProgress<int>? progress = null)
        {
            return DownloadAndExtractZipAsync(resourcePath, lectureId, progress, null);
        }

        public static async Task<string?> DownloadAndExtractZipAsync(
            string resourcePath,
            string lectureId,
            IProgress<int>? progress,
            IProgress<DownloadStats>? statsProgress)
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
                const int downloadBufferSize = 1024 * 1024; // 1 MB buffer to improve throughput
                var buffer = new byte[downloadBufferSize];
                long read = 0;
                await using var input = await res.Content.ReadAsStreamAsync();
                await using var output = new FileStream(
                    tempZip,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    downloadBufferSize,
                    FileOptions.Asynchronous | FileOptions.SequentialScan
                );
                var sw = System.Diagnostics.Stopwatch.StartNew();
                int len;
                while ((len = await input.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await output.WriteAsync(buffer, 0, len);
                    read += len;
                    int percent = 0;
                    if (total > 0)
                    {
                        percent = (int)(read * 50 / total);
                        progress?.Report(percent);
                    }
                    var seconds = sw.Elapsed.TotalSeconds;
                    var speed = seconds > 0 ? (read / (1024d * 1024d)) / seconds : 0;
                    statsProgress?.Report(new DownloadStats
                    {
                        BytesRead = read,
                        TotalBytes = total,
                        SpeedMbps = speed,
                        Percent = percent,
                        Phase = "DOWNLOAD"
                    });
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
                    statsProgress?.Report(new DownloadStats
                    {
                        BytesRead = 0,
                        TotalBytes = 0,
                        SpeedMbps = 0,
                        Percent = percent,
                        Phase = "EXTRACT"
                    });
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

