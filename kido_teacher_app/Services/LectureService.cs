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
            try
            {
                using (var res = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    res.EnsureSuccessStatusCode();
                    var total = res.Content.Headers.ContentLength ?? 0;
                    const int downloadBufferSize = 1024 * 1024; // 1 MB buffer to improve throughput
                    var buffer = new byte[downloadBufferSize];
                    long read = 0;
                    using var input = await res.Content.ReadAsStreamAsync();
                    using var output = new FileStream(
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
            }
            catch (Exception exDownload)
            {
                System.Diagnostics.Debug.WriteLine($"[Download] Failed to download ZIP: {exDownload.Message}");
                throw new Exception($"Lỗi tải file: {exDownload.Message}", exDownload);
            }

            // Verify ZIP file was downloaded successfully
            if (!File.Exists(tempZip))
            {
                throw new Exception("Lỗi: File ZIP không được tải thành công");
            }

            var fileInfo = new FileInfo(tempZip);
            if (fileInfo.Length == 0)
            {
                throw new Exception("Lỗi: File ZIP trống (kích thước = 0)");
            }

            System.Diagnostics.Debug.WriteLine($"[Download] Successfully downloaded ZIP: {tempZip} ({fileInfo.Length} bytes)");
            // ======================
            // 2️⃣ EXTRACT (50–100%) - BỎ QUA THỨ MỤC GỐC
            // ======================
            try
            {
                using (var zip = ZipFile.OpenRead(tempZip))
                {
                    int total = zip.Entries.Count;
                    int current = 0;
                    var commonRoot = GetSharedRootFolder(zip.Entries);
                    var wrapperFolders = GetWrapperFoldersToStrip(zip.Entries, commonRoot);

                    foreach (var entry in zip.Entries)
                    {
                        // bỏ thư mục rỗng
                        if (string.IsNullOrEmpty(entry.Name))
                            continue;
                        // Bỏ qua thư mục gốc nếu tìm thấy
                        var normalizedFullName = entry.FullName.Replace('\\', '/');
                        var relativePath = normalizedFullName;
                        if (!string.IsNullOrEmpty(commonRoot) && relativePath.StartsWith(commonRoot))
                        {
                            relativePath = relativePath.Substring(commonRoot.Length);
                        }

                        relativePath = NormalizeExtractRelativePath(relativePath, wrapperFolders);

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
                        
                        try
                        {
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
                        }
                        catch (Exception exEntry)
                        {
                            System.Diagnostics.Debug.WriteLine($"[Extract] Failed to extract '{entry.Name}': {exEntry.Message}");
                            // Continue với file tiếp theo thay vì fail toàn bộ
                        }
                        
                        await Task.Yield();
                    }
                }
            }
            catch (Exception exZip)
            {
                System.Diagnostics.Debug.WriteLine($"[Extract] Failed to open ZIP: {exZip.Message}");
                throw new Exception($"Lỗi giải nén file: {exZip.Message}", exZip);
            }

            progress?.Report(100);
            // ======================
            // 3️⃣ VERIFY GIẢI NÉN THÀNH CÔNG
            // ======================
            // Kiểm tra thư mục extract có file nào không
            if (!Directory.Exists(extractRoot))
            {
                System.Diagnostics.Debug.WriteLine($"[Extract] Extract folder does not exist: {extractRoot}");
                throw new Exception("Lỗi: Thư mục giải nén không tồn tại");
            }

            var filesInExtract = Directory.GetFiles(extractRoot, "*.*", SearchOption.AllDirectories);
            if (filesInExtract.Length == 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Extract] No files found in extract folder: {extractRoot}");
                throw new Exception("Lỗi: Không có file nào được giải nén từ ZIP");
            }

            System.Diagnostics.Debug.WriteLine($"[Extract] Successfully extracted {filesInExtract.Length} files to: {extractRoot}");

            // ======================
            // 4️⃣ VALIDATE FILES AFTER EXTRACT
            // ======================
            ValidateExtractedFiles(filesInExtract, extractRoot);

            // ======================
            // 5️⃣ TEST OPEN FILES
            // ======================
            TestOpenExtractedFiles(extractRoot);

            // ======================
            // 6️⃣ XÓA FILE ZIP SAU KHI GIẢI NÉN XONG
            // ======================
            try
            {
                if (File.Exists(tempZip))
                {
                    File.Delete(tempZip);
                    System.Diagnostics.Debug.WriteLine($"[Download] Deleted temporary ZIP file: {tempZip}");
                }
            }
            catch (Exception exDelete)
            {
                System.Diagnostics.Debug.WriteLine($"[Download] Warning: Failed to delete ZIP file: {exDelete.Message}");
                // Không throw exception nếu xóa file thất bại
            }
            return extractRoot;
        }

        private static string? GetSharedRootFolder(IEnumerable<ZipArchiveEntry> entries)
        {
            string? rootFolder = null;

            foreach (var entry in entries.Where(e => !string.IsNullOrEmpty(e.Name)))
            {
                var normalized = entry.FullName.Replace('\\', '/');
                var parts = normalized.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2)
                    return null;

                if (string.IsNullOrWhiteSpace(parts[0]))
                    return null;

                if (rootFolder == null)
                {
                    rootFolder = parts[0];
                    continue;
                }

                if (!string.Equals(rootFolder, parts[0], StringComparison.Ordinal))
                    return null;
            }

            return string.IsNullOrWhiteSpace(rootFolder) ? null : rootFolder + "/";
        }

        private static HashSet<string> GetWrapperFoldersToStrip(
            IEnumerable<ZipArchiveEntry> entries,
            string? commonRoot)
        {
            var wrapperFolders = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in entries.Where(e => !string.IsNullOrEmpty(e.Name)))
            {
                var relativePath = entry.FullName.Replace('\\', '/');
                if (!string.IsNullOrEmpty(commonRoot) && relativePath.StartsWith(commonRoot, StringComparison.Ordinal))
                    relativePath = relativePath.Substring(commonRoot.Length);

                var parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || IsReservedElearningFolder(parts[0]))
                    continue;

                if (IsLectureEntryPoint(parts[1]))
                    wrapperFolders.Add(parts[0]);
            }

            return wrapperFolders;
        }

        private static string NormalizeExtractRelativePath(
            string relativePath,
            ISet<string> wrapperFolders)
        {
            var parts = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
                return relativePath;

            if (!wrapperFolders.Contains(parts[0]))
                return relativePath;

            return string.Join("/", parts.Skip(1));
        }

        private static bool IsReservedElearningFolder(string folderName)
        {
            return
                string.Equals(folderName, "html5", StringComparison.OrdinalIgnoreCase)
                || string.Equals(folderName, "mobile", StringComparison.OrdinalIgnoreCase)
                || string.Equals(folderName, "story_content", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLectureEntryPoint(string fileName)
        {
            return
                string.Equals(fileName, "story.html", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Path.GetExtension(fileName), ".mp4", StringComparison.OrdinalIgnoreCase)
                || IsStandaloneLectureDocument(fileName);
        }

        private static bool IsStandaloneLectureDocument(string filePath)
        {
            var extension = Path.GetExtension(filePath);

            return
                string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".pptx", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".ppsx", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".ppt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(extension, ".pps", StringComparison.OrdinalIgnoreCase);
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

        // =====================================================
        // VALIDATE EXTRACTED FILES
        // =====================================================
        private static void ValidateExtractedFiles(string[] filesInExtract, string extractRoot)
        {
            if (filesInExtract.Length == 0)
                return;

            int validFiles = 0;
            int emptyFiles = 0;
            int inaccessibleFiles = 0;
            var problematicFiles = new List<string>();

            foreach (var filePath in filesInExtract)
            {
                try
                {
                    var fileInfo = new FileInfo(filePath);

                    // Check if file is empty
                    if (fileInfo.Length == 0)
                    {
                        emptyFiles++;
                        problematicFiles.Add($"{Path.GetFileName(filePath)} (size=0)");
                        System.Diagnostics.Debug.WriteLine($"[Validate] Empty file: {filePath}");
                        continue;
                    }

                    // Try to read file to check if accessible
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Just try to read a few bytes to verify access
                        var buffer = new byte[Math.Min(1024, (int)stream.Length)];
                        var bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            inaccessibleFiles++;
                            problematicFiles.Add(Path.GetFileName(filePath));
                            System.Diagnostics.Debug.WriteLine($"[Validate] Cannot read file: {filePath}");
                            continue;
                        }
                    }

                    validFiles++;
                }
                catch (Exception ex)
                {
                    inaccessibleFiles++;
                    problematicFiles.Add(Path.GetFileName(filePath));
                    System.Diagnostics.Debug.WriteLine($"[Validate] Error validating '{filePath}': {ex.Message}");
                }
            }

            // Log validation summary
            System.Diagnostics.Debug.WriteLine(
                $"[Validate] Summary - Valid: {validFiles}, Empty: {emptyFiles}, Inaccessible: {inaccessibleFiles}");

            // If too many files are problematic, throw exception
            if (inaccessibleFiles > 0 || emptyFiles > filesInExtract.Length * 0.5)
            {
                var msg = new System.Text.StringBuilder();
                msg.AppendLine("Lỗi: Nhiều file giải nén không hợp lệ:");
                msg.AppendLine($"  - Valid: {validFiles}/{filesInExtract.Length}");
                msg.AppendLine($"  - Empty: {emptyFiles}");
                msg.AppendLine($"  - Inaccessible: {inaccessibleFiles}");
                
                if (problematicFiles.Count > 0 && problematicFiles.Count <= 10)
                {
                    msg.AppendLine("  - Files:");
                    foreach (var f in problematicFiles.Take(10))
                        msg.AppendLine($"    • {f}");
                }

                System.Diagnostics.Debug.WriteLine("[Validate] " + msg.ToString());
                throw new Exception(msg.ToString());
            }

            if (validFiles == 0)
            {
                throw new Exception("Lỗi: Không có file nào hợp lệ sau giải nén");
            }

            System.Diagnostics.Debug.WriteLine($"[Validate] All files validated successfully: {validFiles} files");
        }

        // =====================================================
        // TEST OPEN EXTRACTED FILES
        // =====================================================
        private static void TestOpenExtractedFiles(string extractRoot)
        {
            try
            {
                var resourceService = new LectureResourceService();
                var mappedFiles = resourceService.MapLectureFiles(extractRoot);

                System.Diagnostics.Debug.WriteLine("[TestOpen] Testing extracted files...");

                int testedFiles = 0;
                int successfulFiles = 0;
                var failedFiles = new List<string>();

                // Test PDF
                if (!string.IsNullOrEmpty(mappedFiles.PdfPath))
                {
                    testedFiles++;
                    if (CanOpenFile(mappedFiles.PdfPath, "PDF"))
                        successfulFiles++;
                    else
                        failedFiles.Add($"PDF: {Path.GetFileName(mappedFiles.PdfPath)}");
                }

                // Test Video
                if (!string.IsNullOrEmpty(mappedFiles.VideoPath))
                {
                    testedFiles++;
                    if (CanOpenFile(mappedFiles.VideoPath, "VIDEO"))
                        successfulFiles++;
                    else
                        failedFiles.Add($"VIDEO: {Path.GetFileName(mappedFiles.VideoPath)}");
                }

                // Test E-Learning
                if (!string.IsNullOrEmpty(mappedFiles.ElearningPath))
                {
                    testedFiles++;
                    if (CanOpenFile(mappedFiles.ElearningPath, "ELEARNING"))
                        successfulFiles++;
                    else
                        failedFiles.Add($"E-LEARNING: {Path.GetFileName(mappedFiles.ElearningPath)}");
                }

                // Test PowerPoint
                if (!string.IsNullOrEmpty(mappedFiles.PowerPointPath))
                {
                    testedFiles++;
                    if (CanOpenFile(mappedFiles.PowerPointPath, "POWERPOINT"))
                        successfulFiles++;
                    else
                        failedFiles.Add($"POWERPOINT: {Path.GetFileName(mappedFiles.PowerPointPath)}");
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[TestOpen] Test result: {successfulFiles}/{testedFiles} files opened successfully");

                // If some files failed to open, log warning
                if (failedFiles.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("[TestOpen] Warning - Failed to open:");
                    foreach (var f in failedFiles)
                        System.Diagnostics.Debug.WriteLine($"  - {f}");
                }

                // Fail if no files can be opened
                if (testedFiles > 0 && successfulFiles == 0)
                {
                    throw new Exception("Lỗi: Không thể mở bất kỳ file nào được giải nén");
                }

                // Fail if too many files failed
                if (testedFiles > 0 && successfulFiles < testedFiles * 0.5)
                {
                    var msg = new System.Text.StringBuilder();
                    msg.AppendLine($"Lỗi: Quá nhiều file không thể mở ({testedFiles - successfulFiles}/{testedFiles}):");
                    foreach (var f in failedFiles.Take(5))
                        msg.AppendLine($"  - {f}");
                    throw new Exception(msg.ToString());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TestOpen] Error: {ex.Message}");
                throw;
            }
        }

        // =====================================================
        // TEST SINGLE FILE OPEN
        // =====================================================
        private static bool CanOpenFile(string filePath, string fileType)
        {
            try
            {
                // Verify file exists and has content
                if (!File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[TestOpen] {fileType} file not found: {filePath}");
                    return false;
                }

                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[TestOpen] {fileType} file is empty: {filePath}");
                    return false;
                }

                // Try to open and read file
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var buffer = new byte[Math.Min(4096, (int)stream.Length)];
                    var bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[TestOpen] {fileType} file cannot be read: {filePath}");
                        return false;
                    }

                    // Additional check for specific file types
                    if (!ValidateFileContent(buffer, bytesRead, fileType))
                    {
                        System.Diagnostics.Debug.WriteLine($"[TestOpen] {fileType} file has invalid content: {filePath}");
                        return false;
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[TestOpen] ✓ {fileType} file OK: {Path.GetFileName(filePath)} ({fileInfo.Length} bytes)");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TestOpen] {fileType} file error: {ex.Message}");
                return false;
            }
        }

        // =====================================================
        // VALIDATE FILE CONTENT BY MAGIC BYTES
        // =====================================================
        private static bool ValidateFileContent(byte[] buffer, int bytesRead, string fileType)
        {
            if (bytesRead < 4)
                return true; // Not enough bytes to check

            try
            {
                switch (fileType.ToUpperInvariant())
                {
                    case "PDF":
                        // PDF magic bytes: %PDF
                        return buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46;

                    case "VIDEO":
                        // Accept common video formats - MP4 (ftyp), MKV (1A45DF A3), AVI (RIFF), etc.
                        // MP4: ftyp signature
                        if (bytesRead >= 8)
                        {
                            if (buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70)
                                return true; // MP4
                            // MKV: 1A 45 DF A3
                            if (buffer[0] == 0x1A && buffer[1] == 0x45 && buffer[2] == 0xDF && buffer[3] == 0xA3)
                                return true; // MKV
                        }
                        // AVI: RIFF header
                        if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
                            return true;
                        // Accept unknown video formats - just check it's readable
                        return true;

                    case "ELEARNING":
                        // HTML files start with < (3C) or whitespace
                        return buffer[0] == 0x3C || buffer[0] == 0x20 || buffer[0] == 0x09 || buffer[0] == 0x0A || buffer[0] == 0x0D;

                    case "POWERPOINT":
                        // PPTX: ZIP format (PK\x03\x04)
                        if (buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04)
                            return true;
                        // ODP (OpenDocument): also ZIP
                        if (buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04)
                            return true;
                        return true; // Accept unknown presentation formats

                    default:
                        return true; // Unknown type, assume valid
                }
            }
            catch
            {
                return true; // If magic byte check fails, assume valid
            }
        }
    }
}

