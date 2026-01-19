using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.GioiThieu
{
    partial class Form_ChinhSuaThongTinCongTy
    {
        private System.ComponentModel.IContainer components = null;

        // ===== HEADER =====
        private Panel headerPanel;
        private PictureBox picLogo;
        private Label lblHeader;
        private PictureBox btnClose;

        // ===== INPUT =====
        private Label lblTenCongTy;
        private TextBox txtTenCongTy;

        private Label lblTenCongTyDayDu;
        private TextBox txtTenCongTyDayDu;

        private Label lblSoDienThoai;
        private TextBox txtSoDienThoai;

        private Label lblEmail;
        private TextBox txtEmail;

        private Label lblDiaChi;
        private TextBox txtDiaChi;

        private Label lblWebsite;
        private TextBox txtWebsite;

        private Label lblFacebook;
        private TextBox txtFacebook;

        private Label lblYoutube;
        private TextBox txtYoutube;

        private Label lblSupport;
        private TextBox txtSupport;

        // ===== BUTTON =====
        private Button btnCapNhat;
        private Button btnTroVe;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // ===== FORM =====
            this.Text = "Thông Tin Công Ty";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(800, 420);
            this.BackColor = Color.FromArgb(245, 245, 245);
            this.MaximizeBox = false;

            // ================= HEADER =================
            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(0, 122, 204)
            };

            picLogo = new PictureBox
            {
                Image = Properties.Resources.logo4,
                Size = new Size(28, 28),
                Location = new Point(10, 8),
                SizeMode = PictureBoxSizeMode.StretchImage
            };

            lblHeader = new Label
            {
                Text = "Thông Tin Công Ty",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 10)
            };

            //btnClose = new PictureBox
            //{
            //    Size = new Size(26, 26),
            //    Location = new Point(760, 9),
            //    Cursor = Cursors.Hand,
            //    SizeMode = PictureBoxSizeMode.StretchImage,
            //    Image = Properties.Resources.icon_close
            //};
            //btnClose.Click += (s, e) => this.Close();

            headerPanel.Controls.Add(picLogo);
            headerPanel.Controls.Add(lblHeader);
            //headerPanel.Controls.Add(btnClose);
            this.Controls.Add(headerPanel);

            // ================= LAYOUT =================
            int lblX = 30;
            int txtX = 180;
            int rowY = 70;
            int rowH = 32;
            int txtWidth = 560;

            lblTenCongTy = new Label { Text = "Tên Công Ty:", Location = new Point(lblX, rowY), AutoSize = true };
            txtTenCongTy = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblTenCongTyDayDu = new Label { Text = "Tên Cty Đầy Đủ:", Location = new Point(lblX, rowY), AutoSize = true };
            txtTenCongTyDayDu = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblSoDienThoai = new Label { Text = "Số Điện Thoại:", Location = new Point(lblX, rowY), AutoSize = true };
            txtSoDienThoai = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblEmail = new Label { Text = "Email:", Location = new Point(lblX, rowY), AutoSize = true };
            txtEmail = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblDiaChi = new Label { Text = "Địa Chỉ:", Location = new Point(lblX, rowY), AutoSize = true };
            txtDiaChi = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblWebsite = new Label { Text = "Website:", Location = new Point(lblX, rowY), AutoSize = true };
            txtWebsite = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblFacebook = new Label { Text = "Meta/Facebook:", Location = new Point(lblX, rowY), AutoSize = true };
            txtFacebook = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblYoutube = new Label { Text = "Youtube:", Location = new Point(lblX, rowY), AutoSize = true };
            txtYoutube = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            rowY += rowH;
            lblSupport = new Label { Text = "Support:", Location = new Point(lblX, rowY), AutoSize = true };
            txtSupport = new TextBox { Location = new Point(txtX, rowY - 4), Width = txtWidth };

            // ================= BUTTON =================
            btnCapNhat = new Button
            {
                Text = "Cập Nhật",
                Size = new Size(110, 34),
                Location = new Point(30, 360),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };

            btnTroVe = new Button
            {
                Text = "Trở Về",
                Size = new Size(110, 34),
                Location = new Point(650, 360),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnTroVe.Click += (s, e) => this.Close();

            // ================= ADD =================
            this.Controls.AddRange(new Control[]
            {
                lblTenCongTy, txtTenCongTy,
                lblTenCongTyDayDu, txtTenCongTyDayDu,
                lblSoDienThoai, txtSoDienThoai,
                lblEmail, txtEmail,
                lblDiaChi, txtDiaChi,
                lblWebsite, txtWebsite,
                lblFacebook, txtFacebook,
                lblYoutube, txtYoutube,
                lblSupport, txtSupport,
                btnCapNhat, btnTroVe
            });
        }
    }
}
