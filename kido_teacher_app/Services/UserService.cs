using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Shared.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public class UserService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };

        // =====================================================
        // GET ALL USERS
        // =====================================================
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

        // =====================================================
        // GET USER BY ID
        // =====================================================
        public static async Task<UserDto?> GetByIdAsync(string userId)
        {
            EnsureAuthorized();

            var cacheKey = $"user_{userId}";

            try
            {
                if (OfflineState.IsOffline())
                    return await DbCacheService.GetAsync<UserDto>(cacheKey);

                var response = await client.GetAsync(ApiRoutes.UserById(userId));

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result =
                    JsonConvert.DeserializeObject<ApiResponse<UserDto>>(json);

                if (result?.data != null)
                    await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(result.data));

                return result?.data;
            }
            catch (Exception ex)
            {
                if (IsNetworkException(ex))
                    OfflineState.SetOffline(true);

                return await DbCacheService.GetAsync<UserDto>(cacheKey);
            }
        }


        // =====================================================
        // GET GROUP IDS OF USER
        // =====================================================
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

        // =====================================================
        // COMMON AUTH CHECK
        // =====================================================
        private static void EnsureAuthorized()
        {
            if (string.IsNullOrEmpty(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token không tồn tại");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }

        private static bool IsNetworkException(Exception ex)
        {
            if (ex is HttpRequestException || ex is TaskCanceledException || ex is System.Net.Sockets.SocketException)
                return true;

            return ex.InnerException != null && IsNetworkException(ex.InnerException);
        }


        //  LỌC USER THEO NHÓM 
        public static async Task<List<UserDto>> GetByGroupAsync(string groupId)
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            // ===== ALL =====
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return await GetAllAsync(); // lấy toàn bộ user
            }

            // ===== LỌC THEO NHÓM =====
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

        // tìm kiếm tài khoản theo id, tên, email,..
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
