using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace kido_teacher_app.Services
{
    public static class CourseService
    {
        private static readonly HttpClient client = new HttpClient();

        private static void EnsureAuth()
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }

        // CREATE
        public static async Task<CourseDto?> CreateAsync(CourseCreateDto dto)
        {
            EnsureAuth();

            var content = new StringContent(
                JsonConvert.SerializeObject(dto),
                Encoding.UTF8,
                "application/json"
            );

            var res = await client.PostAsync($"{AppConfig.ApiBaseUrl}/courses", content);
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<CourseDto>
            >(json);

            return api?.data;
        }


        // GET ALL (paged)
        public static async Task<List<CourseDto>> GetAllAsync()
        {
            EnsureAuth();

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/courses");
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<CoursePagedResult>>(json);

            return api?.data?.data ?? new();
        }




        // GET BY ID
        public static async Task<CourseDto?> GetByIdAsync(string id)
        {
            EnsureAuth();

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/courses/{id}");
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<CourseDto>
            >(json);

            return api?.data;
        }

        public static async Task<bool> UpdateAsync(string id, CourseCreateDto dto)
        {
            using var req = new HttpRequestMessage(
                HttpMethod.Patch,
                $"{AppConfig.ApiBaseUrl}/courses/{id}"
            );

            req.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            var json = JsonConvert.SerializeObject(dto);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await client.SendAsync(req);

            // ❌ API TRẢ LỖI
            if (!res.IsSuccessStatusCode)
            {
                var errBody = await res.Content.ReadAsStringAsync();

                throw new Exception(
                    $"HTTP {(int)res.StatusCode} ({res.StatusCode})\n{errBody}"
                );
            }

            return true;
        }

        // DELETE
        public static async Task<bool> DeleteAsync(string id)
        {
            EnsureAuth();

            var res = await client.DeleteAsync($"{AppConfig.ApiBaseUrl}/courses/{id}");
            return res.IsSuccessStatusCode;
        }


        // lấy bài học theo mã lớp và mã khóa
        public static async Task<List<LectureDto>> GetByClassCourseAsync(
    string classId,
    string courseId)
        {
            var url =
                $"{AppConfig.ApiBaseUrl}/lecture?page=1&size=1000" +
                $"&courseId={courseId}&classId={classId}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new();

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<LecturePagedResult>>(json);

            return api?.data?.data ?? new();
        }


    }
}
