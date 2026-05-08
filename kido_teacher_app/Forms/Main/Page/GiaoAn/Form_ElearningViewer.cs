using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using kido_teacher_app.Shared.Logging;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_ElearningViewer : Form
    {
        private WebView2 webView;
        private readonly string _urlOrPath;
        private readonly string _title;
        private const string LocalElearningHost = "kido-elearning.local";

        public Form_ElearningViewer(string urlOrPath, string title)
        {
            _urlOrPath = urlOrPath;
            _title = title;

            InitUI();
            _ = InitWebViewAsync();
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
        private async System.Threading.Tasks.Task InitWebViewAsync()
        {
            try
            {
                WebViewLog.Info($"E-LEARNING init input='{_urlOrPath}' title='{_title}'");
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                LoadStory();
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"E-LEARNING init failed input='{_urlOrPath}' error='{ex}'");
                ShowError("Không khởi tạo được WebView2", ex.Message);
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

        private void ShowError(string message, string detail = "")
        {
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
