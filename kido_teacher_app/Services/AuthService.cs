using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public class AuthService
    {

        //admin
        public static async Task<LoginResponseDto?> LoginAdminAsync(
    string username,
    string password,
    string deviceId
)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(20);

            var body = new { username, password, deviceId };

            var content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/auth/login/admin",
                content
            );

            var raw = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Sai tài khoản hoặc mật khẩu");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Login lỗi: {raw}");

            var result = JsonConvert.DeserializeObject<LoginResponseDto>(raw);

            if (result == null)
                throw new Exception("Không nhận được dữ liệu từ server");

            // lưu token để dùng chung
            AuthSession.AccessToken = result.data?.accessToken
                ?? throw new Exception("Không nhận được token");

            // lưu userId
            AuthSession.UserId = result.data?.userId
                ?? throw new Exception("Không nhận được userId");

            return result;
        }

        // Giáo viên 

        public static async Task<LoginResponseDto?> LoginTeacherAsync(
            string username,
            string password,
            string deviceId
        )
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(20)
            };

            var body = new
            {
                username,
                password,
                deviceId
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/auth/login/teacher",
                content
            );

            var raw = await response.Content.ReadAsStringAsync();

            // ❌ Sai tài khoản / mật khẩu
            if (response.StatusCode == HttpStatusCode.Unauthorized)
                throw new Exception("Sai tài khoản hoặc mật khẩu");

            // ❌ Lỗi khác
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Login giáo viên lỗi: {raw}");

            var result = JsonConvert.DeserializeObject<LoginResponseDto>(raw);

            if (result == null || result.data == null)
                throw new Exception("Không nhận được dữ liệu đăng nhập");

            // ✅ Lưu token dùng toàn app
            AuthSession.AccessToken = result.data.accessToken
                ?? throw new Exception("Không nhận được accessToken");

            // ✅ Lưu userId
            AuthSession.UserId = result.data.userId
                ?? throw new Exception("Không nhận được userId");

            // ✅ (tuỳ chọn) lưu role
            AuthSession.Role = "TEACHER";

            return result;
        }

    }
}
