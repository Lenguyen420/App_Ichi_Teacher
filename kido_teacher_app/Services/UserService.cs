using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public class UserService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };

        public static async Task<List<UserDto>> GetAllAsync()
        {
            EnsureAuthorized();

            var response = await client.GetAsync($"{ApiRoutes.USERS}?page=1&limit=1000");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserDto>>>(json);

            return result?.data?.items ?? new List<UserDto>();
        }

        public static async Task<UserDto?> GetByIdAsync(string userId)
        {
            EnsureAuthorized();

            var response = await client.GetAsync(ApiRoutes.UserById(userId));

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<UserDto>>(json);

            return result?.data;
        }

        public static async Task<bool> CreateUserAsync(CreateUserRequest request)
        {
            EnsureAuthorized();

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(ApiRoutes.USERS, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {responseText}");

            return true;
        }

        public static async Task<bool> UpdateUserAsync(
            string userId,
            UpdateUserRequest request
        )
        {
            EnsureAuthorized();

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(ApiRoutes.UserById(userId), content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {responseText}");

            return true;
        }

        public static async Task<bool> UpdateUserGroupsAsync(
            string userId,
            List<string> groupIds
        )
        {
            EnsureAuthorized();

            var body = new
            {
                groupIds = groupIds
            };

            var json = JsonConvert.SerializeObject(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync(ApiRoutes.UserById(userId), content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)response.StatusCode}: {responseText}");

            return true;
        }

        public static async Task<List<string>> GetUserGroupIdsAsync(string userId)
        {
            EnsureAuthorized();

            var response = await client.GetAsync(ApiRoutes.UserById(userId));

            if (!response.IsSuccessStatusCode)
                return new List<string>();

            var json = await response.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<UserDto>>(json);

            return result?.data?.groupIds ?? new List<string>();
        }

        public static async Task<bool> DeleteAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId không hợp lệ");

            EnsureAuthorized();

            var response = await client.DeleteAsync(ApiRoutes.UserById(userId));
            return response.IsSuccessStatusCode;
        }

        private static void EnsureAuthorized()
        {
            if (string.IsNullOrEmpty(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token không tồn tại");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }


        public static async Task<List<UserDto>> GetByGroupAsync(string groupId)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            if (string.IsNullOrWhiteSpace(groupId))
            {
                return await GetAllAsync();
            }

            var url =
                $"{AppConfig.ApiBaseUrl}{ApiRoutes.USERS}?page=1&limit=1000&groupId={Uri.EscapeDataString(groupId)}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception($"API lỗi ({(int)res.StatusCode}): {err}");
            }

            var json = await res.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserDto>>>(json);

            return result?.data?.items ?? new List<UserDto>();
        }

        public static async Task<List<UserDto>> SearchAsync(
            string keyword,
            string? groupId = null
        )
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var query = new List<string>
            {
                "page=1",
                "limit=1000"
            };

            if (!string.IsNullOrWhiteSpace(keyword))
                query.Add($"search={Uri.EscapeDataString(keyword)}");

            if (!string.IsNullOrWhiteSpace(groupId))
                query.Add($"groupId={Uri.EscapeDataString(groupId)}");

            var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.USERS}?{string.Join("&", query)}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception($"API lỗi ({(int)res.StatusCode}): {err}");
            }

            var json = await res.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<PagedResult<UserDto>>>(json);

            return result?.data?.items ?? new List<UserDto>();
        }

    }
}
