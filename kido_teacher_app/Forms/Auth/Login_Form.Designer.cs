namespace kido_teacher_app
{
    partial class Form_Login
    {
        private System.ComponentModel.IContainer components = null;

        private Panel leftPanel, rightPanel;
        private RoundPictureBox logoIchi;
        private PictureBox iconUser, iconPass, iconEye, iconFB, iconWWW;
        private Label companyLabel, copyRightLabel, titleLabel, statusLabel, infoLabel;
        private TextBox usernameBox, passwordBox;
        private CheckBox rememberCheck;
        private Button loginButton;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            
            // LEFT PANEL
            
            leftPanel = new Panel();
            logoIchi = new RoundPictureBox();
            companyLabel = new Label();
            copyRightLabel = new Label();

            leftPanel.BackColor = Color.FromArgb(241, 199, 3);
            leftPanel.Dock = DockStyle.Left;
            leftPanel.Size = new Size(450, 750);

            // Logo hình tròn nền trắng
            logoIchi.Location = new Point(75, 30);
            logoIchi.Size = new Size(300, 300);
            logoIchi.SizeMode = PictureBoxSizeMode.Zoom;
            logoIchi.CircleBackColor = Color.White;
           // logoIchi.InnerText = "KIDO";
            //logoIchi.TextColor = Color.Blue;
            logoIchi.Image = Properties.Resources.logo4;

            // Công ty
            companyLabel.Font = new Font("Arial", 16F, FontStyle.Bold);
            companyLabel.ForeColor = Color.FromArgb(153, 0, 0);
            companyLabel.Location = new Point(0, 350);
            companyLabel.Size = new Size(450, 150);
            companyLabel.TextAlign = ContentAlignment.MiddleCenter;
            companyLabel.Text = "CÔNG TY CỔ PHẦN GIÁO DỤC\r\nKHOA HỌC CÔNG NGHỆ\r\nICHI SKILL";

            // Copy right
            copyRightLabel.Font = new Font("Arial", 10F, FontStyle.Italic);
            copyRightLabel.ForeColor = Color.Maroon;
            copyRightLabel.Size = new Size(450, 40);
            copyRightLabel.Location = new Point(0, 710);
            copyRightLabel.TextAlign = ContentAlignment.MiddleCenter;
            copyRightLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            copyRightLabel.Text = "Copyright (c) PB.BaoGa(TM), All rights reserved.";

            leftPanel.Controls.Add(logoIchi);
            leftPanel.Controls.Add(companyLabel);
            leftPanel.Controls.Add(copyRightLabel);

         
            // RIGHT PANEL
            
            rightPanel = new Panel();
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.BackColor = Color.FromArgb(119, 167, 44);

            titleLabel = new Label();
            titleLabel.Text = "Đăng Nhập";
            titleLabel.Font = new Font("Arial", 40F, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(260, 40);

            // Icon user
            iconUser = new PictureBox();
            iconUser.Image = Properties.Resources.icon_info;
            iconUser.Size = new Size(40, 40);
            iconUser.Location = new Point(170, 150);
            iconUser.SizeMode = PictureBoxSizeMode.Zoom;

            // Username
            usernameBox = new TextBox();
            usernameBox.Font = new Font("Arial", 16F);
            usernameBox.Location = new Point(220, 150);
            usernameBox.Width = 350;
            usernameBox.PlaceholderText = "Tên đăng nhập";

            // Icon pass
            iconPass = new PictureBox();
            iconPass.Image = Properties.Resources.icon_lock;
            iconPass.Size = new Size(40, 40);
            iconPass.Location = new Point(170, 220);
            iconPass.SizeMode = PictureBoxSizeMode.Zoom;

            // Password
            passwordBox = new TextBox();
            passwordBox.Font = new Font("Arial", 16F);
            passwordBox.Location = new Point(220, 220);
            passwordBox.Width = 350;
            passwordBox.UseSystemPasswordChar = true;
            passwordBox.PlaceholderText = "Mật khẩu";

            // Icon Eye
            iconEye = new PictureBox();
            iconEye.Image = Properties.Resources.icon_eye;
            iconEye.Size = new Size(40, 40);
            iconEye.Location = new Point(580, 220);
            iconEye.SizeMode = PictureBoxSizeMode.Zoom;
            iconEye.Cursor = Cursors.Hand;

            // Checkbox Ghi nhớ
            rememberCheck = new CheckBox();
            rememberCheck.Text = "Ghi Nhớ";
            rememberCheck.Font = new Font("Arial", 12F);
            rememberCheck.Location = new Point(220, 280);

            // Login Button
            loginButton = new Button();
            loginButton.Text = "Đăng Nhập";
            loginButton.Font = new Font("Arial", 20F, FontStyle.Bold);
            loginButton.BackColor = Color.FromArgb(252, 208, 9);
            loginButton.ForeColor = Color.Black;
            loginButton.FlatStyle = FlatStyle.Flat;
            loginButton.FlatAppearance.BorderSize = 3;
            loginButton.Location = new Point(300, 330);
            loginButton.Size = new Size(220, 60);

            // Status Label
            statusLabel = new Label();
            statusLabel.Font = new Font("Arial", 14F, FontStyle.Bold);
            statusLabel.Location = new Point(260, 410);
            statusLabel.AutoSize = true;
            statusLabel.Text = "Kết Nối Máy Chủ Sẵn Sàng";

            // Info
            infoLabel = new Label();
            infoLabel.Font = new Font("Arial", 16F);
            infoLabel.Location = new Point(150, 460);
            infoLabel.AutoSize = true;
            infoLabel.Text =
                "- Sản phẩm cung cấp bởi: ICHI SKILL\n" +
                "- Hỗ trợ: Ultra Viewer\n" +
                "- Điện thoại: 0707 868 600 - 0789 636 979\n" +
                "- Email: hcns@IchiSkillManager.edu.vn\n" +
                "- Địa chỉ: 231/1 Nguyễn Phúc Chu, Phường 15, Quận Tân Bình";

            // ICON FACEBOOK
            iconFB = new PictureBox();
            iconFB.Image = Properties.Resources.icon_facebook;
            iconFB.Size = new Size(45, 45);
            iconFB.Location = new Point(330, 620);
            iconFB.SizeMode = PictureBoxSizeMode.Zoom;

            // ICON WWW
            iconWWW = new PictureBox();
            iconWWW.Image = Properties.Resources.Icon_www;
            iconWWW.Size = new Size(45, 45);
            iconWWW.Location = new Point(390, 620);
            iconWWW.SizeMode = PictureBoxSizeMode.Zoom;

            // ADD TO RIGHT PANEL
            rightPanel.Controls.Add(titleLabel);
            rightPanel.Controls.Add(iconUser);
            rightPanel.Controls.Add(usernameBox);
            rightPanel.Controls.Add(iconPass);
            rightPanel.Controls.Add(passwordBox);
            rightPanel.Controls.Add(iconEye);
            rightPanel.Controls.Add(rememberCheck);
            rightPanel.Controls.Add(loginButton);
            rightPanel.Controls.Add(statusLabel);
            rightPanel.Controls.Add(infoLabel);
            rightPanel.Controls.Add(iconFB);
            rightPanel.Controls.Add(iconWWW);

           //FORM
            this.ClientSize = new Size(1400, 750);
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Text = "Đăng Nhập Hệ Thống";
        }

     
        // ROUND PICTURE BOX CLASS
       
        public class RoundPictureBox : PictureBox
        {
            public string InnerText { get; set; } = "";
            public Color TextColor { get; set; } = Color.Black;
            public Color CircleBackColor { get; set; } = Color.White;

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, Width - 1, Height - 1);
                this.Region = new Region(gp);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Nền tròn trắng
                using (SolidBrush bg = new SolidBrush(CircleBackColor))
                    e.Graphics.FillEllipse(bg, 0, 0, Width - 1, Height - 1);

                // BÓNG ĐỔ
                using (SolidBrush shadow = new SolidBrush(Color.FromArgb(60, 0, 0, 0)))
                    e.Graphics.FillEllipse(shadow, 8, 8, Width - 16, Height - 16);

                // Vẽ text
                if (!string.IsNullOrEmpty(InnerText))
                {
                    using (Font f = new Font("Arial", 48F, FontStyle.Bold))
                    using (SolidBrush b = new SolidBrush(TextColor))
                    {
                        SizeF textSize = e.Graphics.MeasureString(InnerText, f);
                        float x = (Width - textSize.Width) / 2;
                        float y = (Height - textSize.Height) / 2;
                        e.Graphics.DrawString(InnerText, f, b, x, y);
                    }
                }

                base.OnPaint(e);
            }
        }
    }
}
