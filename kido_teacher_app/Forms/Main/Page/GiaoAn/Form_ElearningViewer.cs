using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using kido_teacher_app.Shared.Logging;
using kido_teacher_app.Config;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_ElearningViewer : Form
    {
        private WebView2 webView = null!;
        private readonly string _urlOrPath;
        private readonly string _title;
        private const string LocalElearningHost = "kido-elearning.local";
        private const int SideBySideConfigurationError = unchecked((int)0x800736B1);
        private bool _initializationStarted;

        public Form_ElearningViewer(string urlOrPath, string title)
        {
            _urlOrPath = urlOrPath;
            _title = title;

            InitUI();
            Shown += Form_ElearningViewer_Shown;
        }

        // ================= UI =================
        private void InitUI()
        {
            this.Text = $"E-Learning - {_title}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(webView);
        }

        // ================= WEBVIEW INIT =================
        private async void Form_ElearningViewer_Shown(object? sender, EventArgs e)
        {
            if (_initializationStarted)
                return;

            _initializationStarted = true;
            await InitWebViewAsync();
        }

        private async System.Threading.Tasks.Task InitWebViewAsync()
        {
            try
            {
                WebViewLog.Info($"E-LEARNING init input='{_urlOrPath}' title='{_title}'");
                // Keep the browser profile writable and stable across ClickOnce updates.
                var userDataFolder = Path.Combine(AppConfig.AppDataRoot, "WebView2");
                var environment = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: userDataFolder);
                WebViewLog.Info($"E-LEARNING runtime='{environment.BrowserVersionString}' processBits='{IntPtr.Size * 8}' userDataFolder='{userDataFolder}'");
                await webView.EnsureCoreWebView2Async(environment);
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                LoadStory();
            }
            catch (Exception ex)
            {
                if (ex is WebView2RuntimeNotFoundException)
                {
                    WebViewLog.Error($"E-LEARNING WebView2 runtime missing input='{_urlOrPath}'");
                    OpenWithDefaultBrowser("Máy chưa cài WebView2 Runtime nên app không thể mở e-learning bên trong ứng dụng. "
                        + "Với Windows 7 SP1, hãy cài WebView2 Runtime 109 từ bộ cài dành cho Win7.");
                    return;
                }

                if (ex.HResult == SideBySideConfigurationError)
                {
                    WebViewLog.Error($"E-LEARNING native runtime side-by-side failure input='{_urlOrPath}' hresult='0x{ex.HResult:X8}'");
                    OpenWithDefaultBrowser(
                        "WebView2 hoặc Microsoft Visual C++ Runtime trên máy đang bị thiếu/hỏng. "
                        + "Hãy Repair hoặc cài lại Microsoft Edge WebView2 Runtime và Microsoft Visual C++ Redistributable (x86).",
                        "WebView2 bị lỗi");
                    return;
                }

                WebViewLog.Error($"E-LEARNING init failed input='{_urlOrPath}' error='{ex}'");
                OpenWithDefaultBrowser(
                    $"Không khởi tạo được WebView2: {ex.Message}",
                    "Không khởi tạo được WebView2");
            }
        }

        // ================= LOAD STORY =================
        private void LoadStory()
        {
            if (string.IsNullOrWhiteSpace(_urlOrPath))
            {
                ShowError("Không có đường dẫn bài giảng");
                return;
            }

            // ===== ONLINE URL =====
            if (_urlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                WebViewLog.Info($"E-LEARNING navigate online url='{_urlOrPath}'");
                webView.CoreWebView2.Navigate(_urlOrPath);
                return;
            }

            // ===== LOCAL FILE =====
            string fullPath = _urlOrPath;

            if (!Path.IsPathRooted(fullPath))
                fullPath = Path.Combine(Application.StartupPath, fullPath);

            WebViewLog.Info($"E-LEARNING resolved input='{_urlOrPath}' fullPath='{fullPath}' exists='{File.Exists(fullPath)}'");

            if (!File.Exists(fullPath))
            {
                WebViewLog.Error($"E-LEARNING file missing fullPath='{fullPath}'");
                ShowError("Không tìm thấy bài học", fullPath);
                return;
            }

            try
            {
                var storyFile = new FileInfo(fullPath);
                var storyFolder = storyFile.DirectoryName;
                if (string.IsNullOrWhiteSpace(storyFolder))
                {
                    WebViewLog.Error($"E-LEARNING invalid story folder fullPath='{fullPath}'");
                    ShowError("Đường dẫn bài học không hợp lệ", fullPath);
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    LocalElearningHost,
                    storyFolder,
                    CoreWebView2HostResourceAccessKind.Allow);

                var localUrl = $"https://{LocalElearningHost}/{EncodePathSegment(storyFile.Name)}";
                WebViewLog.Info($"E-LEARNING navigate localUrl='{localUrl}' folder='{storyFolder}' file='{storyFile.Name}'");
                webView.CoreWebView2.Navigate(localUrl);
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"E-LEARNING open failed fullPath='{fullPath}' error='{ex}'");
                ShowError("Không mở được bài học", ex.Message);
            }
        }

        private static string EncodePathSegment(string value)
        {
            return string.Join("/", value.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Where(part => !string.IsNullOrEmpty(part))
                .Select(WebUtility.UrlEncode));
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                WebViewLog.Info($"E-LEARNING navigation success source='{webView.Source}'");
                return;
            }

            WebViewLog.Error($"E-LEARNING navigation failed source='{webView.Source}' status='{e.WebErrorStatus}' http='{e.HttpStatusCode}'");
            ShowError("WebView2 không tải được bài học", $"{e.WebErrorStatus} ({e.HttpStatusCode})");
        }

        private void OpenWithDefaultBrowser(string reason, string heading = "Thiếu WebView2 Runtime")
        {
            try
            {
                var isWebUrl = Uri.TryCreate(_urlOrPath, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                var target = _urlOrPath;
                if (!isWebUrl && !Path.IsPathRooted(target))
                    target = Path.Combine(Application.StartupPath, target);

                var targetExists = isWebUrl || File.Exists(target);
                WebViewLog.Info($"E-LEARNING fallback external reason='{reason}' target='{target}' exists='{targetExists}'");

                if (targetExists)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });

                    ShowError(
                        heading,
                        $"{reason} Bài học đã được mở bằng trình duyệt mặc định.");
                    return;
                }

                ShowError($"{heading} và không tìm thấy bài học", target);
            }
            catch (Exception fallbackEx)
            {
                WebViewLog.Error($"E-LEARNING fallback external failed input='{_urlOrPath}' error='{fallbackEx}'");
                ShowError(heading, $"{reason} Không mở được bằng trình duyệt mặc định: {fallbackEx.Message}");
            }
        }

        private void ShowError(string message, string detail = "")
        {
            if (webView.CoreWebView2 == null)
            {
                webView.Visible = false;
                Controls.Add(new Label
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    ForeColor = Color.Red,
                    Font = new Font("Segoe UI", 12F),
                    Padding = new Padding(30),
                    Text = $"{message}{Environment.NewLine}{Environment.NewLine}{detail}"
                });
                return;
            }

            webView.NavigateToString($@"
                <div style='
                    font-family:Segoe UI;
                    color:red;
                    font-size:18px;
                    padding:30px'>
                    <b>{WebUtility.HtmlEncode(message)}</b><br/>
                    <small>{WebUtility.HtmlEncode(detail)}</small>
                </div>");
        }
    }
}
