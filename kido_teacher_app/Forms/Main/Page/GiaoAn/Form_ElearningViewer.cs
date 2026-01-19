using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_ElearningViewer : Form
    {
        private WebView2 webView;
        private readonly string _urlOrPath;
        private readonly string _title;

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
            await webView.EnsureCoreWebView2Async();
            LoadStory();
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
                webView.Source = new Uri(_urlOrPath);
                return;
            }

            // ===== LOCAL FILE =====
            string fullPath = _urlOrPath;

            if (!Path.IsPathRooted(fullPath))
                fullPath = Path.Combine(Application.StartupPath, fullPath);

            if (!File.Exists(fullPath))
            {
                ShowError("Không tìm thấy bài học", fullPath);
                return;
            }

            webView.Source = new Uri(fullPath);
        }

        private void ShowError(string message, string detail = "")
        {
            webView.NavigateToString($@"
                <div style='
                    font-family:Segoe UI;
                    color:red;
                    font-size:18px;
                    padding:30px'>
                    <b>{message}</b><br/>
                    <small>{detail}</small>
                </div>");
        }
    }
}
