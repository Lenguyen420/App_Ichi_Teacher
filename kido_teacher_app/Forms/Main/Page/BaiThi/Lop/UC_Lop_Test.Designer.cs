namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    partial class UC_Lop_Test
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
        // lưu button index theo số câu
        // private Dictionary<int, Button> questionIndexButtons = new Dictionary<int, Button>();

      

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
            this.panelHeader.Dock = DockStyle.Top;
            this.panelHeader.Height = 60;
            this.panelHeader.BackColor = Color.WhiteSmoke;

            // ===== TABLE HEADER =====
            var headerLayout = new TableLayoutPanel();
            headerLayout.Dock = DockStyle.Fill;
            headerLayout.ColumnCount = 3;
            headerLayout.RowCount = 1;
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // trống
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F)); // title
            headerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F)); // button

            // ===== TITLE =====
            this.lblTitle.Text = "Tên Bài Thi";
            this.lblTitle.Dock = DockStyle.Fill;
            this.lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            this.lblTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);

            // ===== BUTTON THOÁT =====
            this.btnThoat.Text = "Thoát";
            this.btnThoat.Width = 80;
            this.btnThoat.Height = 32;
            this.btnThoat.Anchor = AnchorStyles.Right;
            this.btnThoat.Margin = new Padding(0, 14, 15, 14);
            btnThoat.Click += BtnThoat_Click;
            // ===== ADD =====
            headerLayout.Controls.Add(new Label(), 0, 0); // cột trống
            headerLayout.Controls.Add(lblTitle, 1, 0);
            headerLayout.Controls.Add(btnThoat, 2, 0);

            this.panelHeader.Controls.Clear();
            this.panelHeader.Controls.Add(headerLayout);


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
            btnSubmit.Click += BtnSubmit_Click;



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

            for (int i = 1; i <= 10; i++)
            {
                var btn = new Button();
                btn.Text = i.ToString();
                btn.Width = 45;
                btn.Height = 35;
                btn.Margin = new Padding(5);

                btn.BackColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderColor = Color.Gray;
                btn.UseVisualStyleBackColor = false;
                flowQuestionIndex.Controls.Add(btn);

                // ⭐ GỌI LOGIC – KHÔNG LƯU Ở DESIGNER
                RegisterQuestionIndexButton(i, btn);
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
        private Panel CreateQuestion(int index)
        {
            var card = new Panel();
            card.Width = 780;
            card.Height = 200;
            card.BackColor = Color.White;
            card.Margin = new Padding(10);
            card.Padding = new Padding(15);
            //card.BorderStyle = BorderStyle.FixedSingle;

            // ===== LABEL CÂU HỎI =====
            var lblQuestion = new Label();
            lblQuestion.Text = $"Câu {index}: Nội dung câu hỏi mẫu";
            lblQuestion.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblQuestion.AutoSize = false;
            lblQuestion.Width = 740;
            lblQuestion.Height = 35;
            lblQuestion.Location = new Point(10, 10);

            card.Controls.Add(lblQuestion);

            string[] answers =
            {
        "A. Đáp án A",
        "B. Đáp án B",
        "C. Đáp án C",
        "D. Đáp án D"
    };

            int correctIndex = 1; // ví dụ đáp án đúng là B
            int top = 55;

            for (int i = 0; i < answers.Length; i++)
            {
                int answerIndex = i;

                var rb = new RadioButton();
                rb.Text = answers[i];
                rb.Width = 730;
                rb.Height = 30;
                rb.Location = new Point(20, top);
                rb.Font = new Font("Segoe UI", 10.5F);
                rb.Tag = answerIndex; // lưu index
                rb.BackColor = Color.White;

                rb.CheckedChanged += (s, e) =>
                {
                    if (rb.Checked)
                    {
                        OnAnswerSelected(index, answerIndex);
                    }
                };

                card.Controls.Add(rb);
                top += 35;
            }

            // Lưu đáp án đúng vào Tag của card
            card.Tag = correctIndex;

            return card;
        }

    }
}
