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
                    .FirstOrDefault(),

                VideoPath = Directory
                    .GetFiles(extractPath, "*.mp4", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(),

                ElearningPath = Directory
                    .GetFiles(extractPath, "story.html", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(),

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
    }
}
