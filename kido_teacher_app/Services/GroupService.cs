using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Services
{
    public static class GroupService
    {
        private static readonly HttpClient client = new HttpClient();

        // ================= GET ALL GROUPS =================
        // API: GET /groups
        public static async Task<List<GroupDto>> GetAllAsync()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/groups?page=1&limit=1000");
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"API /groups lỗi {(int)res.StatusCode}: {json}");

            var result =
                JsonConvert.DeserializeObject<ApiResponse<List<GroupDto>>>(json);

            return result?.data ?? new List<GroupDto>();
        }

        // ================= CREATE GROUP =================
        // API: POST /groups
        public static async Task<GroupDto?> CreateAsync(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
                throw new Exception("Tên nhóm không hợp lệ");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var body = new { name = groupName };

            var content = new StringContent(
                JsonConvert.SerializeObject(body),
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/groups",
                content
            );

            res.EnsureSuccessStatusCode();

            var json = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GroupDto>(json);
        }

        // ================= GET MEMBERS BY GROUP =================
        // API: GET /user-groups/group/{groupId}/members
        public static async Task<List<GroupMemberModel>> GetMembersAsync(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                return new List<GroupMemberModel>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.GetAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups/group/{groupId}/members"
            );

            if (!res.IsSuccessStatusCode)
                return new List<GroupMemberModel>();

            var json = await res.Content.ReadAsStringAsync();

            //  BACKEND TRẢ VỀ LIST TRỰC TIẾP
            var result = JsonConvert.DeserializeObject<ApiResponse<List<GroupMemberModel>>>(json);

            return result?.data ?? new List<GroupMemberModel>();
        }



        // ================= ADD USERS TO GROUP =================
        // API: POST /user-groups/group/{groupId}/members
        // ================= ADD USERS TO GROUP =================
        // API: POST /user-groups/group/{groupId}/members
        public static async Task AddUsersToGroupAsync(
    string groupId,
    AddUsersToGroupRequest request
)
        {
            if (request == null || request.users.Count == 0)
                throw new Exception("Danh sách user rỗng");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var json = JsonConvert.SerializeObject(request);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync(
                $"{AppConfig.ApiBaseUrl}/user-groups/group/{groupId}/members",
                content
            );

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                throw new Exception("Thêm user thất bại: " + err);
            }
        }




        // ================= REMOVE USER FROM GROUP =================
        // API: DELETE /user-groups/group/{groupId}/members
        public static async Task<bool> RemoveMemberAsync(string groupId, string userId)
        {
            if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(userId))
                return false;

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var body = new
            {
                userIds = new List<string> { userId }
            };

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                $"{AppConfig.ApiBaseUrl}/user-groups/group/{groupId}/members"
            )
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    Encoding.UTF8,
                    "application/json"
                )
            };

            var res = await client.SendAsync(request);

            if (res.IsSuccessStatusCode)
                return true;

            var error = await res.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        // ================= DELETE GROUP =================
        // API: DELETE /groups/{groupId}
        // ================= DELETE GROUP =================
        // API: DELETE /groups/{groupId}
        public static async Task<bool> DeleteGroupAsync(string groupId)
        {
            if (string.IsNullOrEmpty(groupId))
                throw new Exception("GroupId không hợp lệ");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.DeleteAsync(
                $"{AppConfig.ApiBaseUrl}/groups/{groupId}"
            );

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception("Xóa nhóm thất bại: " + json);

            // Backend trả về: { success: true, message, data }
            var result = JsonConvert.DeserializeObject<ApiResponse<object>>(json);

            return result != null && result.success;
        }

        // ================= UPDATE GROUP =================
        // API: PUT /groups/{groupId}
        public static async Task<GroupDto> UpdateGroupAsync(
            string groupId,
            UpdateGroupRequest request
        )
        {
            if (string.IsNullOrEmpty(groupId))
                throw new Exception("GroupId không hợp lệ");

            if (request == null || string.IsNullOrWhiteSpace(request.name))
                throw new Exception("Tên nhóm không hợp lệ");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var content = new StringContent(
                JsonConvert.SerializeObject(request),
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PutAsync(
                $"{AppConfig.ApiBaseUrl}/groups/{groupId}",
                content
            );

            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception("Cập nhật nhóm thất bại: " + json);

            var result =
                JsonConvert.DeserializeObject<ApiResponse<GroupDto>>(json);

            return result?.data;
        }

        // Tìm kiếm theo tên hoặc mã nhóm 
        public static async Task<List<GroupDto>> SearchGroupsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<GroupDto>();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var url =
                $"{AppConfig.ApiBaseUrl}/groups/search?keyword={Uri.EscapeDataString(keyword)}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
                return new List<GroupDto>();

            var json = await res.Content.ReadAsStringAsync();

            //  TRƯỚC ĐÂY: ApiResponse<List<GroupDto>>
            var response =
                JsonConvert.DeserializeObject<ApiResponse<PagedResult<GroupDto>>>(json);

            return response?.data?.items ?? new List<GroupDto>();
        }

    }
}
