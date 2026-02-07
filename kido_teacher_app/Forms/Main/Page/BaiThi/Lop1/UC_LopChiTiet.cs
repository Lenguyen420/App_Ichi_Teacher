using kido_teacher_app.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    public partial class UC_LopChiTiet : UserControl
    {
        private readonly Panel parentContainer;
        private readonly ClassDto currentClass;

        private int currentPage = 1;
        private const int pageSize = 10;
        private int totalPages;

        private List<(string title, string subject, string level, string time)> examData;

        public UC_LopChiTiet(Panel parent, ClassDto cls)
        {
            InitializeComponent();
            parentContainer = parent;
            currentClass = cls;



            currentClass.exams = new List<ExamDto>
{
    new ExamDto
    {
        title = "Phiếu bài tập - Fluency Time! 2 - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 6: Lunch time! - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Khó",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 6: Lunch time! - Đề số 02",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 5: This is... - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 4: They're bears! - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Khó",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 4: They're bears! - Đề số 02",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 3: My family - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 2: My body - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Khó",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Unit 1: Hello! - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Dễ",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Review 1 - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Khó",
        type = "Phiếu bài tập",
        time = 15
    },
    new ExamDto
    {
        title = "Phiếu bài tập - Review 1 - Đề số 01",
        subject = "Tiếng Anh 1 / Sách Family and Friends",
        level = "Khó",
        type = "Phiếu bài tập",
        time = 15
    }
};


            LoadFromClass();


            picPrev.Click += (s, e) =>
            {
                if (currentPage > 1)
                    LoadPage(currentPage - 1);
            };

            picNext.Click += (s, e) =>
            {
                if (currentPage < totalPages)
                    LoadPage(currentPage + 1);
            };

        }

        // sự kiện back 
        private void PicBack_Click(object sender, EventArgs e)
        {
            Control parent = this.Parent;
            if (parent == null) return;

            parent.Controls.Clear();

             
            UC_Lop1 uc = new UC_Lop1((Panel)parent, currentClass);
            uc.Dock = DockStyle.Fill;

            parent.Controls.Add(uc);

            
        }


        private void LoadFromClass()
        {
            lblPackageName.Text = currentClass.name;
            lblDescription.Text = currentClass.note;

            examData = currentClass.exams
            .Select(e => (
                e.title,
                $"{e.subject} | Độ khó: {e.level} | Dạng đề: {e.type}",
                e.level,
                $"{e.time} phút"
            ))
            .ToList();

            totalPages = (int)Math.Ceiling(examData.Count / (double)pageSize);
            LoadPage(1);
        }

        private void LoadPage(int page)
        {
            if (page < 1 || page > totalPages) return;

            currentPage = page;
            flowExamList.Controls.Clear();

            // ⭐⭐ ĐẶT Ở ĐÂY ⭐⭐
            if (examData.Count == 0)
            {
                flowExamList.Controls.Add(new Label
                {
                    Text = "Chưa có đề thi nào",
                    Font = new Font("Segoe UI", 11, FontStyle.Italic),
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Margin = new Padding(20)
                });

                lblPage.Text = "0 / 0";
                return;
            }
            // ⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐⭐

            var pageData = examData
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            foreach (var e in pageData)
                flowExamList.Controls.Add(
                    CreateExamRow(e.title, e.subject, e.level, e.time)
                );

            lblPage.Text = $"{currentPage}/{totalPages}";
        }

        //private Panel CreateExamRow(string title, string subject, string level, string time)
        //{
        //    var card = new Panel
        //    {
        //        Height = 110,
        //        Width = flowExamList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
        //        BackColor = Color.White,
        //        BorderStyle = BorderStyle.FixedSingle,
        //        Margin = new Padding(0, 0, 0, 15)
        //    };

        //    var lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 11, FontStyle.Bold), Location = new Point(15, 15) };
        //    var lblSub = new Label { Text = subject, Location = new Point(15, 45), ForeColor = Color.Gray };
        //    var lblLevel = new Label { Text = $"Độ khó: {level}", Location = new Point(15, 70) };

        //    var btn = new Button
        //    {
        //        Text = "Thi thử",
        //        Size = new Size(80, 35),
        //        Location = new Point(card.Width - 100, 35)
        //    };

        //    card.Controls.AddRange(new Control[] { lblTitle, lblSub, lblLevel, btn });
        //    return card;
        //}

        private Panel CreateExamRow(string title, string subject, string level, string time)
        {
            var card = new Panel
            {
                Height = 110,
                Width = flowExamList.ClientSize.Width
             - SystemInformation.VerticalScrollBarWidth,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(0, 0, 0, 15)
            };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Location = new Point(15, 15)
            };

            var lblSub = new Label
            {
                Text = subject,
                Location = new Point(15, 45),
                ForeColor = Color.Gray
            };

            var lblLevel = new Label
            {
                Text = $"Độ khó: {level}",
                Location = new Point(15, 70)
            };

            var btn = new Button
            {
                Text = "Thi thử",
                Size = new Size(80, 35),
                Location = new Point(card.Width - 100, 35),
                Cursor = Cursors.Hand
            };

            btn.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btn.Location = new Point(card.Width - btn.Width - 15, 35);

            card.SizeChanged += (s, e) =>
            {
                btn.Left = card.Width - btn.Width - 15;
            };

            flowExamList.SizeChanged += (s, e) =>
            {
                foreach (Panel p in flowExamList.Controls.OfType<Panel>())
                {
                    p.Width = flowExamList.ClientSize.Width
                              - SystemInformation.VerticalScrollBarWidth;
                }
            };

            // ⭐ CLICK THI THỬ
            btn.Click += (s, e) =>
            {
                parentContainer.Controls.Clear();

                var ucTest = new UC_Lop1_Test(parentContainer, currentClass)
                {
                    Dock = DockStyle.Fill
                };

                parentContainer.Controls.Add(ucTest);
            };

            card.Controls.AddRange(new Control[]
            {
        lblTitle, lblSub, lblLevel, btn
            });

            return card;
        }


    }
}
