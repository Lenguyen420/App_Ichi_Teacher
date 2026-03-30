using kido_teacher_app.Models;
using System.IO;
using System.Linq;

namespace kido_teacher_app.Services
{
    public class LectureResourceService
    {
        public LectureFiles MapLectureFiles(string extractPath)
        {
            if (string.IsNullOrWhiteSpace(extractPath) || !Directory.Exists(extractPath))
            {
                return new LectureFiles();
            }

            return new LectureFiles
            {
                PdfPath = FindBestMatch(extractPath, "*.pdf"),
                VideoPath = FindBestMatch(extractPath, "*.mp4"),
                ElearningPath = FindBestMatch(extractPath, "story.html")
            };
        }

        private static string? FindBestMatch(string rootPath, string pattern)
        {
            return Directory
                .GetFiles(rootPath, pattern, SearchOption.AllDirectories)
                .OrderBy(path => path.Count(ch =>
                    ch == Path.DirectorySeparatorChar ||
                    ch == Path.AltDirectorySeparatorChar))
                .ThenBy(path => path.Length)
                .FirstOrDefault();
        }
    }
}
