using kido_teacher_app.Model;
using kido_teacher_app.Shared.Caching;
using kido_teacher_app.Shared.Network;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class OfflinePrefetchService
    {
        public static async Task<(bool Success, string Message)> PrefetchTeacherOfflineAsync(
            bool prefetchImages = true,
            IProgress<string>? statusProgress = null)
        {
            if (OfflineState.IsOffline())
                return (false, "Không có kết nối mạng hoặc API đang không phản hồi.");

            try
            {
                statusProgress?.Report("Đang tải cache lớp...");
                var classes = await ClassService.GetAllAsync();
                if (classes == null || classes.Count == 0)
                    return (false, "Không lấy được danh sách lớp để cache.");

                var cachedClassCount = 0;
                var cachedCourseCount = 0;
                var cachedLectureCount = 0;

                foreach (var cls in classes)
                {
                    if (string.IsNullOrWhiteSpace(cls?.id))
                        continue;

                    cachedClassCount++;

                    if (prefetchImages)
                    {
                        statusProgress?.Report("Đang tải cache hình ảnh lớp...");
                        await TryPrefetchClassImageAsync(cls);
                    }

                    statusProgress?.Report("Đang tải cache khóa học...");
                    var courses = await CourseService.GetByClassIdAsync(cls.id);
                    if (courses == null || courses.Count == 0)
                        continue;

                    foreach (var course in courses)
                    {
                        if (string.IsNullOrWhiteSpace(course?.id))
                            continue;

                        cachedCourseCount++;

                        if (prefetchImages)
                        {
                            statusProgress?.Report("Đang tải cache hình ảnh khóa học...");
                            await TryPrefetchCourseImageAsync(course, cls.id);
                        }

                        statusProgress?.Report("Đang tải cache bài giảng...");
                        // Chỉ prefetch danh sách lecture + ảnh, không kéo tài nguyên giáo án
                        var lectures = await CourseService.GetByClassCourseAsync(cls.id, course.id);
                        if (lectures == null || lectures.Count == 0)
                            continue;

                        cachedLectureCount += lectures.Count;

                        if (prefetchImages && lectures != null)
                        {
                            foreach (var lecture in lectures)
                            {
                                if (string.IsNullOrWhiteSpace(lecture?.id))
                                    continue;

                                statusProgress?.Report("Đang tải cache hình ảnh bài giảng...");
                                await TryPrefetchLectureImageAsync(lecture);
                            }
                        }
                    }
                }

                if (cachedClassCount <= 0)
                    return (false, "Không cache được lớp nào.");

                if (cachedCourseCount <= 0)
                    return (false, "Không cache được khóa học nào.");

                if (cachedLectureCount <= 0)
                    return (false, "Không cache được bài giảng nào.");

                return (true, "Đã cache xong dữ liệu ban đầu.");
            }
            catch (Exception ex)
            {
                if (IsNetworkException(ex))
                    OfflineState.SetOffline(true);

                return (false, $"Lỗi khi tải cache ban đầu: {ex.Message}");
            }
        }

        private static async Task TryPrefetchClassImageAsync(ClassDto cls)
        {
            var file = GetClassImageFile(cls);
            if (string.IsNullOrWhiteSpace(file))
                return;

            try
            {
                var img = await ClassImageCacheService.GetOrDownloadImageAsync(cls.id, file);
                img?.Dispose();
            }
            catch
            {
                // ignore per-item errors
            }
        }

        private static async Task TryPrefetchCourseImageAsync(CourseDto course, string classId)
        {
            var file = GetCourseImageFile(course);
            if (string.IsNullOrWhiteSpace(file))
                return;

            try
            {
                var img = await CourseImageCacheService.GetOrDownloadImageAsync(course.id, file, classId);
                img?.Dispose();
            }
            catch
            {
                // ignore per-item errors
            }
        }

        private static async Task TryPrefetchLectureImageAsync(LectureDto lecture)
        {
            var file = lecture?.avatar;
            if (string.IsNullOrWhiteSpace(file))
                return;

            try
            {
                var img = await LectureImageCacheService.GetOrDownloadImageAsync(lecture.id, file);
                img?.Dispose();
            }
            catch
            {
                // ignore per-item errors
            }
        }

        private static string GetClassImageFile(ClassDto c)
        {
            if (!string.IsNullOrEmpty(c.currentImage))
                return c.currentImage;

            if (!string.IsNullOrEmpty(c.avatarImage))
                return c.avatarImage;

            if (!string.IsNullOrEmpty(c.avatar))
                return c.avatar;

            return c.imageUrl;
        }

        private static string? GetCourseImageFile(CourseDto c)
        {
            if (!string.IsNullOrWhiteSpace(c.image))
                return c.image;

            if (!string.IsNullOrWhiteSpace(c.thumbnailImage))
                return c.thumbnailImage;

            return null;
        }

        private static bool IsNetworkException(Exception ex)
        {
            if (ex is HttpRequestException || ex is TaskCanceledException || ex is System.Net.Sockets.SocketException)
                return true;

            return ex.InnerException != null && IsNetworkException(ex.InnerException);
        }
    }
}
