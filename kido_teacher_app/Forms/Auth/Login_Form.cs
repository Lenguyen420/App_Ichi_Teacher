//using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app
{
    public partial class Form_Login : Form
    {
        private bool _applyingResponsiveLayout;

        public Form_Login()
        {
            InitializeComponent();

            this.AcceptButton = loginButton;

            loginButton.Click += loginButton_Click;
            iconEye.Click += iconEye_Click;
            Shown += (_, _) => ApplyResponsiveLayout(fitToWorkingArea: true);
            Resize += (_, _) => ApplyResponsiveLayout();
            DpiChanged += (_, _) => BeginInvoke(new Action(() => ApplyResponsiveLayout(fitToWorkingArea: true)));
        }

        private void ApplyResponsiveLayout(bool fitToWorkingArea = false)
        {
            if (_applyingResponsiveLayout || !IsHandleCreated)
                return;

            _applyingResponsiveLayout = true;

            try
            {
                if (fitToWorkingArea)
                    FitToWorkingArea();

                SuspendLayout();
                leftPanel.SuspendLayout();
                rightPanel.SuspendLayout();

                int leftWidth = Clamp((int)Math.Round(ClientSize.Width * 0.32), 320, 450);
                leftWidth = Math.Min(leftWidth, Math.Max(280, ClientSize.Width / 2));
                leftPanel.Width = leftWidth;

                int leftPadding = Clamp(leftWidth / 12, 24, 40);
                int logoSize = Clamp(leftWidth - (leftPadding * 2), 180, 300);
                int logoTop = Clamp(ClientSize.Height / 25, 24, 48);
                logoIchi.Size = new Size(logoSize, logoSize);
                logoIchi.Location = new Point((leftPanel.ClientSize.Width - logoSize) / 2, logoTop);

                int companyTop = logoIchi.Bottom + Clamp(ClientSize.Height / 28, 20, 36);
                companyLabel.Location = new Point(0, companyTop);
                companyLabel.Size = new Size(leftPanel.ClientSize.Width, 150);

                copyRightLabel.Width = leftPanel.ClientSize.Width;
                copyRightLabel.Location = new Point(
                    0,
                    leftPanel.ClientSize.Height - copyRightLabel.Height - 10
                );

                int contentWidth = Clamp(rightPanel.ClientSize.Width - 220, 300, 460);
                int iconSize = 40;
                int rowWidth = iconSize + 10 + contentWidth + 50;
                int rowX = Math.Max(30, (rightPanel.ClientSize.Width - rowWidth) / 2);
                int titleTop = Clamp(rightPanel.ClientSize.Height / 18, 30, 48);

                titleLabel.Location = new Point(
                    Math.Max(20, (rightPanel.ClientSize.Width - titleLabel.Width) / 2),
                    titleTop
                );

                int userRowY = titleLabel.Bottom + Clamp(rightPanel.ClientSize.Height / 12, 40, 80);
                int passwordRowY = userRowY + Clamp(rightPanel.ClientSize.Height / 12, 60, 78);

                usernameBox.Width = contentWidth;
                passwordBox.Width = contentWidth;

                iconUser.Location = new Point(rowX, userRowY);
                usernameBox.Location = new Point(
                    iconUser.Right + 10,
                    userRowY + Math.Max(0, (iconUser.Height - usernameBox.Height) / 2)
                );

                iconPass.Location = new Point(rowX, passwordRowY);
                passwordBox.Location = new Point(
                    iconPass.Right + 10,
                    passwordRowY + Math.Max(0, (iconPass.Height - passwordBox.Height) / 2)
                );
                iconEye.Location = new Point(
                    passwordBox.Right + 10,
                    passwordRowY
                );

                rememberCheck.Location = new Point(
                    usernameBox.Left,
                    passwordBox.Bottom + Clamp(ClientSize.Height / 40, 18, 28)
                );

                loginButton.Location = new Point(
                    Math.Max(20, (rightPanel.ClientSize.Width - loginButton.Width) / 2),
                    rememberCheck.Bottom + Clamp(ClientSize.Height / 28, 22, 34)
                );

                statusLabel.Location = new Point(
                    Math.Max(20, (rightPanel.ClientSize.Width - statusLabel.Width) / 2),
                    loginButton.Bottom + Clamp(ClientSize.Height / 36, 18, 28)
                );

                int maxInfoWidth = Clamp(rightPanel.ClientSize.Width - 120, 360, 820);
                infoLabel.AutoSize = false;
                infoLabel.MaximumSize = Size.Empty;
                infoLabel.Size = MeasureWrappedText(infoLabel.Text, infoLabel.Font, maxInfoWidth);
                infoLabel.Location = new Point(
                    Math.Max(20, (rightPanel.ClientSize.Width - infoLabel.Width) / 2),
                    statusLabel.Bottom + Clamp(ClientSize.Height / 30, 20, 32)
                );

                int socialTop = infoLabel.Bottom + Clamp(ClientSize.Height / 30, 18, 28);
                int socialGap = 15;
                int socialLeft = Math.Max(
                    20,
                    (rightPanel.ClientSize.Width - (iconFB.Width + socialGap + iconWWW.Width)) / 2
                );
                iconFB.Location = new Point(socialLeft, socialTop);
                iconWWW.Location = new Point(iconFB.Right + socialGap, socialTop);
            }
            finally
            {
                rightPanel.ResumeLayout(true);
                leftPanel.ResumeLayout(true);
                ResumeLayout(true);
                _applyingResponsiveLayout = false;
            }
        }

        private void FitToWorkingArea()
        {
            var area = Screen.FromControl(this).WorkingArea;
            int margin = 40;
            int targetWidth = Math.Min(Width, Math.Max(640, area.Width - margin));
            int targetHeight = Math.Min(Height, Math.Max(520, area.Height - margin));

            if (Width != targetWidth || Height != targetHeight)
            {
                Size = new Size(targetWidth, targetHeight);
            }

            Location = new Point(
                area.Left + Math.Max(0, (area.Width - Width) / 2),
                area.Top + Math.Max(0, (area.Height - Height) / 2)
            );
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static Size MeasureWrappedText(string text, Font font, int maxWidth)
        {
            return TextRenderer.MeasureText(
                text,
                font,
                new Size(maxWidth, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
            );
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Text.Trim();
            var oldText = loginButton.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu");
                return;
            }

            try
            {
                loginButton.Enabled = false;
 
                string deviceId = Environment.MachineName;

                await AuthService.LoginTeacherAsync(
                    username,
                    password,
                    deviceId
                );

                // AuthService đã set:
                // AuthSession.AccessToken
                // AuthSession.UserId
                // AuthSession.Role = TEACHER
                statusLabel.Text = "Đang chuẩn bị cache...";
                statusLabel.Visible = true;

                var progress = new Progress<string>(message =>
                {
                    statusLabel.Text = message;
                });

                var prefetched = await OfflinePrefetchService.PrefetchTeacherOfflineAsync(
                    prefetchImages: true,
                    statusProgress: progress
                );

                if (!prefetched)
                {
                    AuthService.ClearRememberToken();
                    MessageBox.Show(
                        "Không tải được cache ban đầu (danh sách và hình ảnh). Vui lòng kiểm tra mạng rồi đăng nhập lại.",
                        "Đăng nhập thất bại",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                    return;
                }

                statusLabel.Text = "Đang lưu phiên đăng nhập...";
                if (rememberCheck.Checked)
                {
                    AuthService.SaveRememberToken(AuthSession.AccessToken);
                }
                else
                {
                    AuthService.ClearRememberToken();
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Đăng nhập thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                loginButton.Text = oldText;
                loginButton.Enabled = true;
            }
        }


        private void iconEye_Click(object sender, EventArgs e)
        {
            passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
        }
    }
}
