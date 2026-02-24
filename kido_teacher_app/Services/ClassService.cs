using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using kido_teacher_app.Model;
using kido_teacher_app.Config;
using kido_teacher_app.Shared.Caching;
using kido_teacher_app.Shared.Logging;
using kido_teacher_app.Shared.Network;

namespace kido_teacher_app.Services
{
    public static class ClassService
    {
        private static readonly HttpClient client = new HttpClient();
        private const string CacheKeyAll = "classes_all";

        // ======================================
        // GET /classes – Lấy danh sách lớp học
        // ======================================
        public static async Task<List<ClassDto>> GetAllAsync()
        {
            if (OfflineState.IsOffline())
            {
                var cached = await DbCacheService.GetAsync<List<ClassDto>>(CacheKeyAll);
                return cached ?? new List<ClassDto>();
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);

            try
            {
                var url = $"{AppConfig.ApiBaseUrl}{ApiRoutes.CLASSES}";
                FileLog.Info($"[ClassService] GET {url}");

                var res = await client.GetAsync(url);
                var json = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    FileLog.Error($"[ClassService] HTTP {(int)res.StatusCode}: {json}");
                    throw new Exception($"API {ApiRoutes.CLASSES} lỗi {(int)res.StatusCode}: {json}");
                }

                FileLog.Info($"[ClassService] HTTP {(int)res.StatusCode} bodyLength={json?.Length ?? 0}");
                FileLog.Info($"[ClassService] Response body: {json}");

                var data = ExtractClasses(json);

                var normalized = CacheImagePathNormalizer.NormalizeClassesForCache(data);
                await DbCacheService.SaveAsync(CacheKeyAll, JsonConvert.SerializeObject(normalized));

                return data;
            }
            catch
            {
                var cached = await DbCacheService.GetAsync<List<ClassDto>>(CacheKeyAll);
                return cached ?? new List<ClassDto>();
            }
        }

        private static List<ClassDto> ExtractClasses(string json)
        {
            // Supports: { data: [ ... ] } OR { data: { data: [ ... ] } } OR { data: { items: [ ... ] } }
            try
            {
                var root = JObject.Parse(json);
                var dataToken = root["data"];
                if (dataToken == null || dataToken.Type == JTokenType.Null)
                    return new List<ClassDto>();

                if (dataToken.Type == JTokenType.Array)
                    return dataToken.ToObject<List<ClassDto>>() ?? new List<ClassDto>();

                if (dataToken.Type == JTokenType.Object)
                {
                    var nested = dataToken["data"] ?? dataToken["items"];
                    if (nested != null && nested.Type == JTokenType.Array)
                        return nested.ToObject<List<ClassDto>>() ?? new List<ClassDto>();
                }
            }
            catch
            {
            }

            // Fallback: original shape
            try
            {
                var apiRes =
                    JsonConvert.DeserializeObject<ApiResponse<List<ClassDto>>>(json);
                return apiRes?.data ?? new List<ClassDto>();
            }
            catch
            {
                return new List<ClassDto>();
            }
        }

        // lấy chi tiết lớp học để chỉnh sửa 
        public static async Task<ClassDto?> GetByIdAsync(string id)
        {
            if (OfflineState.IsOffline())
                return null;

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
    }
}
