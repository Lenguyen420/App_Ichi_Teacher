using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class UserGroupService
    {
        private static readonly HttpClient client = new HttpClient();

        // ================= ADD USERS TO GROUP =================
        public static async Task<AddUsersToGroupResponse> AddUsersToGroupAsync(
            string groupId,
            AddUsersToGroupRequest request
        )
        {
            if (string.IsNullOrEmpty(groupId))
                throw new Exception("groupId không hợp lệ");

            if (request == null || request.users == null || request.users.Count == 0)
                throw new Exception("Danh sách user rỗng");
            //throw new Exception("Danh sách user rỗng");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups/group/{groupId}/members",
                content
            );

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception("Thêm user thất bại: " + json);

            return JsonConvert.DeserializeObject<AddUsersToGroupResponse>(json);
        }


        // Lấy chi tiết các nhóm của 1 tài khoản 
        public static async Task<List<UserGroupModel>> GetGroupsByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return new List<UserGroupModel>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.GetAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups/user/{userId}"
            );

            if (!res.IsSuccessStatusCode)
                return new List<UserGroupModel>();

            var json = await res.Content.ReadAsStringAsync();

            // TRƯỚC ĐÂY: ApiResponse<PagedResult<UserGroupModel>>
            var result =
                JsonConvert.DeserializeObject<ApiResponse<List<UserGroupModel>>>(json);

            return result?.data ?? new List<UserGroupModel>();
        }

        // xóa nhóm khỏi 1 user của admin 
        public static async Task<bool> RemoveUserFromGroupAsync(string groupId, string userId)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            string url = $"{AppConfig.ApiBaseUrl}/user-groups/group/{groupId}/member";

            var body = new
            {
                userId = userId
            };

            var json = JsonConvert.SerializeObject(body);

            using (var request = new HttpRequestMessage(HttpMethod.Delete, url))
            {
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await client.SendAsync(request);

                return res.IsSuccessStatusCode;
            }
        }


        //thêm 1 tài khoản vào nhóm . màn tài khoản 
        public static async Task<List<string>> GetGroupIdsOfUserAsync(string userId)
        {
            var res = await client.GetAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups/user/{userId}"
            );

            if (!res.IsSuccessStatusCode)
                return new List<string>();

            var json = await res.Content.ReadAsStringAsync();

            var result =
                JsonConvert.DeserializeObject<ApiResponse<List<UserGroupModel>>>(json);

            return result?.data?
                .Select(x => x.id)
                .ToList()
                ?? new List<string>();
        }


        public static async Task<bool> AddUserToGroupAsync(string userId, string groupId)
        {
            var body = new
            {
                userId,
                groupId
            };

            var content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups",
                content
            );

            return res.IsSuccessStatusCode;
        }

        



    }
}
