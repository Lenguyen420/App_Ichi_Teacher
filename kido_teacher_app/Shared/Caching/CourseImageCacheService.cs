using kido_teacher_app.Config;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Wrapper cho ImageCacheService v?i folder cache Course/{classId}
    /// </summary>
    public static class CourseImageCacheService
    {
        public static async Task<Image?> GetOrDownloadImageAsync(
            string courseId, 
            string? imageFilename, 
            string classId)
        {
            // ? T?o subfolder theo classId
            var cacheFolder = Path.Combine(AppConfig.CourseImageCacheFolder, classId);

            return await ImageCacheService.GetOrDownloadImageAsync(
                courseId, 
                imageFilename, 
                cacheFolder
            );
        }

        public static void DeleteCache(string courseId, string classId)
        {
            var cacheFolder = Path.Combine(AppConfig.CourseImageCacheFolder, classId);
            ImageCacheService.DeleteCache(courseId, cacheFolder);
        }

        /// <summary>
        /// X�a to�n b? cache c?a m?t class
        /// </summary>
        public static void DeleteClassCache(string classId)
        {
            try
            {
                var classFolder = Path.Combine(AppConfig.CourseImageCacheFolder, classId);
                if (Directory.Exists(classFolder))
                {
                    Directory.Delete(classFolder, true);
                }
            }
            catch
            {
                // Fail silently
            }
        }
    }
}
