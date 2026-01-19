namespace kido_teacher_app.Model
{
    public class LoginResponseDto
    {
        public bool success { get; set; }
        public string message { get; set; }
        public LoginData data { get; set; }
    }

    public class LoginData
    {
        public string accessToken { get; set; }
        public string userId { get; set; }
        public string userType { get; set; }
        public string deviceId { get; set; }
    }
}
