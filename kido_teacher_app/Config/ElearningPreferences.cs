using System;
using System.IO;

namespace kido_teacher_app.Config
{
    public static class ElearningPreferences
    {
        private static readonly object LockObj = new object();

        private static string PreferencePath =>
            Path.Combine(AppConfig.AppDataRoaming, "elearning-open-external.pref");

        public static bool AlwaysOpenInDefaultBrowser
        {
            get
            {
                lock (LockObj)
                {
                    try
                    {
                        return File.Exists(PreferencePath)
                            && string.Equals(File.ReadAllText(PreferencePath).Trim(), "1", StringComparison.Ordinal);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            set
            {
                lock (LockObj)
                {
                    try
                    {
                        Directory.CreateDirectory(AppConfig.AppDataRoaming);
                        File.WriteAllText(PreferencePath, value ? "1" : "0");
                    }
                    catch
                    {
                        // A preference write failure must not prevent opening a lesson.
                    }
                }
            }
        }
    }
}
