namespace kido_teacher_app
{
    partial class Main_Form
    {
        private System.ComponentModel.IContainer components = null;

        private Panel panelLeft;
        private Panel panelMain;

        public Panel menuGioiThieu;
        public Panel menuTaiKhoan;
        public Panel menuGiaoAn;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.panelLeft = new Panel();
            this.panelMain = new Panel();

            this.SuspendLayout();

            this.panelLeft.BackColor = Color.FromArgb(255, 255, 0);
            this.panelLeft.Dock = DockStyle.Left;
            this.panelLeft.Width = 300;
            this.panelLeft.Name = "panelLeft";


            PictureBox logo = new PictureBox()
            {
                Image = Properties.Resources.logo4,
                Size = new Size(180, 180),
                Location = new Point(50, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            logo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                var gp = new System.Drawing.Drawing2D.GraphicsPath();
                gp.AddEllipse(0, 0, logo.Width - 1, logo.Height - 1);
                logo.Region = new Region(gp);
            };

            panelLeft.Controls.Add(logo);

            int y = 240;

            menuGioiThieu = CreateMenu("Giới Thiệu", Properties.Resources.icon_info, ref y);
            menuTaiKhoan = CreateMenu("Tài Khoản", Properties.Resources.icon_user, ref y);
            menuGiaoAn = CreateMenu("Giáo Án", Properties.Resources.icon_book, ref y);
            this.panelMain.Dock = DockStyle.Fill;
            this.panelMain.BackColor = Color.White;
            this.panelMain.Name = "panelMain";

            this.ClientSize = new Size(1500, 900);

            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelLeft);

            this.Text = "Giáo viên";

            this.ResumeLayout(false);
        }

        private Panel CreateMenu(string text, Image icon, ref int y)
        {
            Panel menu = new Panel()
            {
                Width = 300,
                Location = new Point(0, y),
                BackColor = Color.FromArgb(102, 178, 76),
                Cursor = Cursors.Hand,
                Padding = new Padding(10),
                AutoSize = false,
                Height = 75
            };

            PictureBox pic = new PictureBox()
            {
                Image = icon,
                Size = new Size(40, 40),
                Location = new Point(15, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            Label lbl = new Label()
            {
                Text = text,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Location = new Point(70, 15),
                AutoSize = false,
                MaximumSize = new Size(200, 0),
                Cursor = Cursors.Hand
            };

            lbl.Size = TextRenderer.MeasureText(text, lbl.Font, new Size(200, 0),
                                                TextFormatFlags.WordBreak);

            menu.Height = Math.Max(60, lbl.Height + 30);

            menu.Controls.Add(pic);
            menu.Controls.Add(lbl);
            this.panelLeft.Controls.Add(menu);

            pic.Click += (s, e) => menu_Click(menu, e);
            lbl.Click += (s, e) => menu_Click(menu, e);
            menu.Click += (s, e) => menu_Click(menu, e);

            y += menu.Height + 10;
            return menu;
        }
    }
}
