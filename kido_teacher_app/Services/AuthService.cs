using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public class AuthService
    {
        private const string TokenCheckPath = "/auth/token/check";
        private static string TokenFilePath =>
            Path.Combine(AppConfig.AppDataRoaming, "token.txt");

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

        public static void SaveRememberToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return;
            }

            Directory.CreateDirectory(AppConfig.AppDataRoaming);
            File.WriteAllText(TokenFilePath, token);
        }

        public static void ClearRememberToken()
        {
            try
            {
                if (File.Exists(TokenFilePath))
                {
                    File.Delete(TokenFilePath);
                }
            }
            catch
            {
                // ignore
            }
        }

        public static async Task<bool> TryLoginWithSavedTokenAsync()
        {
            var token = LoadRememberToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            var alive = await CheckTokenAliveAsync(token);
            if (!alive)
            {
                ClearRememberToken();
                return false;
            }

            AuthSession.AccessToken = token;
            SetSessionFromToken(token);
            return true;
        }

        private static string? LoadRememberToken()
        {
            try
            {
                if (!File.Exists(TokenFilePath))
                {
                    return null;
                }

                return File.ReadAllText(TokenFilePath).Trim();
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> CheckTokenAliveAsync(string token)
        {
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(20)
                };

                var url = $"{AppConfig.ApiBaseUrl}{TokenCheckPath}";
                var body = new { token };
                var content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync(url, content);
                var raw = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                return TryReadAlive(raw, out var alive) && alive;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadAlive(string raw, out bool alive)
        {
            alive = false;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            try
            {
                var root = JObject.Parse(raw);
                var data = root["data"] ?? root;
                var aliveToken = data["alive"];
                if (aliveToken == null)
                {
                    return false;
                }

                if (aliveToken.Type == JTokenType.Boolean)
                {
                    alive = aliveToken.Value<bool>();
                    return true;
                }

                if (aliveToken.Type == JTokenType.String &&
                    bool.TryParse(aliveToken.Value<string>(), out var parsed))
                {
                    alive = parsed;
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static void SetSessionFromToken(string token)
        {
            try
            {
                var userId = GetJwtClaim(token, "userId") ?? GetJwtClaim(token, "sub");
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    AuthSession.UserId = userId;
                }

                var userType = GetJwtClaim(token, "userType") ?? GetJwtClaim(token, "role");
                if (!string.IsNullOrWhiteSpace(userType))
                {
                    AuthSession.Role = userType;
                }
            }
            catch
            {
                // ignore
            }
        }

        private static string? GetJwtClaim(string token, string claim)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }

            var parts = token.Split('.');
            if (parts.Length < 2)
            {
                return null;
            }

            try
            {
                var json = Base64UrlDecode(parts[1]);
                var obj = JObject.Parse(json);
                return obj[claim]?.Value<string>();
            }
            catch
            {
                return null;
            }
        }

        private static string Base64UrlDecode(string input)
        {
            var s = input.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2:
                    s += "==";
                    break;
                case 3:
                    s += "=";
                    break;
            }

            var bytes = Convert.FromBase64String(s);
            return Encoding.UTF8.GetString(bytes);
        }

    }
}
