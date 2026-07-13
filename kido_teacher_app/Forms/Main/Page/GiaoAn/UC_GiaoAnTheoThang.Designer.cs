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
            lblWelcome.Text = "Chào Mừng Bạn Đến Với Ichi Skill";
            lblWelcome.Dock = DockStyle.Top;
            lblWelcome.Height = 50;
            lblWelcome.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblWelcome.BackColor = Color.FromArgb(220, 220, 220);
            lblWelcome.TextAlign = ContentAlignment.MiddleLeft;
            lblWelcome.Padding = new Padding(20, 0, 0, 0);

            // ===== HEADER XANH =====
            lblTitle.Text = "Giáo Án / Khối 1";
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 40;
            lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitle.BackColor = Color.FromArgb(146, 208, 80);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblTitle.Padding = new Padding(20, 0, 0, 0);

            // ===== BACK BUTTON =====
            btnBack.Image = Properties.Resources.icon_back1;
            btnBack.SizeMode = PictureBoxSizeMode.StretchImage;
            btnBack.Size = new Size(30, 30);
            btnBack.Cursor = Cursors.Hand;
            btnBack.BackColor = lblTitle.BackColor;
            btnBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            //btnBack.Location = new Point(1050, 55);

            lblTitle.Controls.Add(btnBack);

            btnBack.Location = new Point(
                lblTitle.Width - btnBack.Width - 10,
                (lblTitle.Height - btnBack.Height) / 2
            );
            lblTitle.Resize += (s, e) =>
            {
                btnBack.Location = new Point(
                    lblTitle.Width - btnBack.Width - 10,
                    (lblTitle.Height - btnBack.Height) / 2
                );
            };

            btnBack.Click += BtnBack_Click;

           

            // ===== FLOW MONTHS =====
            flowMonths.Dock = DockStyle.Fill;
            flowMonths.AutoScroll = true;
            flowMonths.WrapContents = true;
            flowMonths.Padding = new Padding(8, 20, 8, 20);
            flowMonths.BackColor = Color.White;

            //// ===== TẠO 12 THÁNG =====
            //for (int i = 1; i <= 12; i++)
            //{
            //    Panel monthPanel = CreateMonthItem(i);
            //    flowMonths.Controls.Add(monthPanel);
            //}

            this.Controls.Add(flowMonths);
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblWelcome);
            //this.Controls.Add(btnBack);
            //this.Controls.SetChildIndex(btnBack, 0);

            this.ResumeLayout(false);
        }
    }
}
