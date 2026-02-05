using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi
{
    partial class UC_BaiThi_Main
    {
        private Label lblHeader;
        private Label lblTitle;
        
        private Panel panelContent;

        

        private void InitializeComponent()
        {
            lblHeader = new Label();
            lblTitle = new Label();
          
            panelContent = new Panel();
            SuspendLayout();
            // 
            // lblHeader
            // 
            lblHeader.BackColor = Color.FromArgb(230, 230, 230);
            lblHeader.Dock = DockStyle.Top;
            lblHeader.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
            lblHeader.Location = new Point(0, 0);
            lblHeader.Name = "lblHeader";
            lblHeader.Size = new Size(1390, 60);
            lblHeader.TabIndex = 3;
            lblHeader.Text = "Chào Mừng Bạn Đến Với KIDO";
            lblHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblTitle
            // 
            lblTitle.BackColor = Color.FromArgb(150, 220, 80);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            lblTitle.Location = new Point(0, 60);
            lblTitle.Name = "lblTitle";
            lblTitle.Padding = new Padding(15, 0, 0, 0);
            lblTitle.Size = new Size(1390, 45);
            lblTitle.TabIndex = 2;
            lblTitle.Text = "BÀI THI";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;



            // ================= FILTER =================
            panelFilter = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10, 10, 10, 5)
            };

            FlowLayoutPanel flp = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight
            };

            Label lblClass = new Label
            {
                Text = "Lớp:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 5, 0)
            };

            cbClass = new ComboBox
            {
                Width = 400,
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

            Label lblFilter = new Label
            {
                Text = "Lọc:",
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 6, 5, 0)
            };

            txtFilter = new TextBox
            {
                Width = 350,
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Nhập tên lớp..."
            };

            flp.Controls.Add(lblClass);
            flp.Controls.Add(cbClass);
            flp.Controls.Add(btnSearch);
            flp.Controls.Add(lblFilter);
            flp.Controls.Add(txtFilter);

            panelFilter.Controls.Add(flp);

            // 
            // panelContent
            // 
            panelContent.BackColor = Color.White;
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 155);
            panelContent.Name = "panelContent";
            panelContent.Size = new Size(1390, 590);
            panelContent.TabIndex = 0;
            // 
            // UC_BaiThi
            // 
            Controls.Add(panelContent);
            Controls.Add(panelFilter);
            Controls.Add(lblTitle);
            Controls.Add(lblHeader);
            Name = "UC_BaiThi";
            Size = new Size(1390, 745);
            ResumeLayout(false);
        }
    }
}
