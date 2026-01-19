using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class ApiDownloadService
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<byte[]> DownloadAsync(string url)
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    AuthSession.AccessToken
                );

            return await client.GetByteArrayAsync(url);
        }
    }
}
