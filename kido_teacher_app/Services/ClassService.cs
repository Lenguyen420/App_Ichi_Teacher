using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using kido_teacher_app.Model;
using kido_teacher_app.Config;
using System.IO;

namespace kido_teacher_app.Services
{
    public static class ClassService
    {
        private static readonly HttpClient client = new HttpClient();

        // ======================================
        // POST /classes – Tạo lớp học
        // ======================================
        public static async Task<ClassDto?> CreateAsync(ClassCreateDto dto)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PostAsync($"{AppConfig.ApiBaseUrl}{ApiRoutes.CLASSES}", content);

            if (!res.IsSuccessStatusCode)
                return null;

            var resJson = await res.Content.ReadAsStringAsync();
            var apiRes = JsonConvert.DeserializeObject<ApiResponse<ClassDto>>(resJson);

            return apiRes?.data;
        }

        // ======================================
        // GET /classes – Lấy danh sách lớp học
        // ======================================
        public static async Task<List<ClassDto>> GetAllAsync()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}{ApiRoutes.CLASSES}");
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new Exception($"API {ApiRoutes.CLASSES} lỗi {(int)res.StatusCode}: {json}");

            // ⭐ data = ARRAY
            var apiRes =
                JsonConvert.DeserializeObject<ApiResponse<List<ClassDto>>>(json);

            return apiRes?.data ?? new List<ClassDto>();
        }




        // ======================================
        // PATCH /classes/{id} – Cập nhật lớp học
        // ======================================
        public static async Task<bool> PatchAsync(string id, ClassCreateDto dto)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.PatchAsync(
                $"{AppConfig.ApiBaseUrl}{ApiRoutes.ClassById(id)}", content
            );

            return res.IsSuccessStatusCode;
        }


        // lấy chi tiết lớp học để chỉnh sửa 
        public static async Task<ClassDto?> GetByIdAsync(string id)
        {
            client.DefaultRequestHeaders.Authorization = null;
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}{ApiRoutes.ClassById(id)}");

            if (!res.IsSuccessStatusCode)
                return null;

            var json = await res.Content.ReadAsStringAsync();

            // ⭐ data là OBJECT, không phải LIST
            var apiRes = JsonConvert.DeserializeObject<ApiResponse<ClassDto>>(json);
            
            return apiRes?.data;

        }
        // xóa lớp học 
        public static async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var res = await client.DeleteAsync($"{AppConfig.ApiBaseUrl}{ApiRoutes.ClassById(id)}");
            var json = await res.Content.ReadAsStringAsync();

            // Nếu backend trả ApiResponse
            try
            {
                var apiRes = JsonConvert.DeserializeObject<ApiResponse<object>>(json);
                return res.IsSuccessStatusCode && apiRes?.success == true;
            }
            catch
            {
                // fallback: chỉ kiểm tra status
                return res.IsSuccessStatusCode;
            }
        }

    }
}
