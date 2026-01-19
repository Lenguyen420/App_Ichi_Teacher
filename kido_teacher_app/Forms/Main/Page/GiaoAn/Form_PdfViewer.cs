using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;
using System;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_PdfViewer : Form
    {
        private WebView2 webView;

        public Form_PdfViewer(string pdfUrl, string lessonTitle)
        {
            // 🔹 TIÊU ĐỀ FORM = TÊN BÀI HỌC
            this.Text = $"Giáo án PDF – {lessonTitle}";
            this.WindowState = FormWindowState.Maximized;

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(webView);

            this.Load += async (s, e) =>
            {
                await webView.EnsureCoreWebView2Async();
                webView.Source = new Uri(pdfUrl);
            };
        }
    }
}
