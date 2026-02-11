using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    partial class UC_Lop_KetQua
    {
        private IContainer components = null;

        // ===== ROOT =====
        private Panel panelHeader;
        private Panel panelBody;

        // ===== HEADER =====
        private PictureBox picBack;
        private TableLayoutPanel tblHeaderStats;
        private Label lblTong, lblDung, lblSai, lblTuLuan, lblChuaLam;

        // ===== BODY =====
        private Panel panelLeft;
        private Panel panelCenter;
        private Panel panelRight;

        // ===== LEFT =====
        private Label lblLeftTitle;
        private FlowLayoutPanel panelDeThi;

        // ===== CENTER =====
        private FlowLayoutPanel flowKetQua;

        // ===== RIGHT =====
        private Label lblKetQuaTitle;
        private Label lblTenBaiThi;
        private Label lblDiem;
        private Label lblXepHang;
        private Button btnLamLai;
        private Button btnDong;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new Container();
            SuspendLayout();

            // ================= ROOT =================
            BackColor = Color.White;
            Size = new Size(1200, 700);

            // ================= HEADER =================
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };

            picBack = new PictureBox
            {
                Image = Properties.Resources.icon_back1,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(28, 28),
                Location = new Point(12, 16),
                Cursor = Cursors.Hand
            };
            picBack.Click += btnDong_Click;

            tblHeaderStats = new TableLayoutPanel
            {
                AutoSize = true,
                ColumnCount = 5
            };

            lblTong = CreateHeaderFilter("Tất cả (0)", Color.Black);
            lblDung = CreateHeaderFilter("Đúng (0)", Color.Green);
            lblSai = CreateHeaderFilter("Sai (0)", Color.Red);
            lblTuLuan = CreateHeaderFilter("Tự luận (0)", Color.FromArgb(60, 60, 60));
            lblChuaLam = CreateHeaderFilter("Chưa làm (0)", Color.Gray);

            tblHeaderStats.Controls.Add(lblTong, 0, 0);
            tblHeaderStats.Controls.Add(lblDung, 1, 0);
            tblHeaderStats.Controls.Add(lblSai, 2, 0);
            tblHeaderStats.Controls.Add(lblTuLuan, 3, 0);
            tblHeaderStats.Controls.Add(lblChuaLam, 4, 0);

            panelHeader.Controls.Add(tblHeaderStats);
            panelHeader.Controls.Add(picBack);

            panelHeader.Resize += (s, e) =>
            {
                tblHeaderStats.Left = (panelHeader.Width - tblHeaderStats.Width) / 2;
                tblHeaderStats.Top = 18;
            };

            // ================= BODY =================
            panelBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250)
            };

            // ================= LEFT =================
            panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 260,
                BackColor = Color.White,
                Padding = new Padding(10)
            };

            lblLeftTitle = new Label
            {
                Text = "Làm tiếp các đề thi khác",
                Dock = DockStyle.Top,
                Height = 44,
                Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
                Padding = new Padding(6, 0, 0, 10)
            };

            panelDeThi = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            panelLeft.Controls.Add(panelDeThi);
            panelLeft.Controls.Add(lblLeftTitle);

            // ================= RIGHT =================
            panelRight = new Panel
            {
                Dock = DockStyle.Right,
                Width = 260,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            lblKetQuaTitle = new Label
            {
                Text = "KẾT QUẢ BÀI THI",
                Dock = DockStyle.Top,
                Height = 34,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 90, 160),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblTenBaiThi = new Label
            {
                Text = "Đề kiểm tra 15 phút - Đề số 02",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                AutoSize = true,
                MaximumSize = new Size(230, 0),
                TextAlign = ContentAlignment.MiddleCenter,
                Padding = new Padding(0, 8, 0, 12)
            };

            lblDiem = new Label
            {
                Text = "0",
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 42F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblXepHang = new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            btnLamLai = new Button
            {
                Text = "LÀM LẠI ĐỀ THI",
                Dock = DockStyle.Bottom,
                Height = 45,
                BackColor = Color.SeaGreen,      // xanh lá
                ForeColor = Color.White,         // chữ trắng cho nổi
                FlatStyle = FlatStyle.Flat,
                UseVisualStyleBackColor = false
            };
            btnLamLai.Click += btnLamLai_Click;

            btnDong = new Button
            {
                Text = "ĐÓNG",
                Dock = DockStyle.Bottom,
                Height = 40
            };
            btnDong.Click += btnDong_Click;

            panelRight.Controls.Add(btnLamLai);
            panelRight.Controls.Add(btnDong);
            panelRight.Controls.Add(lblXepHang);
            panelRight.Controls.Add(lblDiem);
            panelRight.Controls.Add(lblTenBaiThi);
            panelRight.Controls.Add(lblKetQuaTitle);

            // ================= CENTER =================
            panelCenter = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            flowKetQua = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            panelCenter.Controls.Add(flowKetQua);

            // ================= ADD ORDER (CỰC KỲ QUAN TRỌNG) =================
            panelBody.Controls.Add(panelCenter); // FILL LUÔN ADD CUỐI
            panelBody.Controls.Add(panelRight);
            panelBody.Controls.Add(panelLeft);

            Controls.Add(panelBody);
            Controls.Add(panelHeader);

            ResumeLayout(false);
        }

        private Label CreateHeaderFilter(string text, Color color)
        {
            return new Label
            {
                Text = text,
                AutoSize = true,
                Margin = new Padding(14, 0, 14, 0),
                ForeColor = color,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }
    }
}
