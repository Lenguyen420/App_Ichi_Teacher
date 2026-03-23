using kido_teacher_app.Models;
using System.IO;

namespace kido_teacher_app.Services
{
    public class LectureResourceService
    {
        public LectureFiles MapLectureFiles(string extractPath)
        {
            return new LectureFiles
            {
                PdfPath = Directory
                    .GetFiles(extractPath, "*.pdf", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault()
                    ?? FindWrappedPdfPath(extractPath),

                VideoPath = Directory
                    .GetFiles(extractPath, "*.mp4", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(),

                ElearningPath = Directory
                    .GetFiles(extractPath, "story.html", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault()
                    ?? FindWrappedElearningPath(extractPath),

                PowerPointPath = Directory
                    .GetFiles(extractPath, "*.pptx", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault()
                    ?? Directory
                        .GetFiles(extractPath, "*.ppsx", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault()
                    ?? Directory
                        .GetFiles(extractPath, "*.ppt", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault()
                    ?? Directory
                        .GetFiles(extractPath, "*.pps", SearchOption.TopDirectoryOnly)
                        .FirstOrDefault()
            };
        }

        private static string? FindWrappedPdfPath(string extractPath)
        {
            foreach (var folderPath in Directory.GetDirectories(extractPath))
            {
                var folderName = Path.GetFileName(folderPath);
                if (IsReservedElearningFolder(folderName))
                    continue;

                if (Directory.GetDirectories(folderPath).Length > 0)
                    continue;

                var pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
                if (pdfFiles.Length == 1)
                    return pdfFiles[0];
            }

            return null;
        }

        private static string? FindWrappedElearningPath(string extractPath)
        {
            foreach (var folderPath in Directory.GetDirectories(extractPath))
            {
                var folderName = Path.GetFileName(folderPath);
                if (IsReservedElearningFolder(folderName))
                    continue;

                var storyPath = Path.Combine(folderPath, "story.html");
                if (File.Exists(storyPath))
                    return storyPath;
            }

            return null;
        }

        private static bool IsReservedElearningFolder(string folderName)
        {
            return
                string.Equals(folderName, "html5", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(folderName, "mobile", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(folderName, "story_content", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
