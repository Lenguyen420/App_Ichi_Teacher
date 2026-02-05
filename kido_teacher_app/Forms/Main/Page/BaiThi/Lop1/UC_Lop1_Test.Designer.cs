namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    partial class UC_Lop1_Test
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Button btnThoat;

        private System.Windows.Forms.Panel panelBody;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelRight;

        private System.Windows.Forms.FlowLayoutPanel flowQuestions;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.Button btnSubmit;
        private System.Windows.Forms.FlowLayoutPanel flowQuestionIndex;


        private System.Windows.Forms.Timer examTimer;
        private TimeSpan remainingTime = TimeSpan.FromMinutes(25);

        private Panel panelTime;
        private Panel panelSubmit;
        private Panel panelIndex;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Designer

        private void InitializeComponent()
        {
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.btnThoat = new System.Windows.Forms.Button();

            this.panelBody = new System.Windows.Forms.Panel();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();

            this.flowQuestions = new System.Windows.Forms.FlowLayoutPanel();
            this.lblTime = new System.Windows.Forms.Label();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.flowQuestionIndex = new System.Windows.Forms.FlowLayoutPanel();

            // ================= HEADER =================
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Height = 60;
            this.panelHeader.BackColor = System.Drawing.Color.WhiteSmoke;

            this.lblTitle.Text = "Tên Bài Thi";
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);

            this.btnThoat.Text = "Thoát";
            this.btnThoat.Width = 80;
            this.btnThoat.Height = 30;
            this.btnThoat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnThoat.Location = new System.Drawing.Point(900, 15);

            this.panelHeader.Controls.Add(this.lblTitle);
            this.panelHeader.Controls.Add(this.btnThoat);

            // ================= BODY =================
            this.panelBody.Dock = System.Windows.Forms.DockStyle.Fill;

            // ================= LEFT =================
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Padding = new System.Windows.Forms.Padding(15);
            this.panelLeft.AutoScroll = true;

            this.flowQuestions.Dock = System.Windows.Forms.DockStyle.Top;
            this.flowQuestions.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowQuestions.WrapContents = false;
            this.flowQuestions.AutoSize = true;

            // thêm 10 câu
            for (int i = 1; i <= 10; i++)
                this.flowQuestions.Controls.Add(CreateQuestion(i));

            this.panelLeft.Controls.Add(this.flowQuestions);

            // ================= RIGHT =================
            this.panelRight.Dock = DockStyle.Right;
            this.panelRight.Width = 260;
            this.panelRight.BackColor = Color.WhiteSmoke;
            this.panelRight.Padding = new Padding(10);

            // ===== PANEL TIME =====
            panelTime = new Panel();
            panelTime.Dock = DockStyle.Top;
            panelTime.Height = 70;

            lblTime.Text = $"Thời gian làm bài\n00:00";
            lblTime.Dock = DockStyle.Fill;
            lblTime.TextAlign = ContentAlignment.MiddleCenter;
            lblTime.Font = new Font("Segoe UI", 11F, FontStyle.Bold);

            panelTime.Controls.Add(lblTime);

            // ===== PANEL SUBMIT =====
            panelSubmit = new Panel();
            panelSubmit.Dock = DockStyle.Top;
            panelSubmit.Height = 70;
            panelSubmit.Padding = new Padding(10, 5, 10, 5);

            btnSubmit.Text = "NỘP BÀI";
            btnSubmit.Dock = DockStyle.Fill;
            btnSubmit.Height = 40;

            btnSubmit.FlatStyle = FlatStyle.Flat;
            btnSubmit.FlatAppearance.BorderColor = Color.DodgerBlue;
            btnSubmit.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnSubmit.ForeColor = Color.DodgerBlue;
            btnSubmit.BackColor = Color.White;

            btnSubmit.MouseEnter += (s, e) =>
            {
                btnSubmit.BackColor = Color.DodgerBlue;
                btnSubmit.ForeColor = Color.White;
            };
            btnSubmit.MouseLeave += (s, e) =>
            {
                btnSubmit.BackColor = Color.White;
                btnSubmit.ForeColor = Color.DodgerBlue;
            };

            panelSubmit.Controls.Add(btnSubmit);

            // ===== PANEL INDEX =====
            panelIndex = new Panel();
            panelIndex.Dock = DockStyle.Fill;
            panelIndex.Padding = new Padding(10, 5, 10, 10);

            flowQuestionIndex.Dock = DockStyle.Top;
            flowQuestionIndex.AutoSize = true;
            flowQuestionIndex.WrapContents = true;

            for (int i = 101; i <= 110; i++)
            {
                var btn = new Button();
                btn.Text = i.ToString();
                btn.Width = 45;
                btn.Height = 35;
                btn.Margin = new Padding(5);
                flowQuestionIndex.Controls.Add(btn);
            }

            panelIndex.Controls.Add(flowQuestionIndex);

            // ===== ADD RIGHT =====
            this.panelRight.Controls.Add(panelIndex);
            this.panelRight.Controls.Add(panelSubmit);
            this.panelRight.Controls.Add(panelTime);


            // ===== ADD BODY =====
            this.panelBody.Controls.Add(this.panelLeft);
            this.panelBody.Controls.Add(this.panelRight);

            // ===== ADD ROOT =====
            this.Controls.Add(this.panelBody);
            this.Controls.Add(this.panelHeader);

            // ===== TIMER =====
            examTimer = new System.Windows.Forms.Timer();
            examTimer.Interval = 1000;
            examTimer.Tick += ExamTimer_Tick;
            examTimer.Start();

            // ===== SIZE =====
            this.Size = new Size(1000, 650);


        }

        #endregion

        // ================= QUESTION =================
        private System.Windows.Forms.GroupBox CreateQuestion(int index)
        {
            var gb = new System.Windows.Forms.GroupBox();
            gb.Text = $"{index}. Nội dung câu hỏi mẫu";
            gb.Width = 780;
            gb.Height = 180;
            gb.Font = new Font("Segoe UI", 11F);
            gb.Padding = new Padding(10);

            // ===== TABLE LAYOUT =====
            var table = new TableLayoutPanel();
            table.Dock = DockStyle.Fill;
            table.ColumnCount = 2;
            table.RowCount = 2;

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            string[] answers =
            {
        "A. Đáp án A",
        "B. Đáp án B",
        "C. Đáp án C",
        "D. Đáp án D"
    };

            for (int i = 0; i < 4; i++)
            {
                var rb = new RadioButton();
                rb.Text = answers[i];
                rb.Font = new Font("Segoe UI", 10.5F);
                rb.Dock = DockStyle.Fill;
                rb.Padding = new Padding(5);
                rb.AutoSize = true;

                table.Controls.Add(rb, i % 2, i / 2);
            }

            gb.Controls.Add(table);
            return gb;
        }
        private void ExamTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds <= 0)
            {
                examTimer.Stop();
                MessageBox.Show("Hết giờ làm bài!");
                return;
            }

            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            lblTime.Text = $"Thời gian làm bài:\n{remainingTime:mm\\:ss}";
        }

    }
}
