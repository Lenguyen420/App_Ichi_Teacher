using kido_teacher_app.Config;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Windows.Forms;

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

            Controls.Add(webView);

            await webView.EnsureCoreWebView2Async();

            var safeToken = AuthSession.AccessToken?.Replace("'", "\\'");
            var safeUsername = AuthSession.Username?.Replace("'", "\\'");
            var safeUserId = AuthSession.UserId?.Replace("'", "\\'");

            string script = $@"
                localStorage.setItem('accessToken', '{safeToken}');
                localStorage.setItem('username', '{safeUsername}');
                localStorage.setItem('userId', '{safeUserId}');
            ";

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);

            webView.Source = new Uri("https://fe.kidostudent.kidoedu.vn");
        }
    }
}
