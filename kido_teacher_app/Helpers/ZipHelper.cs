using System.IO;
using System.IO.Compression;

namespace kido_teacher_app.Helpers
{
    public static class ZipHelper
    {
        public static string ExtractZip(string zipPath, string extractRoot)
        {
            string folderName = Path.GetFileNameWithoutExtension(zipPath);
            string extractPath = Path.Combine(extractRoot, folderName);

            if (!Directory.Exists(extractPath))
            {
                ZipFile.ExtractToDirectory(zipPath, extractPath);
            }

            return extractPath;
        }
    }
}
