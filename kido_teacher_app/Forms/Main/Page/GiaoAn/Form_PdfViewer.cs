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
    public class Form_PdfViewer : Form
    {
        private readonly WebView2 webView;
        private readonly string _pdfUrlOrPath;
        private const string LocalPdfHost = "kido-pdf.local";

        public Form_PdfViewer(string pdfUrl, string lessonTitle)
        {
            _pdfUrlOrPath = pdfUrl;

            this.Text = $"Giáo án PDF - {lessonTitle}";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            this.Controls.Add(webView);
            this.Load += async (s, e) => await InitWebViewAsync();
        }

        private async System.Threading.Tasks.Task InitWebViewAsync()
        {
            try
            {
                WebViewLog.Info($"PDF init input='{_pdfUrlOrPath}'");
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                LoadPdf();
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"PDF init failed input='{_pdfUrlOrPath}' error='{ex}'");
                ShowError("Không khởi tạo được WebView2", ex.Message);
            }
        }

        private void LoadPdf()
        {
            if (string.IsNullOrWhiteSpace(_pdfUrlOrPath))
            {
                ShowError("Không có đường dẫn PDF");
                return;
            }

            if (_pdfUrlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                WebViewLog.Info($"PDF navigate online url='{_pdfUrlOrPath}'");
                webView.CoreWebView2.Navigate(_pdfUrlOrPath);
                return;
            }

            var fullPath = _pdfUrlOrPath;
            if (!Path.IsPathRooted(fullPath))
                fullPath = Path.Combine(Application.StartupPath, fullPath);

            WebViewLog.Info($"PDF resolved input='{_pdfUrlOrPath}' fullPath='{fullPath}' exists='{File.Exists(fullPath)}'");

            if (!File.Exists(fullPath))
            {
                WebViewLog.Error($"PDF file missing fullPath='{fullPath}'");
                ShowError("Không tìm thấy file PDF", fullPath);
                return;
            }

            try
            {
                var pdfFile = new FileInfo(fullPath);
                var pdfFolder = pdfFile.DirectoryName;
                if (string.IsNullOrWhiteSpace(pdfFolder))
                {
                    WebViewLog.Error($"PDF invalid folder fullPath='{fullPath}'");
                    ShowError("Đường dẫn PDF không hợp lệ", fullPath);
                    return;
                }

                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    LocalPdfHost,
                    pdfFolder,
                    CoreWebView2HostResourceAccessKind.Allow);

                var localUrl = $"https://{LocalPdfHost}/{EncodePathSegment(pdfFile.Name)}";
                WebViewLog.Info($"PDF navigate localUrl='{localUrl}' folder='{pdfFolder}' file='{pdfFile.Name}'");
                webView.CoreWebView2.Navigate(localUrl);
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"PDF open failed fullPath='{fullPath}' error='{ex}'");
                ShowError("Không mở được PDF", ex.Message);
            }
        }

        private void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                WebViewLog.Info($"PDF navigation success source='{webView.Source}'");
                return;
            }

            WebViewLog.Error($"PDF navigation failed source='{webView.Source}' status='{e.WebErrorStatus}' http='{e.HttpStatusCode}'");
            ShowError("WebView2 không tải được PDF", $"{e.WebErrorStatus} ({e.HttpStatusCode})");
        }

        private static string EncodePathSegment(string value)
        {
            return string.Join("/", value.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Where(part => !string.IsNullOrEmpty(part))
                .Select(WebUtility.UrlEncode));
        }

        private void ShowError(string message, string detail = "")
        {
            webView.NavigateToString($@"
                <div style='
                    font-family:Segoe UI;
                    color:#b00020;
                    font-size:18px;
                    padding:30px'>
                    <b>{WebUtility.HtmlEncode(message)}</b><br/>
                    <small>{WebUtility.HtmlEncode(detail)}</small>
                </div>");
        }
    }
}
