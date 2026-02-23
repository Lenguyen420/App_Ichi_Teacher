using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
//using System.Drawing.Drawing2D;

namespace kido_teacher_app.Forms.Main.Page
{
    partial class UC_LichSuBaiThi
    {
        private IContainer components = null;

        private Panel panelHeader;
        private Label lblTitle;

        private Panel panelFilter;
        private Label lblClass;
        private ComboBox cboClass;
        private Button btnSearch;
        private Label lblFilter;
        private TextBox txtSearch;

        private Panel panelContainer;
        private Panel flowHistory;

        private Label lblWelcome;
        private Panel panelTableWrapper;
        private Panel panelHeaderTable;
        private Panel panelBodyTable;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // ===== HEADER XÁM =====
            lblWelcome = new Label
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(230, 230, 230),
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                Text = "Chào Mừng Bạn Đến Với KIDO",
                TextAlign = ContentAlignment.MiddleCenter
            };
            // ===== HEADER XANH =====
            panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(150, 220, 80)
            };

            lblTitle = new Label
            {
                Dock = DockStyle.Fill,
                Text = "LỊCH SỬ BÀI THI",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Padding = new Padding(20, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panelHeader.Controls.Add(lblTitle);

            // ===== FILTER =====
            panelFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(15, 10, 10, 5)
            };

            FlowLayoutPanel flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill
            };

            lblClass = new Label
            {
                Text = "Lớp:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 5, 0)
            };

            cboClass = new ComboBox
            {
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10),
                Margin = new Padding(0, 2, 10, 0)
            };

            btnSearch = new Button
            {
                Text = "Tìm kiếm",
                Width = 90,
                Height = 30,
                Margin = new Padding(0, 2, 10, 0)
            };

            lblFilter = new Label
            {
                Text = "Lọc:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 5, 0)
            };

            txtSearch = new TextBox
            {
                Width = 250,
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Nhập tên bài thi..."
            };

            flp.Controls.Add(lblClass);
            flp.Controls.Add(cboClass);
            flp.Controls.Add(btnSearch);
            flp.Controls.Add(lblFilter);
            flp.Controls.Add(txtSearch);

            panelFilter.Controls.Add(flp);

            // ===== CONTAINER =====
            panelContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            // ===== TABLE WRAPPER (bo góc) =====
            panelTableWrapper = new Panel
            {
                Dock = DockStyle.Left,
                Width = 900,
                BackColor = Color.White,
                Padding = new Padding(10),
            };

            panelContainer.Resize += (s, e) =>
            {
                panelTableWrapper.Width = (int)(panelContainer.Width * 0.7);
            };

            // ===== HEADER TABLE (cố định) =====
            panelHeaderTable = new Panel
            {
                Dock = DockStyle.Top,
                Height = 45,
                BackColor = Color.FromArgb(240, 240, 240)
            };

            // ===== BODY TABLE (scroll) =====
            panelBodyTable = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White
            };

            panelTableWrapper.Controls.Add(panelBodyTable);
            panelTableWrapper.Controls.Add(panelHeaderTable);

            panelContainer.Controls.Add(panelTableWrapper);

            //panelContainer.Resize += (s, e) =>
            //{
            //    flowHistory.Width = (int)(panelContainer.Width * 0.7);
            //};

            // ===== ADD =====
            Controls.Add(panelContainer);
            Controls.Add(panelFilter);
            Controls.Add(panelHeader);
            Controls.Add(lblWelcome);

            Size = new Size(1390, 745);
            ResumeLayout(false);
        }
    }
}