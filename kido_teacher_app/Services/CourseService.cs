using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Shared.Caching;
using kido_teacher_app.Shared.Network;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

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

        public static async Task<List<CourseDto>> GetByClassIdAsync(string classId)
        {
            EnsureAuth();
            var cacheKey = $"courses_class_{classId}";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<CourseDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/courses?classId={classId}");
                if (!res.IsSuccessStatusCode) throw new Exception();

                var json = await res.Content.ReadAsStringAsync();

                var api = JsonConvert.DeserializeObject<
                    ApiResponse<CoursePagedResult>>(json);

                var data = api?.data?.data ?? new();
                var normalized = CacheImagePathNormalizer.NormalizeCoursesForCache(data);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<CourseDto>>(cacheKey);
                return cached ?? new();
            }
        }


        // GET ALL (paged)
        public static async Task<List<CourseDto>> GetAllAsync()
        {
            EnsureAuth();
            var cacheKey = "courses_all";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<CourseDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/courses");
                if (!res.IsSuccessStatusCode) throw new Exception();

                var json = await res.Content.ReadAsStringAsync();

                var api = JsonConvert.DeserializeObject<
                    ApiResponse<CoursePagedResult>>(json);

                var data = api?.data?.data ?? new();
                var normalized = CacheImagePathNormalizer.NormalizeCoursesForCache(data);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<CourseDto>>(cacheKey);
                return cached ?? new();
            }
        }




        // GET BY ID
        public static async Task<CourseDto?> GetByIdAsync(string id)
        {
            EnsureAuth();

            if (OfflineState.IsOffline())
                return null;

            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}/courses/{id}");
            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();

            var api = JsonConvert.DeserializeObject<
                ApiResponse<CourseDto>
            >(json);

            return api?.data;
        }

        // ======================================
        // GET /courses/max-code ï¿½ L?y mï¿½ khï¿½a h?c l?n nh?t
        // ======================================
        public static async Task<string> GetMaxCodeAsync()
        {
            EnsureAuth();
            if (OfflineState.IsOffline())
                return string.Empty;
            // ? Dï¿½ng Service chung
            return await kido_teacher_app.Shared.Common.GetMaxCodeService.GetMaxCodeAsync(client, ApiRoutes.COURSES_MAX_CODE);
        }

        // l?y bï¿½i h?c theo mï¿½ l?p vï¿½ mï¿½ khï¿½a
        public static async Task<List<LectureDto>> GetByClassCourseAsync(
            string classId,
            string courseId)
        {
            var url =
                $"{AppConfig.ApiBaseUrl}/lecture?page=1&size=1000" +
                $"&courseId={courseId}&classId={classId}";
            var cacheKey = $"lectures_class_{classId}_course_{courseId}";

            try
            {
                if (OfflineState.IsOffline())
                {
                    var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                    return cached ?? new();
                }

                var res = await client.GetAsync(url);
                if (!res.IsSuccessStatusCode) throw new Exception();

                var json = await res.Content.ReadAsStringAsync();

                var api = JsonConvert.DeserializeObject<
                    ApiResponse<LecturePagedResult>>(json);

                var data = api?.data?.data ?? new();
                var normalized = CacheImagePathNormalizer.NormalizeLecturesForCache(data);
                await DbCacheService.SaveAsync(cacheKey, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                return cached ?? new();
            }
        }


    }
}


