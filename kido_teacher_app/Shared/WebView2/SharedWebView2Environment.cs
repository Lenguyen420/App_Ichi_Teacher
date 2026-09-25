using kido_teacher_app.Config;
using Microsoft.Web.WebView2.Core;
using System.IO;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.WebView2
{
    public static class SharedWebView2Environment
    {
        private static readonly object LockObj = new object();
        private static Task<CoreWebView2Environment>? _environmentTask;

        public static Task<CoreWebView2Environment> GetAsync()
        {
            lock (LockObj)
            {
                if (_environmentTask == null || _environmentTask.IsCanceled || _environmentTask.IsFaulted)
                {
                    var userDataFolder = Path.Combine(AppConfig.AppDataRoot, "WebView2");
                    _environmentTask = CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                }

                return _environmentTask;
            }
        }
    }
}
