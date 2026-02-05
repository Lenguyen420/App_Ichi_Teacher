using kido_teacher_app.Config;
using kido_teacher_app.Model;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net.Http.Headers;

namespace kido_teacher_app.Shared.Common
{
    public class GetMaxCodeService
    {
        public static async Task<string> GetMaxCodeAsync(HttpClient client, string route)
        {
            var res = await client.GetAsync($"{AppConfig.ApiBaseUrl}{route}");
            var json = await res.Content.ReadAsStringAsync();
            // ⭐ Parse đúng cấu trúc: { data: { maxCode: 5 } }
            try
            {
                var apiRes = JsonConvert.DeserializeObject<ApiResponse<MaxCodeResponse>>(json);
                var maxCode = apiRes?.data?.maxCode.ToString() ?? "0";
                return maxCode;
            }
            catch (Exception ex)
            {
                
                // Fallback: parse trực tiếp nếu format khác
                dynamic dynRes = JsonConvert.DeserializeObject(json);
                return dynRes?.data?.maxCode?.ToString() ?? "0";
            }
        }
        // ⭐ Helper class for max-code response
        private class MaxCodeResponse
        {
            public int maxCode { get; set; }
        }
    }
}
