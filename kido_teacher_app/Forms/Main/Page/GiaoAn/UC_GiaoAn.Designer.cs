using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    partial class UC_GiaoAn
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblWelcome;
        private Label lblTitle;
        private Panel contentPanel;
        private FlowLayoutPanel flowGiaoAn;

        private Panel itemTemplate;
        private PictureBox picItem;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Designer generated code
        private void InitializeComponent()
        {
            this.lblWelcome = new Label();
            this.lblTitle = new Label();
            this.contentPanel = new Panel();
            this.flowGiaoAn = new FlowLayoutPanel();

            this.itemTemplate = new Panel();
            this.picItem = new PictureBox();

            this.contentPanel.SuspendLayout();
            this.itemTemplate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picItem)).BeginInit();
            this.SuspendLayout();

            // ================= HEADER XÁM =================
            this.lblWelcome.Text = "Chào Mừng Bạn Đến Với KIDO";
            this.lblWelcome.Dock = DockStyle.Top;
            this.lblWelcome.Height = 55;
            this.lblWelcome.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            this.lblWelcome.BackColor = Color.FromArgb(220, 220, 220);
            this.lblWelcome.TextAlign = ContentAlignment.MiddleLeft;
            this.lblWelcome.Padding = new Padding(15, 0, 0, 0);

            // ================= HEADER XANH =================
            this.lblTitle.Text = "Giáo Án";
            this.lblTitle.Dock = DockStyle.Top;
            this.lblTitle.Height = 45;
            this.lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            this.lblTitle.BackColor = Color.FromArgb(146, 208, 80);
            this.lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            this.lblTitle.Padding = new Padding(20, 0, 0, 0);

            // ================= CONTENT PANEL =================
            this.contentPanel.Dock = DockStyle.Fill;
            this.contentPanel.AutoScroll = true;
            this.contentPanel.Controls.Add(this.flowGiaoAn);

            // ================= FLOW PANEL =================
            this.flowGiaoAn.Dock = DockStyle.Fill;
            this.flowGiaoAn.WrapContents = true;
            this.flowGiaoAn.AutoScroll = true;
            this.flowGiaoAn.Padding = new Padding(20);

            // ================= ITEM CARD =================
            this.itemTemplate.BackColor = Color.White;
            this.itemTemplate.Size = new Size(240, 240);
            this.itemTemplate.Margin = new Padding(20);
            this.itemTemplate.Cursor = Cursors.Hand;

            //this.itemTemplate.Paint += ItemTemplate_Paint;
            //this.itemTemplate.MouseEnter += Item_MouseEnter;
            //this.itemTemplate.MouseLeave += Item_MouseLeave;
            //this.itemTemplate.Click += Item_Click;
            this.itemTemplate.Paint += ItemTemplate_Paint;
            this.itemTemplate.MouseEnter += Item_MouseEnter;
            this.itemTemplate.MouseLeave += Item_MouseLeave;
            this.itemTemplate.Click += Item_Click;

            this.picItem.MouseEnter += Item_MouseEnter;
            this.picItem.MouseLeave += Item_MouseLeave;
            this.picItem.Click += Item_Click;

            // ================= IMAGE =================
            this.picItem.Image = Properties.Resources.coursedefault1;
            this.picItem.SizeMode = PictureBoxSizeMode.StretchImage;
            this.picItem.Location = new Point(8, 8);
            this.picItem.Size = new Size(224, 224);
            this.picItem.Cursor = Cursors.Hand;

            this.picItem.MouseEnter += Item_MouseEnter;
            this.picItem.MouseLeave += Item_MouseLeave;
            this.picItem.Click += Item_Click;

            this.itemTemplate.Controls.Add(this.picItem);
            // ⭐ XÓA DÒNG NÀY - KHÔNG ADD PLACEHOLDER
            // this.flowGiaoAn.Controls.Add(this.itemTemplate);

            // ================= ADD TO MAIN =================
            this.Controls.Add(this.contentPanel);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblWelcome);

            this.Size = new Size(1100, 650);

            this.contentPanel.ResumeLayout(false);
            this.itemTemplate.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picItem)).EndInit();
            this.ResumeLayout(false);
        }
        #endregion
    }
}

