using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    partial class UC_GiaoAnTheoThang
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblWelcome;
        private Label lblTitle;
        private FlowLayoutPanel flowMonths;
        private PictureBox btnBack;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lblWelcome = new Label();
            this.lblTitle = new Label();
            this.flowMonths = new FlowLayoutPanel();
            this.btnBack = new PictureBox();

            this.SuspendLayout();

            // ===== HEADER XÁM =====
            lblWelcome.Text = "Chào Mừng Bạn Đến Với KIDO";
            lblWelcome.Dock = DockStyle.Top;
            lblWelcome.Height = 55;
            lblWelcome.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            lblWelcome.BackColor = Color.LightGray;

            // ===== HEADER XANH =====
            lblTitle.Text = "Giáo Án / Khối 1";
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 45;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.BackColor = Color.FromArgb(146, 208, 80);

            // ===== BACK BUTTON =====
            btnBack.Image = Properties.Resources.icon_back1;
            btnBack.SizeMode = PictureBoxSizeMode.StretchImage;
            btnBack.Size = new Size(30, 30);
            btnBack.Cursor = Cursors.Hand;
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);

            // ===== FLOW MONTHS =====
            flowMonths.Dock = DockStyle.Fill;
            flowMonths.AutoScroll = true;
            flowMonths.WrapContents = true;
            flowMonths.FlowDirection = FlowDirection.LeftToRight;
            flowMonths.Padding = new Padding(20, 20, 20, 20);

            //// ===== TẠO 12 THÁNG =====
            //for (int i = 1; i <= 12; i++)
            //{
            //    Panel monthPanel = CreateMonthItem(i);
            //    flowMonths.Controls.Add(monthPanel);
            //}

            this.Controls.Add(flowMonths);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblWelcome);

            this.Resize += UC_GiaoAnTheoThang_Resize;

            this.ResumeLayout(false);
        }
    }
}
