using kido_teacher_app.Config;
using System.Drawing;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Caching
{
    /// <summary>
    /// Wrapper cho ImageCacheService v?i folder cache Class
    /// </summary>
    public static class ClassImageCacheService
    {
        public static async Task<Image?> GetOrDownloadImageAsync(string classId, string? imageFilename)
        {
            return await ImageCacheService.GetOrDownloadImageAsync(
                classId, 
                imageFilename, 
                AppConfig.ClassImageCacheFolder
            );
        }

        public static void DeleteCache(string classId)
        {
            ImageCacheService.DeleteCache(classId, AppConfig.ClassImageCacheFolder);
        }
    }
}
