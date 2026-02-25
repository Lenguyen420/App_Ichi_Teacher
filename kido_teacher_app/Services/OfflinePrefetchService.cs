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
        public static async Task PrefetchTeacherOfflineAsync(bool prefetchImages = true)
        {
            if (OfflineState.IsOffline())
                return;

            try
            {
                var classes = await ClassService.GetAllAsync();
                foreach (var cls in classes)
                {
                    if (string.IsNullOrWhiteSpace(cls?.id))
                        continue;

                    if (prefetchImages)
                        await TryPrefetchClassImageAsync(cls);

                    var courses = await CourseService.GetByClassIdAsync(cls.id);
                    foreach (var course in courses)
                    {
                        if (string.IsNullOrWhiteSpace(course?.id))
                            continue;

                        if (prefetchImages)
                            await TryPrefetchCourseImageAsync(course, cls.id);

                        // Lectures + resources are cached by GetByClassCourseAsync
                        var lectures = await LectureService.GetByClassCourseAsync(cls.id, course.id);

                        if (prefetchImages && lectures != null)
                        {
                            foreach (var lecture in lectures)
                            {
                                if (string.IsNullOrWhiteSpace(lecture?.id))
                                    continue;

                                await TryPrefetchLectureImageAsync(lecture);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (IsNetworkException(ex))
                    OfflineState.SetOffline(true);
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
