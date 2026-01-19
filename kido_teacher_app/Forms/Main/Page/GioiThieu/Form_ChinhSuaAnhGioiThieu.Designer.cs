using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.QuanLyNoiDung
{
    partial class Form_ChinhSuaAnhGioiThieu : Form
    {
        private System.ComponentModel.IContainer components = null;

        // ===== HEADER =====
        private Panel headerPanel;
        private PictureBox picLogo;
        private Label lblHeader;

        // ===== CONTENT =====
        private FlowLayoutPanel flowImages;

        // ===== BUTTON =====
        private Button btnCapNhat;
        private Button btnThemHinh;
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
            this.Text = "Chỉnh Sửa Ảnh Giới Thiệu";
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new Size(1150, 600);
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
                Text = "Chỉnh Sửa Ảnh Giới Thiệu",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 10)
            };

            headerPanel.Controls.Add(picLogo);
            headerPanel.Controls.Add(lblHeader);
            this.Controls.Add(headerPanel);

            // ================= FLOW IMAGES =================
            flowImages = new FlowLayoutPanel
            {
                Location = new Point(15, 60),
                Size = new Size(1120, 420),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            this.Controls.Add(flowImages);

            // ================= BUTTON =================
            btnCapNhat = new Button
            {
                Text = "Cập Nhật",
                Size = new Size(110, 34),
                Location = new Point(15, 500),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };

            btnThemHinh = new Button
            {
                Text = "Thêm Hình",
                Size = new Size(110, 34),
                Location = new Point(140, 500),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };

            btnTroVe = new Button
            {
                Text = "Trở Về",
                Size = new Size(110, 34),
                Location = new Point(1020, 500),
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand
            };
            btnTroVe.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[]
            {
                btnCapNhat,
                btnThemHinh,
                btnTroVe
            });
        }
    }
}
