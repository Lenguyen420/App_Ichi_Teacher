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
            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };

            this.Controls.Add(webView);

            await webView.EnsureCoreWebView2Async();

            var safeToken = AuthSession.AccessToken?.Replace("'", "\\'");
            var safeUsername = AuthSession.Username?.Replace("'", "\\'");

            string script = $@"
        localStorage.setItem('accessToken', '{safeToken}');
        localStorage.setItem('username', '{safeUsername}');
    ";

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

            webView.Source = new Uri("https://fe.kidostudent.kidoedu.vn");
        }
    }
    }