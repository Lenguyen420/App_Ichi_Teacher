using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    partial class UC_GioiThieu
    {
        private System.ComponentModel.IContainer components = null;

        private Panel container;
        private Panel topHeader;
        private Panel header1;
        private Panel header2;
        private Label lblTitle;
        private Label lblCompany;

        private Panel bannerContainer;
        private PictureBox banner;
        private LinkLabel detailLink;

        private Panel footer;
        private PictureBox pictureHopDong;
        private LinkLabel editImg, editInfo;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ================= CONTAINER =================
            container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // ================= TOP HEADER =================
            topHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110
            };

            header1 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            lblTitle = new Label
            {
                Text = "Chào Mừng Bạn Đến Với KIDO",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            header1.Controls.Add(lblTitle);

            header2 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(76, 175, 80)
            };

            lblCompany = new Label
            {
                Text = "CÔNG TY CỔ PHẦN GIÁO DỤC KHOA HỌC CÔNG NGHỆ ICHI SKILL",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header2.Controls.Add(lblCompany);

            topHeader.Controls.Add(header2);
            topHeader.Controls.Add(header1);

            // ================= BANNER =================
            bannerContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            banner = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.slide1
            };

            detailLink = new LinkLabel
            {
                Text = "Xem Chi Tiết",
                AutoSize = true,
                Font = new Font("Segoe UI", 12),
                LinkColor = Color.Blue,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            bannerContainer.Resize += (s, e) =>
            {
                detailLink.Location = new Point(
                    bannerContainer.Width - detailLink.Width - 20,
                    10
                );
            };

            bannerContainer.Controls.Add(banner);
            bannerContainer.Controls.Add(detailLink);

            // ================= FOOTER =================
            footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 160,
                BackColor = Color.FromArgb(220, 230, 245)
            };

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3
            };

            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

            footer.Controls.Add(layout);

            // LEFT PANEL
            var leftPanel = new Panel { Dock = DockStyle.Fill };

            leftPanel.Controls.AddRange(new Control[]
            {
                MakeIcon(Properties.Resources.icon_address, 10, 10),
                MakeFooterLabel("Địa chỉ: 231/1 Nguyễn Phúc Chu, Phường 15, Quận Tân Bình", 40, 10),

                MakeIcon(Properties.Resources.icon_phone, 10, 40),
                MakeFooterLabel("Điện thoại: 0707 868 600 – 0789 636 979", 40, 40),

                MakeIcon(Properties.Resources.icon_mail, 10, 70),
                MakeFooterLabel("Email: hcns@IchiSkillManager.edu.vn", 40, 70),

                MakeIcon(Properties.Resources.Icon_www, 10, 100),
                MakeFooterLabel("Website: https://IchiSkillManager.edu.vn", 40, 100)
            });

            layout.Controls.Add(leftPanel, 0, 0);

            // CENTER PANEL — ẢNH HỢP ĐỒNG CAO = FOOTER
            var centerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            pictureHopDong = new PictureBox
            {
                Image = Properties.Resources.hopdong,
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom   // ⭐ giữ tỉ lệ, FULL chiều cao footer
            };

            centerPanel.Controls.Add(pictureHopDong);
            layout.Controls.Add(centerPanel, 1, 0);

            // RIGHT PANEL — CANH LỀ PHẢI
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            editImg = new LinkLabel
            {
                Text = "Chỉnh Sửa Hình Ảnh Giới Thiệu",
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            editImg.LinkClicked += EditImg_LinkClicked;


            editInfo = new LinkLabel
            {
                Text = "Chỉnh Sửa Thông Tin Công Ty",
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            editInfo.LinkClicked += EditInfo_LinkClicked;

            rightPanel.Controls.Add(editImg);
            rightPanel.Controls.Add(editInfo);

            // ⭐ canh sát lề phải bằng Resize
            rightPanel.Resize += (s, e) =>
            {
                int padding = 20;

                editImg.Location = new Point(
                    rightPanel.Width - editImg.Width - padding,
                    35
                );

                editInfo.Location = new Point(
                    rightPanel.Width - editInfo.Width - padding,
                    65
                );
            };

            layout.Controls.Add(rightPanel, 2, 0);

            // ================= ADD =================
            container.Controls.Add(bannerContainer);
            container.Controls.Add(topHeader);
            container.Controls.Add(footer);

            Controls.Add(container);
            Size = new Size(1100, 650);
        }

        private PictureBox MakeIcon(Image img, int x, int y)
        {
            return new PictureBox
            {
                Image = img,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(20, 20),
                Location = new Point(x, y)
            };
        }

        private Label MakeFooterLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11),
                AutoSize = true,
                Location = new Point(x, y),
                MaximumSize = new Size(650, 0)
            };
        }
    }
}
