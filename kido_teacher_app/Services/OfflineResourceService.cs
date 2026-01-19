using System.IO;

namespace kido_teacher_app.Services
{
    public static class OfflineResourceService
    {
        public static string GetLectureFolder(string lectureId)
        {
            string root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KIDO",
                "OfflineLessons",
                lectureId
            );

            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            return root;
        }

        public static string GetOfflineFilePath(string lectureId, string fileName)
        {
            return Path.Combine(GetLectureFolder(lectureId), fileName);
        }

        public static bool IsDownloaded(string lectureId, string fileName)
        {
            return File.Exists(GetOfflineFilePath(lectureId, fileName));
        }
    }
}
