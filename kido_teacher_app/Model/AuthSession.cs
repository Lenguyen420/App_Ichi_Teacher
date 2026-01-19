namespace kido_teacher_app.Services
{
    public static class AuthSession
    {
        public static string AccessToken;
        public static string UserId;

        public static string? Role { get; set; } // ADMIN | TEACHER
    }
}
