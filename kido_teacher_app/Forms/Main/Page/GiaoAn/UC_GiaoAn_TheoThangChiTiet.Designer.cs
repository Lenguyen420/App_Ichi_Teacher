namespace kido_teacher_app.Forms.Main.Page.GiaoAn
{
    partial class UC_GiaoAn_TheoThangChiTiet
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblHeader;
        private Label lblInfo;
        private FlowLayoutPanel flowList;

        // ⭐ THÊM FIELD BACK BUTTON
        private PictureBox btnBack;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblHeader = new System.Windows.Forms.Label();
            this.lblInfo = new System.Windows.Forms.Label();
            this.flowList = new System.Windows.Forms.FlowLayoutPanel();

            this.SuspendLayout();

            // ================================
            // HEADER XÁM
            // ================================
            this.lblHeader.Text = "Chào Mừng Bạn Đến Với KIDO";
            this.lblHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHeader.Height = 50;
            this.lblHeader.BackColor = System.Drawing.Color.FromArgb(220, 220, 220);
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblHeader.Padding = new Padding(20, 0, 0, 0);

            // ================================
            // HEADER XANH
            // ================================
            this.lblInfo.Text = "Giáo Án / Khối 1 / Tháng ?";
            this.lblInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblInfo.Height = 40;
            this.lblInfo.BackColor = System.Drawing.Color.FromArgb(146, 208, 80);
            this.lblInfo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblInfo.Padding = new Padding(20, 0, 0, 0);

            /// ================================
            // ⭐ BACK BUTTON (NO TRANSPARENT)
            // ================================
            this.btnBack = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.btnBack)).BeginInit();

            // ảnh gốc
            this.btnBack.Image = Properties.Resources.icon_back1;   // giữ nguyên icon, không xử lý nền

            this.btnBack.SizeMode = PictureBoxSizeMode.StretchImage;
            this.btnBack.Width = 30;
            this.btnBack.Height = 30;
            this.btnBack.Cursor = Cursors.Hand;

            // thêm vào control trước để Resize có thể reposition
            this.Controls.Add(this.btnBack);

            //MakePictureBoxRound(this.btnBack);

            // Click → quay về màn trước
            this.btnBack.Click += BtnBack_Click;

            ((System.ComponentModel.ISupportInitialize)(this.btnBack)).EndInit();

            // ================================
            // FLOW LIST
            // ================================
            this.flowList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowList.AutoScroll = true;
            this.flowList.FlowDirection = FlowDirection.TopDown;
            this.flowList.WrapContents = false;
            this.flowList.Padding = new Padding(15);
            this.flowList.BackColor = System.Drawing.Color.White;

            // ================================
            // ADD CONTROL
            // ================================
            this.Controls.Add(this.flowList);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblHeader);

            // ⭐ ĐĂNG KÝ SỰ KIỆN RESIZE ĐỂ ĐẶT ICON ĐÚNG VỊ TRÍ
            this.Resize += UC_GiaoAn_TheoThangChiTiet_Resize;

            this.Name = "UC_GiaoAn_TheoThangChiTiet";
            this.Size = new System.Drawing.Size(1100, 650);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void UC_GiaoAn_TheoThangChiTiet_Resize(object sender, EventArgs e)
        {
            if (btnBack != null && lblInfo != null)
            {
                int y = lblInfo.Top + (lblInfo.Height - btnBack.Height) / 2;
                int x = this.Width - btnBack.Width - 20;

                btnBack.Location = new Point(x, y);
                btnBack.BringToFront();
            }
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            Control parent = this.Parent;

            if (parent != null)
            {
                parent.Controls.Remove(this);
                parent.Controls.Add(
                    new UC_GiaoAnTheoThang(_className, _classId, _courseId)
                    {
                        Dock = DockStyle.Fill
                    }
                );
            }
        }
    }
}
