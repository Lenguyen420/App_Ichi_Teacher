using kido_teacher_app.Shared.Logging;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.GiaoAn
{
    public class Form_PdfViewer : Form
    {
        private readonly string _pdfUrlOrPath;

        public Form_PdfViewer(string pdfUrl, string lessonTitle)
        {
            _pdfUrlOrPath = pdfUrl;

            Text = $"Giáo án PDF - {lessonTitle}";
            WindowState = FormWindowState.Maximized;
            BackColor = Color.White;

            Load += (s, e) => OpenPdfWithDefaultApp();
        }

        private void OpenPdfWithDefaultApp()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_pdfUrlOrPath))
                {
                    WebViewLog.Error("PDF default open failed: empty path");
                    ShowStatus("Không có đường dẫn PDF.");
                    return;
                }

                var target = _pdfUrlOrPath;
                if (!target.StartsWith("http", StringComparison.OrdinalIgnoreCase) && !Path.IsPathRooted(target))
                    target = Path.Combine(Application.StartupPath, target);

                var exists = target.StartsWith("http", StringComparison.OrdinalIgnoreCase) || File.Exists(target);
                WebViewLog.Info($"PDF default open input='{_pdfUrlOrPath}' target='{target}' exists='{exists}'");

                if (!exists)
                {
                    WebViewLog.Error($"PDF default open file missing target='{target}'");
                    ShowStatus($"Không tìm thấy file PDF:{Environment.NewLine}{target}");
                    return;
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });

                ShowStatus("PDF đã được mở bằng ứng dụng mặc định.");
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"PDF default open failed input='{_pdfUrlOrPath}' error='{ex}'");
                ShowStatus($"Không mở được PDF bằng ứng dụng mặc định:{Environment.NewLine}{ex.Message}");
            }
        }

        private void ShowStatus(string message)
        {
            Controls.Clear();
            Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(30),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(64, 64, 64),
                Text = message
            });
        }
    }
}
