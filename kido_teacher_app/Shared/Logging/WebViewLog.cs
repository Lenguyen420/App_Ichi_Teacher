using kido_teacher_app.Config;
using System;
using System.IO;

namespace kido_teacher_app.Shared.Logging
{
    public static class WebViewLog
    {
        private static readonly object LockObj = new object();

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
                var logPath = Path.Combine(AppConfig.CacheFolder, "log_webview.txt");
                var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

                lock (LockObj)
                {
                    File.AppendAllText(logPath, line);
                }
            }
            catch
            {
                // Logging must never block opening teaching resources.
            }
        }
    }
}
