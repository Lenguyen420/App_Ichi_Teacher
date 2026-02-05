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

           

            currentClass.exams ??= new List<ExamDto>
            {
                new ExamDto
                {
                    title = "Đề kiểm tra KNS số 1",
                    subject = "Kỹ năng sống",
                    level = "Dễ",
                    time = 30
                },
                new ExamDto
                {
                    title = "Đề kiểm tra STEM số 1",
                    subject = "STEM",
                    level = "Trung bình",
                    time = 45
                },
                new ExamDto
                {
                    title = "Đề tổng hợp KNS + STEM",
                    subject = "KNS + STEM",
                    level = "Khó",
                    time = 60
                }
            };

            LoadFromClass();


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

            examData = currentClass.exams?
                .Select(e => (e.title, e.subject, e.level, $"{e.time} phút"))
                .ToList()
                ?? new();

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
               // Width = flowExamList.ClientSize.Width - SystemInformation.VerticalScrollBarWidth,
                BackColor = Color.White,
                Dock = DockStyle.Top,
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

                var ucTest = new UC_Lop1_Test
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
