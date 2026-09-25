using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using kido_teacher_app.Shared.Logging;
using kido_teacher_app.Shared.WebView2;
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
        private static Form_ElearningViewer? _activeViewer;
        private bool _initializationStarted;
        private bool _fallbackStarted;
        private Label? _statusLabel;
        private CheckBox _alwaysUseBrowserCheckBox = null!;
        private readonly Stopwatch _openStopwatch = Stopwatch.StartNew();

        public static void ShowLesson(string urlOrPath, string title)
        {
            if (_activeViewer != null && !_activeViewer.IsDisposed)
            {
                if (string.Equals(_activeViewer._urlOrPath, urlOrPath, StringComparison.OrdinalIgnoreCase))
                {
                    if (_activeViewer.WindowState == FormWindowState.Minimized)
                        _activeViewer.WindowState = FormWindowState.Maximized;

                    _activeViewer.Show();
                    _activeViewer.BringToFront();
                    _activeViewer.Activate();
                    WebViewLog.Info($"E-LEARNING duplicate open prevented input='{urlOrPath}'");
                    return;
                }

                _activeViewer.Close();
            }

            var viewer = new Form_ElearningViewer(urlOrPath, title);
            _activeViewer = viewer;
            viewer.FormClosed += (sender, args) =>
            {
                if (ReferenceEquals(_activeViewer, viewer))
                    _activeViewer = null;
            };
            viewer.Show();
        }

        private Form_ElearningViewer(string urlOrPath, string title)
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
                BackColor = Color.White,
                Visible = false
            };

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 12F),
                Padding = new Padding(30),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Đang khởi tạo trình xem và tải bài giảng..."
            };

            var contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            contentPanel.Controls.Add(webView);
            contentPanel.Controls.Add(_statusLabel);

            var openInBrowserButton = new Button
            {
                AutoSize = true,
                Text = "Mở bằng trình duyệt",
                Margin = new Padding(8, 5, 8, 5)
            };
            openInBrowserButton.Click += (sender, args) => OpenWithDefaultBrowser(
                "Người dùng chọn mở bằng trình duyệt mặc định.",
                "Đang mở bằng trình duyệt mặc định",
                false);

            _alwaysUseBrowserCheckBox = new CheckBox
            {
                AutoSize = true,
                Text = "Luôn mở e-learning bằng trình duyệt",
                Checked = ElearningPreferences.AlwaysOpenInDefaultBrowser,
                Margin = new Padding(8, 9, 8, 5)
            };
            _alwaysUseBrowserCheckBox.CheckedChanged += (sender, args) =>
            {
                ElearningPreferences.AlwaysOpenInDefaultBrowser = _alwaysUseBrowserCheckBox.Checked;
                WebViewLog.Info($"E-LEARNING external browser preference='{_alwaysUseBrowserCheckBox.Checked}'");
            };

            var toolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = false,
                BackColor = Color.WhiteSmoke,
                Padding = new Padding(8, 4, 8, 4)
            };
            toolbar.Controls.Add(openInBrowserButton);
            toolbar.Controls.Add(_alwaysUseBrowserCheckBox);

            Controls.Add(contentPanel);
            Controls.Add(toolbar);
            toolbar.BringToFront();
        }

        // ================= WEBVIEW INIT =================
        private async void Form_ElearningViewer_Shown(object? sender, EventArgs e)
        {
            if (_initializationStarted)
                return;

            _initializationStarted = true;

            if (ElearningPreferences.AlwaysOpenInDefaultBrowser)
            {
                OpenWithDefaultBrowser(
                    "Tùy chọn luôn mở bằng trình duyệt đang được bật.",
                    "Đã mở bài học bằng trình duyệt mặc định",
                    false);
                return;
            }

            await InitWebViewAsync();
        }

        private async System.Threading.Tasks.Task InitWebViewAsync()
        {
            try
            {
                WebViewLog.Info($"E-LEARNING init input='{_urlOrPath}' title='{_title}'");
                var environmentTimer = Stopwatch.StartNew();
                var userDataFolder = Path.Combine(AppConfig.AppDataRoot, "WebView2");
                var environment = await SharedWebView2Environment.GetAsync();
                if (IsDisposed || Disposing) return;
                WebViewLog.Info($"E-LEARNING environment ready elapsedMs='{environmentTimer.ElapsedMilliseconds}' runtime='{environment.BrowserVersionString}' processBits='{IntPtr.Size * 8}' userDataFolder='{userDataFolder}'");

                var controllerTimer = Stopwatch.StartNew();
                await webView.EnsureCoreWebView2Async(environment);
                if (IsDisposed || Disposing) return;
                WebViewLog.Info($"E-LEARNING controller ready elapsedMs='{controllerTimer.ElapsedMilliseconds}' totalMs='{_openStopwatch.ElapsedMilliseconds}'");
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webView.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
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
                OpenWithDefaultBrowser(ex.Message, "Không mở được bài học bằng WebView2");
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
            if (_fallbackStarted || IsDisposed || Disposing) return;

            if (e.IsSuccess)
            {
                WebViewLog.Info($"E-LEARNING navigation success elapsedMs='{_openStopwatch.ElapsedMilliseconds}' source='{webView.Source}'");
                webView.Visible = true;
                webView.BringToFront();
                if (_statusLabel != null)
                    _statusLabel.Visible = false;
                return;
            }

            WebViewLog.Error($"E-LEARNING navigation failed source='{webView.Source}' status='{e.WebErrorStatus}' http='{e.HttpStatusCode}'");
            // A replaced/cancelled navigation is not a failure to open the lesson.
            if (e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) return;
            OpenWithDefaultBrowser($"{e.WebErrorStatus} ({e.HttpStatusCode})", "WebView2 không tải được bài học");
        }

        private void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            WebViewLog.Error($"E-LEARNING process failed kind='{e.ProcessFailedKind}' elapsedMs='{_openStopwatch.ElapsedMilliseconds}' source='{webView.Source}'");
            if (e.ProcessFailedKind != CoreWebView2ProcessFailedKind.BrowserProcessExited
                && e.ProcessFailedKind != CoreWebView2ProcessFailedKind.RenderProcessExited
                && e.ProcessFailedKind != CoreWebView2ProcessFailedKind.RenderProcessUnresponsive)
                return;

            OpenWithDefaultBrowser(e.ProcessFailedKind.ToString(), "WebView2 đã ngừng hoạt động");
        }

        private void OpenWithDefaultBrowser(
            string reason,
            string heading = "Không mở được WebView2",
            bool isError = true)
        {
            if (_fallbackStarted || IsDisposed || Disposing) return;
            _fallbackStarted = true;

            try
            {
                var isWebUrl = Uri.TryCreate(_urlOrPath, UriKind.Absolute, out var uri)
                    && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                var target = _urlOrPath;
                if (string.IsNullOrWhiteSpace(target))
                {
                    ShowError("Không có đường dẫn bài giảng");
                    return;
                }
                if (!isWebUrl && uri != null && uri.IsFile)
                    target = uri.LocalPath;
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

                    ShowStatus(
                        heading,
                        "Đã gửi yêu cầu mở bài học bằng trình duyệt mặc định. Bỏ chọn tùy chọn phía trên nếu lần sau muốn mở trong ứng dụng.",
                        isError ? Color.Red : Color.DarkGreen);
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
            ShowStatus(message, detail, Color.Red);
        }

        private void ShowStatus(string message, string detail, Color color)
        {
            if (IsDisposed || Disposing) return;

            // Render status with WinForms even when the WebView process has failed.
            webView.Visible = false;
            if (_statusLabel == null)
            {
                _statusLabel = new Label
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    ForeColor = color,
                    Font = new Font("Segoe UI", 12F),
                    Padding = new Padding(30),
                };
                Controls.Add(_statusLabel);
            }
            _statusLabel.ForeColor = color;
            _statusLabel.Visible = true;
            _statusLabel.Text = $"{message}{Environment.NewLine}{Environment.NewLine}{detail}";
            _statusLabel.BringToFront();
        }
    }
}
