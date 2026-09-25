using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using kido_teacher_app.Shared.Logging;
using kido_teacher_app.Shared.Web;
using kido_teacher_app.Shared.WebView2;
using kido_teacher_app.Config;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_ElearningViewer : Form
    {
        private WebView2? webView;
        private string _urlOrPath = string.Empty;
        private string _title = string.Empty;
        private const int SideBySideConfigurationError = unchecked((int)0x800736B1);
        private const int WebViewInitializationTimeoutMilliseconds = 12000;
        private static Form_ElearningViewer? _activeViewer;
        private bool _fallbackStarted;
        private bool _controllerFailed;
        private bool _allowClose;
        private int _lessonVersion;
        private Task<bool>? _initializationTask;
        private string _initializationFailureReason = string.Empty;
        private Label? _statusLabel;
        private Panel _contentPanel = null!;
        private CheckBox _alwaysUseBrowserCheckBox = null!;
        private readonly Stopwatch _openStopwatch = Stopwatch.StartNew();

        public static void WarmUp()
        {
            if (WindowsVersionHelper.IsWindows7 || ElearningPreferences.AlwaysOpenInDefaultBrowser)
                return;

            var viewer = GetOrCreateViewer();
            viewer.CreateControl();
            viewer.webView?.CreateControl();
            viewer.StartWarmUp();
        }

        public static void ShowLesson(string urlOrPath, string title)
        {
            var viewer = GetOrCreateViewer();
            if (viewer.Visible
                && string.Equals(viewer._urlOrPath, urlOrPath, StringComparison.OrdinalIgnoreCase))
            {
                if (viewer.WindowState == FormWindowState.Minimized)
                    viewer.WindowState = FormWindowState.Maximized;

                viewer.BringToFront();
                viewer.Activate();
                WebViewLog.Info($"E-LEARNING duplicate open prevented input='{urlOrPath}'");
                return;
            }

            viewer.Show();
            viewer.BringToFront();
            viewer.Activate();
            viewer.OpenLesson(urlOrPath, title);
        }

        public static void Shutdown()
        {
            var viewer = _activeViewer;
            _activeViewer = null;
            if (viewer == null || viewer.IsDisposed)
                return;

            viewer._allowClose = true;
            viewer.Close();
            viewer.Dispose();
        }

        private static Form_ElearningViewer GetOrCreateViewer()
        {
            if (_activeViewer == null || _activeViewer.IsDisposed)
                _activeViewer = new Form_ElearningViewer();

            return _activeViewer;
        }

        private Form_ElearningViewer()
        {
            InitUI();
            FormClosing += Form_ElearningViewer_FormClosing;
        }

        // ================= UI =================
        private void InitUI()
        {
            this.Text = $"E-Learning - {_title}";
            this.BackColor = Color.White;

            if (WindowsVersionHelper.IsWindows7)
            {
                WindowState = FormWindowState.Normal;
                StartPosition = FormStartPosition.CenterScreen;
                Size = new Size(680, 260);
            }
            else
            {
                WindowState = FormWindowState.Maximized;
                webView = new WebView2
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.White,
                    Visible = false
                };
            }

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

            _contentPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            if (webView != null)
                _contentPanel.Controls.Add(webView);
            _contentPanel.Controls.Add(_statusLabel);

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
                Text = WindowsVersionHelper.IsWindows7
                    ? "Windows 7 luôn mở e-learning bằng trình duyệt"
                    : "Luôn mở e-learning bằng trình duyệt",
                Checked = WindowsVersionHelper.IsWindows7 || ElearningPreferences.AlwaysOpenInDefaultBrowser,
                Enabled = !WindowsVersionHelper.IsWindows7,
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

            Controls.Add(_contentPanel);
            Controls.Add(toolbar);
            toolbar.BringToFront();
        }

        // ================= WEBVIEW INIT =================
        private async void StartWarmUp()
        {
            var ready = await EnsureWebViewReadyAsync();
            WebViewLog.Info($"E-LEARNING warmup completed ready='{ready}'");
        }

        private async void OpenLesson(string urlOrPath, string title)
        {
            _lessonVersion++;
            var lessonVersion = _lessonVersion;
            _urlOrPath = urlOrPath;
            _title = title;
            _fallbackStarted = false;
            _openStopwatch.Restart();
            Text = $"E-Learning - {_title}";
            ShowStatus("Đang mở bài giảng...", _title, Color.DimGray);

            if (WindowsVersionHelper.IsWindows7)
            {
                OpenWithDefaultBrowser(
                    "Windows 7 sử dụng trình duyệt mặc định thay cho WebView2.",
                    "Đã mở bài học bằng trình duyệt mặc định",
                    false);
                return;
            }

            if (ElearningPreferences.AlwaysOpenInDefaultBrowser)
            {
                OpenWithDefaultBrowser(
                    "Tùy chọn luôn mở bằng trình duyệt đang được bật.",
                    "Đã mở bài học bằng trình duyệt mặc định",
                    false);
                return;
            }

            if (_controllerFailed)
                RecreateWebViewControl();

            var initializationTask = EnsureWebViewReadyAsync();
            var completedTask = await Task.WhenAny(
                initializationTask,
                Task.Delay(WebViewInitializationTimeoutMilliseconds));

            if (lessonVersion != _lessonVersion || IsDisposed || Disposing)
                return;

            if (completedTask != initializationTask)
            {
                WebViewLog.Error($"E-LEARNING initialization timeout elapsedMs='{_openStopwatch.ElapsedMilliseconds}' input='{_urlOrPath}'");
                OpenWithDefaultBrowser(
                    "WebView2 khởi tạo quá 12 giây.",
                    "Trình xem trong ứng dụng phản hồi chậm",
                    false);
                return;
            }

            if (!await initializationTask)
            {
                OpenWithDefaultBrowser(
                    string.IsNullOrWhiteSpace(_initializationFailureReason)
                        ? "Không khởi tạo được WebView2."
                        : _initializationFailureReason,
                    "Không khởi tạo được WebView2");
                return;
            }

            if (lessonVersion == _lessonVersion && !_fallbackStarted)
                LoadStory();
        }

        private Task<bool> EnsureWebViewReadyAsync()
        {
            if (_initializationTask == null)
                _initializationTask = InitWebViewAsync();

            return _initializationTask;
        }

        private async Task<bool> InitWebViewAsync()
        {
            _initializationFailureReason = string.Empty;

            try
            {
                if (webView == null)
                {
                    _initializationFailureReason = "WebView2 không được khởi tạo trên hệ điều hành này.";
                    return false;
                }

                WebViewLog.Info($"E-LEARNING init input='{_urlOrPath}' title='{_title}'");
                var environmentTimer = Stopwatch.StartNew();
                var userDataFolder = Path.Combine(AppConfig.AppDataRoot, "WebView2");
                var environment = await SharedWebView2Environment.GetAsync();
                if (IsDisposed || Disposing) return false;
                WebViewLog.Info($"E-LEARNING environment ready elapsedMs='{environmentTimer.ElapsedMilliseconds}' runtime='{environment.BrowserVersionString}' processBits='{IntPtr.Size * 8}' userDataFolder='{userDataFolder}'");

                var controllerTimer = Stopwatch.StartNew();
                await webView.EnsureCoreWebView2Async(environment);
                if (IsDisposed || Disposing) return false;
                WebViewLog.Info($"E-LEARNING controller ready elapsedMs='{controllerTimer.ElapsedMilliseconds}' totalMs='{_openStopwatch.ElapsedMilliseconds}'");
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                webView.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;
                _controllerFailed = false;
                return true;
            }
            catch (Exception ex)
            {
                if (ex is WebView2RuntimeNotFoundException)
                {
                    WebViewLog.Error($"E-LEARNING WebView2 runtime missing input='{_urlOrPath}'");
                    _initializationFailureReason = "Máy chưa cài WebView2 Runtime nên app không thể mở e-learning bên trong ứng dụng. "
                        + "Với Windows 7 SP1, hãy cài WebView2 Runtime 109 từ bộ cài dành cho Win7.";
                    return false;
                }

                if (ex.HResult == SideBySideConfigurationError)
                {
                    WebViewLog.Error($"E-LEARNING native runtime side-by-side failure input='{_urlOrPath}' hresult='0x{ex.HResult:X8}'");
                    _initializationFailureReason = "WebView2 hoặc Microsoft Visual C++ Runtime trên máy đang bị thiếu/hỏng. "
                        + "Hãy Repair hoặc cài lại Microsoft Edge WebView2 Runtime và Microsoft Visual C++ Redistributable (x86).";
                    return false;
                }

                WebViewLog.Error($"E-LEARNING init failed input='{_urlOrPath}' error='{ex}'");
                _initializationFailureReason = $"Không khởi tạo được WebView2: {ex.Message}";
                return false;
            }
        }

        private void RecreateWebViewControl()
        {
            if (webView != null)
            {
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
                    webView.CoreWebView2.ProcessFailed -= CoreWebView2_ProcessFailed;
                }
                _contentPanel.Controls.Remove(webView);
                webView.Dispose();
            }

            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Visible = false
            };
            _contentPanel.Controls.Add(webView);
            webView.SendToBack();
            _initializationTask = null;
            _controllerFailed = false;
            WebViewLog.Info("E-LEARNING controller recreated after process failure");
        }

        // ================= LOAD STORY =================
        private void LoadStory()
        {
            if (_fallbackStarted)
                return;

            if (webView?.CoreWebView2 == null)
            {
                ShowError("Trình xem WebView2 chưa sẵn sàng");
                return;
            }

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
                if (!LocalElearningHttpServer.TryCreateLessonUrl(
                    fullPath,
                    out var localUrl,
                    out var serverError))
                {
                    WebViewLog.Error($"E-LEARNING local server unavailable fullPath='{fullPath}' error='{serverError}'");
                    ShowError("Không khởi động được máy chủ bài giảng nội bộ", serverError);
                    return;
                }

                WebViewLog.Info($"E-LEARNING navigate local server url='{localUrl}' fullPath='{fullPath}'");
                webView.CoreWebView2.Navigate(localUrl);
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"E-LEARNING open failed fullPath='{fullPath}' error='{ex}'");
                OpenWithDefaultBrowser(ex.Message, "Không mở được bài học bằng WebView2");
            }
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_fallbackStarted || IsDisposed || Disposing) return;

            if (e.IsSuccess)
            {
                WebViewLog.Info($"E-LEARNING navigation success elapsedMs='{_openStopwatch.ElapsedMilliseconds}' source='{webView?.Source}'");
                if (webView != null)
                {
                    webView.Visible = true;
                    webView.BringToFront();
                }
                if (_statusLabel != null)
                    _statusLabel.Visible = false;
                return;
            }

            WebViewLog.Error($"E-LEARNING navigation failed source='{webView?.Source}' status='{e.WebErrorStatus}' http='{e.HttpStatusCode}'");
            // A replaced/cancelled navigation is not a failure to open the lesson.
            if (e.WebErrorStatus == CoreWebView2WebErrorStatus.OperationCanceled) return;
            OpenWithDefaultBrowser($"{e.WebErrorStatus} ({e.HttpStatusCode})", "WebView2 không tải được bài học");
        }

        private void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            WebViewLog.Error($"E-LEARNING process failed kind='{e.ProcessFailedKind}' elapsedMs='{_openStopwatch.ElapsedMilliseconds}' source='{webView?.Source}'");
            if (e.ProcessFailedKind != CoreWebView2ProcessFailedKind.BrowserProcessExited
                && e.ProcessFailedKind != CoreWebView2ProcessFailedKind.RenderProcessExited
                && e.ProcessFailedKind != CoreWebView2ProcessFailedKind.RenderProcessUnresponsive)
                return;

            _controllerFailed = true;
            OpenWithDefaultBrowser(e.ProcessFailedKind.ToString(), "WebView2 đã ngừng hoạt động");
        }

        private void Form_ElearningViewer_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (_allowClose || e.CloseReason != CloseReason.UserClosing)
                return;

            e.Cancel = true;
            _lessonVersion++;
            _fallbackStarted = true;
            try
            {
                webView?.CoreWebView2?.Navigate("about:blank");
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"E-LEARNING stop lesson before hide failed error='{ex}'");
            }
            Hide();
            WebViewLog.Info("E-LEARNING viewer hidden; controller retained for reuse");
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
                    var browserTarget = target;
                    if (!isWebUrl
                        && (string.Equals(Path.GetExtension(target), ".html", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(Path.GetExtension(target), ".htm", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (!LocalElearningHttpServer.TryCreateLessonUrl(target, out browserTarget, out var serverError))
                        {
                            ShowError(
                                "Không khởi động được máy chủ bài giảng nội bộ",
                                serverError);
                            return;
                        }
                    }

                    ShowStatus(
                        heading,
                        WindowsVersionHelper.IsWindows7
                            ? "Đang khởi động trình duyệt để mở bài qua địa chỉ nội bộ 127.0.0.1..."
                            : "Đang khởi động trình duyệt mặc định...",
                        isError ? Color.Red : Color.DarkGreen);
                    Update();

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = browserTarget,
                        UseShellExecute = true
                    });

                    ShowStatus(
                        heading,
                        WindowsVersionHelper.IsWindows7
                            ? "Bài học đang được phục vụ qua địa chỉ nội bộ 127.0.0.1 để không hiển thị đường dẫn file trên máy."
                            : "Đã gửi yêu cầu mở bài học bằng trình duyệt mặc định. Bỏ chọn tùy chọn phía trên nếu lần sau muốn mở trong ứng dụng.",
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
            if (webView != null)
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
