namespace kido_teacher_app.Config
{
    /// <summary>
    /// Centralized API endpoint constants
    /// Usage: var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.CLASSES}";
    /// </summary>
    public static class ApiRoutes
    {
        // =====================================================
        // AUTHENTICATION
        // =====================================================
        public const string AUTH_LOGIN = "/auth/login";
        public const string AUTH_REFRESH = "/auth/refresh";
        public const string AUTH_LOGOUT = "/auth/logout";

        // =====================================================
        // CLASSES
        // =====================================================
        public const string CLASSES = "/classes";
        public static string ClassById(string id) => $"/classes/{id}";

        // =====================================================
        // COURSES
        // =====================================================
        public const string COURSES = "/courses";
        public const string COURSES_MAX_CODE = "/courses/max-code";
        public static string CourseById(string id) => $"/courses/{id}";

        // =====================================================
        // LECTURES
        // =====================================================
        public const string LECTURES = "/lecture";
        public static string LectureById(string id) => $"/lecture/{id}";
        public const string LECTURE_BULK_ASSIGN_USERS = "/lecture/user/bulk";
        public const string LECTURE_BULK_ASSIGN_GROUPS = "/lecture/group/bulk";

        // =====================================================
        // USERS
        // =====================================================
        public const string USERS = "/users";
        public static string UserById(string id) => $"/users/{id}";
        public static string UserGroups(string userId) => $"/users/{userId}/groups";

        // =====================================================
        // GROUPS
        // =====================================================
        public const string GROUPS = "/groups";
        public static string GroupById(string id) => $"/groups/{id}";
        public static string GroupMembers(string groupId) => $"/groups/{groupId}/members";
        public static string GroupRemoveUser(string groupId, string userId) => $"/groups/{groupId}/users/{userId}";

        // =====================================================
        // UPLOAD
        // =====================================================
        public const string UPLOAD_SINGLE = "/upload/single";
        public const string UPLOAD_ZIP = "/upload/zip";
        public static string UploadDownloadImage(string filename) => $"/upload/downloads/{filename}/image";
        public static string UploadDownloadZip(string filename) => $"/upload/downloads/{filename}/zip";

        // =====================================================
        // FILE
        // =====================================================
        public const string FILE_UPLOAD = "/file/upload";
        public static string FileDownload(string filename) => $"/file/download/{filename}";
    }
}

