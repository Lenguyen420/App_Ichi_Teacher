using kido_teacher_app.Config;
using System;
using System.IO;
using System.Text;

namespace kido_teacher_app.Shared.Logging
{
    public static class FileLog
    {
        private static readonly object _lock = new object();
        private static string LogPath =>
            Path.Combine(AppConfig.CacheFolder, "app.log");

        public static void Info(string message)
        {
            Write("INFO", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static void Write(string level, string message)
        {
            try
            {
                Directory.CreateDirectory(AppConfig.CacheFolder);
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                lock (_lock)
                {
                    File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
                }
            }
            catch
            {
                // swallow
            }
        }
    }
}
