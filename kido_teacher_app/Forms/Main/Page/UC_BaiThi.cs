using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;
using kido_teacher_app.Services;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_BaiThi : UserControl
    {
        private WebView2 webView;

        public UC_BaiThi()
        {
            InitializeComponent();
            InitWeb();
        }

        private async void InitWeb()
        {
            webView = new WebView2();
            webView.Dock = DockStyle.Fill;
            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async();

            // truyền token
            if (!string.IsNullOrEmpty(AuthSession.AccessToken))
            {
                await webView.CoreWebView2.ExecuteScriptAsync(
                    $"localStorage.setItem('token','{AuthSession.AccessToken}')"
                );
            }

            webView.Source = new Uri("http://160.250.132.143:5173/");
        }
    }
    }