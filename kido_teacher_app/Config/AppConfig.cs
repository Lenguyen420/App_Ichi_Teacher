using System.Configuration;
using System.IO;

namespace kido_teacher_app.Config
{
    //public static class AppConfig
    //{
    //    public static string ApiBaseUrl =>
    //        ConfigurationManager.AppSettings["ApiBaseUrl"];

    //    // Nơi ZIP được tải về
    //    public static string DownloadFolder =
    //        @"D:\App_Ichi\Downloads";

    //    // Nơi giải nén bài giảng
    //    public static string LectureExtractFolder =
    //        @"D:\App_Ichi\Resources\Lectures";
    //}

    public static class AppConfig
    {
        public static string ApiBaseUrl =>
            ConfigurationManager.AppSettings["ApiBaseUrl"];

        public static string VersionJsonUrl =>
            ConfigurationManager.AppSettings["VersionJsonUrl"]
            ?? $"{ApiBaseUrl}/uploads/ichiteacher/version.json";

        public static string UpdateVersionApiUrl =>
            ConfigurationManager.AppSettings["UpdateVersionApiUrl"]
            ?? $"{ApiBaseUrl}/updateversion";

        public static string UpdateVersionApiToken =>
            ConfigurationManager.AppSettings["UpdateVersionApiToken"];

        // ROOT APPDATA (LocalApplicationData)
        public static string AppDataRoot =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidoTeacherApp"
            );

        // ROOT APPDATA (Roaming - cho config)
        public static string AppDataRoaming =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KidoTeacherApp"
            );

        // ZIP tải về (nếu cần)
        public static string DownloadFolder =
            Path.Combine(AppDataRoot, "Downloads");

        // Nơi giải nén bài giảng (OFFLINE THẬT)
        public static string LectureExtractFolder =
            Path.Combine(AppDataRoot, "Lectures");

        // ⭐ Thư mục Cache - chứa resource-map.json
        public static string CacheFolder =
            Path.Combine(AppDataRoot, "Cache");

        // ⭐ Cache ảnh Course (Roaming)
        public static string CourseImageCacheFolder =
            Path.Combine(AppDataRoaming, "Course");

        // ⭐ Cache ảnh Lecture (Roaming)
        public static string LectureImageCacheFolder =
            Path.Combine(AppDataRoaming, "Lecture");

        // ⭐ Cache ảnh Class (Roaming)
        public static string ClassImageCacheFolder =
            Path.Combine(AppDataRoaming, "Class");
    }

}
