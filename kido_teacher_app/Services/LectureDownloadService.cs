using System;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class LectureDownloadService
    {
        // =====================================================
        // DOWNLOAD & EXTRACT (WITH LECTURE ID)
        // =====================================================
        public static async Task<string?> DownloadExtractAndLogAsync(
            string zipFilename,
            string lectureId,
            IProgress<int>? progress = null)
        {
            // Tải và giải nén
            var extractPath = await LectureService.DownloadAndExtractZipAsync(zipFilename, lectureId, progress);

            if (string.IsNullOrEmpty(extractPath))
                return null;

            return extractPath;
        }
    }
}
