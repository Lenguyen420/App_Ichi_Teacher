//using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using System;
using System.Windows.Forms;

namespace kido_teacher_app
{
    public partial class Form_Login : Form
    {
        public Form_Login()
        {
            InitializeComponent();

            this.AcceptButton = loginButton;

            loginButton.Click += loginButton_Click;
            iconEye.Click += iconEye_Click;
        }

        private async void loginButton_Click(object sender, EventArgs e)
        {
            string username = usernameBox.Text.Trim();
            string password = passwordBox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu");
                return;
            }

            try
            {
                loginButton.Enabled = false;

                string deviceId = Environment.MachineName;

                // ✅ 1. LOGIN
                await AuthService.LoginStudentAsync(
                    username,
                    password,
                    deviceId
                );

                // ✅ 2. GỌI API LẤY USER
                var user = await UserService.GetByIdAsync(AuthSession.UserId);

                if (user != null)
                {
                    AuthSession.Username = user.userName;
                    AuthSession.Email = user.email;
                    //AuthSession.Avatar = user.avatar;
                }
                else
                {
                    MessageBox.Show("Không lấy được thông tin user");
                    return;
                }

                // ✅ 3. ĐÓNG FORM
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
                loginButton.Enabled = true;
            }
        }

        private void iconEye_Click(object sender, EventArgs e)
        {
            passwordBox.UseSystemPasswordChar = !passwordBox.UseSystemPasswordChar;
        }
    }
}
