using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    partial class UC_LopChiTiet
    {
        private Panel panelLeft;
        private Panel panelRight;
        private Panel panelCenter;
        private Panel panelInfo;

        private TextBox txtSearch;
        private Panel panelCategory;
        private Panel panelCost;

        private Label lblDanhMuc;
        private Button btnAll;

        private Label lblCost;
        private CheckBox cbFree;

        private Panel panelHeader;
        private PictureBox picBack;
        private Label lblAllTitle;

        private FlowLayoutPanel flowExamList;

        private Panel panelPaging;
        private TableLayoutPanel pagingLayout;
        private PictureBox picPrev;
        private PictureBox picNext;
        private Label lblPage;

        // ===== INFO =====
        private Label lblInfoTitle;
        private PictureBox picBanner;
        private Label lblPackageName;
        private Label lblStats;
        private Label lblExpire;
        private Button btnActive;
        private Label lblDescTitle;
        private Label lblDescription;

        private void InitializeComponent()
        {
            SuspendLayout();

            // ================= LEFT =================
            panelLeft = new Panel
            {
                Dock = DockStyle.Left,
                Width = 300,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15)
            };

            txtSearch = new TextBox
            {
                Dock = DockStyle.Top,
                Height = 38,
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Tìm kiếm tên đề thi",
                Margin = new Padding(0, 0, 0, 15)
            };

            panelCategory = new Panel
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Color.White,
                Padding = new Padding(15),
                Margin = new Padding(0, 0, 0, 15)
            };

            lblDanhMuc = new Label
            {
                Text = "Danh mục",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            btnAll = new Button
            {
                Text = "Tất cả đề thi (23)",
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(196, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnAll.FlatAppearance.BorderSize = 0;

            panelCategory.Controls.Add(btnAll);
            panelCategory.Controls.Add(lblDanhMuc);

            panelCost = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            lblCost = new Label
            {
                Text = "Chi phí",
                Dock = DockStyle.Top,
                Height = 30,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            cbFree = new CheckBox
            {
                Text = "Miễn phí",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F)
            };

            panelCost.Controls.Add(cbFree);
            panelCost.Controls.Add(lblCost);

            panelLeft.Controls.Add(panelCost);
            panelLeft.Controls.Add(panelCategory);
            panelLeft.Controls.Add(txtSearch);

            // ================= RIGHT ROOT =================
            panelRight = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            // ================= CENTER =================
            panelCenter = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0, 0, 20, 0),
                BackColor = Color.White
            };

            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45
            };

            picBack = new PictureBox
            {
                Image = Properties.Resources.icon_back1,
                Dock = DockStyle.Left,
                Width = 30,
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };
            picBack.Click += PicBack_Click;
            lblAllTitle = new Label
            {
                Text = "Tất cả đề thi",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold)
            };

            panelHeader.Controls.Add(lblAllTitle);
            panelHeader.Controls.Add(picBack);

            flowExamList = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };

            panelPaging = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50
            };

            pagingLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3
            };
            pagingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pagingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pagingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            picPrev = new PictureBox
            {
                Image = Properties.Resources.icon_trai,
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Right,
                Cursor = Cursors.Hand
            };

            lblPage = new Label
            {
                Text = "1 / 1",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Anchor = AnchorStyles.None,
                AutoSize = true
            };

            picNext = new PictureBox
            {
                Image = Properties.Resources.icon_phai,
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Left,
                Cursor = Cursors.Hand
            };

            pagingLayout.Controls.Add(picPrev, 0, 0);
            pagingLayout.Controls.Add(lblPage, 1, 0);
            pagingLayout.Controls.Add(picNext, 2, 0);

            panelPaging.Controls.Add(pagingLayout);

            panelCenter.Controls.Add(flowExamList);
            panelCenter.Controls.Add(panelPaging);
            panelCenter.Controls.Add(panelHeader);

            // ================= INFO (GÓI THI) =================
            panelInfo = new Panel
            {
                Dock = DockStyle.Right,
                Width = 360,
                BackColor = Color.FromArgb(245, 247, 250),
                Padding = new Padding(15),

            };

            //lblInfoTitle = new Label
            //{
            //    Text = "Gói thi",
            //    Dock = DockStyle.Top,
            //    Height = 40,
            //    Font = new Font("Segoe UI", 13F, FontStyle.Bold)
            //};

            picBanner = new PictureBox
            {
                Dock = DockStyle.Top,

                Height = 220,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Margin = new Padding(0, 0, 0, 15),
                Image = Properties.Resources.classdefault
            };

            lblPackageName = new Label
            {
                Text = "Trọn bộ đề thi Tiếng Anh lớp 1 sách Explore Our World (có đáp án)",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Padding = new Padding(0, 8, 0, 8),
                Margin = new Padding(0, 5, 0, 5),
                MaximumSize = new Size(panelInfo.Width - 30, 0),
                AutoSize = true
            };

            lblStats = new Label
            {
                Text = "👁 14.909    ♥ 6    ♡ Yêu thích",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Gray,
                Margin = new Padding(0, 0, 0, 10)
            };

            lblExpire = new Label
            {
                Text = "Thời hạn sử dụng: 12 tháng",
                Dock = DockStyle.Top,
                Height = 35,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(245, 236, 220),
                ForeColor = Color.FromArgb(160, 110, 0),
                Margin = new Padding(0, 10, 0, 15),
            };

            btnActive = new Button
            {
                Text = "Kích hoạt miễn phí",
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(0, 140, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 20)
            };
            btnActive.FlatAppearance.BorderSize = 0;

            lblDescTitle = new Label
            {
                Text = "Mô tả",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                Padding = new Padding(0, 15, 0, 5)
            };

            lblDescription = new Label
            {
                Text =
        "Trọn bộ đề thi Tiếng Anh lớp 1 sách Explore Our World (có đáp án) "
        + "tổng hợp các dạng đề: kiểm tra học kỳ, 45 phút, 15 phút, "
        + "phiếu luyện tập và phiếu bài tập.",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                MaximumSize = new Size(panelInfo.Width - 30, 0), // ⭐ BẮT BUỘC
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };

            panelInfo.SizeChanged += (s, e) =>
            {
                lblPackageName.MaximumSize = new Size(panelInfo.Width - 30, 0);
                lblDescription.MaximumSize = new Size(panelInfo.Width - 30, 0);
            };

            panelInfo.Controls.Add(lblDescription);
            panelInfo.Controls.Add(lblDescTitle);
            panelInfo.Controls.Add(btnActive);
            panelInfo.Controls.Add(lblExpire);
            panelInfo.Controls.Add(lblStats);
            panelInfo.Controls.Add(lblPackageName);
            panelInfo.Controls.Add(picBanner);
            //panelInfo.Controls.Add(lblInfoTitle);

            // ================= ADD =================
            panelRight.Controls.Add(panelCenter);
            panelRight.Controls.Add(panelInfo);

            Controls.Add(panelRight);
            Controls.Add(panelLeft);

            Size = new Size(1400, 720);
            BackColor = Color.White;

            ResumeLayout(false);
        }
    }
}
