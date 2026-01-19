using kido_teacher_app.Config;
using System.Drawing;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Wrapper cho ImageCacheService v?i folder cache Lecture
    /// </summary>
    public static class LectureImageCacheService
    {
        public static async Task<Image?> GetOrDownloadImageAsync(string lectureId, string? imageFilename)
        {
            return await ImageCacheService.GetOrDownloadImageAsync(
                lectureId, 
                imageFilename, 
                AppConfig.LectureImageCacheFolder
            );
        }

        public static void DeleteCache(string lectureId)
        {
            ImageCacheService.DeleteCache(lectureId, AppConfig.LectureImageCacheFolder);
        }
    }
}
