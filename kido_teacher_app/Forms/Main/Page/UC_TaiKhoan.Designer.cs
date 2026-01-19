using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    partial class UC_TaiKhoan
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblWelcome;
        private Label titleBar;
        private Panel contentPanel;

        private GroupBox gbAccount;
        private GroupBox gbLicense;

        private LinkLabel llChangePw;
        private LinkLabel llChangeCode;

        private TextBox txtMaTK;
        private TextBox txtTaiKhoan;
        private TextBox txtTen;
        private TextBox txtEmail;
        private TextBox txtLoaiTK;
        private TextBox txtNgayKichHoat;
        private TextBox txtNgayHetHan;

        private TextBox txtMaSP;
        private TextBox txtNgayKH;
        private TextBox txtSoNgay;
        private TextBox txtNgayHH;
        private TextBox txtLoaiSP;


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        #region Designer Code
        private void InitializeComponent()
        {
            contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(20);
            contentPanel.BackColor = Color.White;

            lblWelcome = new Label();
            lblWelcome.Text = "Chào Mừng Bạn Đến Với KIDO";
            lblWelcome.Dock = DockStyle.Top;
            lblWelcome.Height = 45;
            lblWelcome.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblWelcome.BackColor = Color.FromArgb(230, 230, 230);
            lblWelcome.Padding = new Padding(15, 0, 0, 0);

            titleBar = new Label();
            titleBar.Text = "Thông Tin Tài Khoản";
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 40;
            titleBar.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            titleBar.BackColor = Color.FromArgb(146, 208, 80);
            titleBar.Padding = new Padding(15, 0, 0, 0);

            // ========== ACCOUNT GROUP ==========
            gbAccount = new GroupBox();
            gbAccount.Text = "Thông Tin Tài Khoản";
            gbAccount.Dock = DockStyle.Top;
            gbAccount.Height = 240;

            var tblAcc = new TableLayoutPanel();
            tblAcc.Dock = DockStyle.Fill;
            tblAcc.ColumnCount = 4;
            tblAcc.RowCount = 4;

            tblAcc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            tblAcc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            tblAcc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            tblAcc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblAcc.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tblAcc.Padding = new Padding(20, 15, 20, 10);

            TextBox MakeBox()
            {
                return new TextBox
                {
                    Dock = DockStyle.Fill,
                    Font = new Font("Segoe UI", 10),
                    Margin = new Padding(5)
                };
            }

            // Row 1
            tblAcc.Controls.Add(new Label { Text = "Mã Tài Khoản:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
            txtMaTK = MakeBox(); tblAcc.Controls.Add(txtMaTK, 1, 0);

            tblAcc.Controls.Add(new Label { Text = "Loại Tài Khoản:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 0);
            txtLoaiTK = MakeBox(); tblAcc.Controls.Add(txtLoaiTK, 3, 0);

            // Row 2
            tblAcc.Controls.Add(new Label { Text = "Tài Khoản:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
            txtTaiKhoan = MakeBox(); tblAcc.Controls.Add(txtTaiKhoan, 1, 1);

            tblAcc.Controls.Add(new Label { Text = "Ngày Kích Hoạt:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 1);
            txtNgayKichHoat = MakeBox(); tblAcc.Controls.Add(txtNgayKichHoat, 3, 1);

            // Row 3
            tblAcc.Controls.Add(new Label { Text = "Tên Tài Khoản:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
            txtTen = MakeBox(); tblAcc.Controls.Add(txtTen, 1, 2);

            tblAcc.Controls.Add(new Label { Text = "Ngày Hết Hạn:", Anchor = AnchorStyles.Left, AutoSize = true }, 2, 2);
            txtNgayHetHan = MakeBox(); tblAcc.Controls.Add(txtNgayHetHan, 3, 2);

            // Row 4
            tblAcc.Controls.Add(new Label { Text = "Email:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 3);
            txtEmail = MakeBox(); tblAcc.Controls.Add(txtEmail, 1, 3);

            llChangePw = new LinkLabel { Text = "Đổi Mật Khẩu", AutoSize = true, Anchor = AnchorStyles.Left };
            tblAcc.Controls.Add(llChangePw, 3, 3);

            gbAccount.Controls.Add(tblAcc);

            // ========== LICENSE GROUP ==========
            gbLicense = new GroupBox();
            gbLicense.Text = "Thông Tin Bản Quyền";
            gbLicense.Dock = DockStyle.Top;
            gbLicense.Height = 240;
            gbLicense.Margin = new Padding(0, 12, 0, 0);

            var tblLic = new TableLayoutPanel();
            tblLic.Dock = DockStyle.Fill;
            tblLic.ColumnCount = 4;
            tblLic.RowCount = 4;

            tblLic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            tblLic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            tblLic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18));
            tblLic.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));

            tblLic.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblLic.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblLic.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tblLic.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            tblLic.Padding = new Padding(20, 15, 20, 10);

            Label L(string t) => new Label
            {
                Text = t,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            TextBox B() => new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(5),
                Font = new Font("Segoe UI", 10)
            };


            // ===== ROW 1 =====
            tblLic.Controls.Add(L("Mã Sản Phẩm:"), 0, 0);
            txtMaSP = B();
            tblLic.Controls.Add(txtMaSP, 1, 0);
            tblLic.SetColumnSpan(txtMaSP, 3);


            // ===== ROW 2 =====
            tblLic.Controls.Add(L("Ngày Kích Hoạt:"), 0, 1);
            txtNgayKH = B();
            tblLic.Controls.Add(txtNgayKH, 1, 1);

            tblLic.Controls.Add(L("Ngày Hết Hạn:"), 2, 1);
            txtNgayHH = B();
            tblLic.Controls.Add(txtNgayHH, 3, 1);


            // ===== ROW 3 =====
            tblLic.Controls.Add(L("Số Ngày Đăng Ký:"), 0, 2);
            txtSoNgay = B();
            tblLic.Controls.Add(txtSoNgay, 1, 2);

            tblLic.Controls.Add(L("Loại Sản Phẩm:"), 2, 2);
            txtLoaiSP = B();
            tblLic.Controls.Add(txtLoaiSP, 3, 2);


            // ===== ROW 4 (LINK) =====Size = new Size(1200, 700);
            llChangeCode = new LinkLabel
            {
                Text = "Đổi Mã Kích Hoạt",
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            tblLic.Controls.Add(llChangeCode, 0, 3);

            gbLicense.Controls.Add(tblLic);



            contentPanel.Controls.Add(gbLicense);
            contentPanel.Controls.Add(gbAccount);

            Controls.Add(contentPanel);
            Controls.Add(titleBar);
            Controls.Add(lblWelcome);

            //Size = new Size(1200, 700);
            Dock = DockStyle.Fill;
        }
        #endregion
    }
}
