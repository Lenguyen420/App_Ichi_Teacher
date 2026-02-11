using kido_teacher_app.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    public partial class UC_Lop : UserControl
    {
        private readonly Panel parentContainer;
        private readonly ClassDto currentClass;

        public UC_Lop(Panel parent, ClassDto cls)
        {
            InitializeComponent();
            parentContainer = parent;
            currentClass = cls;

            LoadData();
        }


        
        // ================= LOAD DATA =================
        private void LoadData()
        {
            flowContent.Controls.Clear();

            foreach (var item in GetSampleData())
            {
                flowContent.Controls.Add(CreateCard(item));
            }
        }

        // ================= CARD =================
        private Panel CreateCard(BaiThiItem item)
        {
            var card = new Panel
            {
                Width = 650,
                Height = 150,
                BackColor = Color.White,
                Margin = new Padding(12),
                BorderStyle = BorderStyle.FixedSingle,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Image = item.Image,
                Size = new Size(120, 120),
                Location = new Point(12, 15),
                SizeMode = PictureBoxSizeMode.Zoom,
                Cursor = Cursors.Hand
            };

            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 12),
                Location = new Point(150, 20),
                Size = new Size(450, 60),
                AutoEllipsis = true,
                Cursor = Cursors.Hand
            };

            var lblCount = new Label
            {
                Text = $"{item.Count} đề",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = new Size(70, 30),
                Location = new Point(150, 95),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(245, 245, 245),
                Cursor = Cursors.Hand
            };

            // ⭐ HÀNH ĐỘNG KHI CLICK CARD
            void OpenChiTiet(object sender, EventArgs e)
            {
                // GÁN ĐỀ THI MẪU
                currentClass.exams = new List<ExamDto>
        {
            new ExamDto { title = "Đề số 1 - Alphabet", subject = "Tiếng Anh", level = "Dễ", time = 30 },
            new ExamDto { title = "Đề số 2 - Numbers",  subject = "Tiếng Anh", level = "Trung bình", time = 40 },
            new ExamDto { title = "Đề số 3 - Colors",   subject = "Tiếng Anh", level = "Khó", time = 45 }
        };

                parentContainer.Controls.Clear();

                var ct = new UC_LopChiTiet(parentContainer, currentClass)
                {
                    Dock = DockStyle.Fill
                };

                parentContainer.Controls.Add(ct);
            }

            // ⭐ GÁN CLICK CHO TẤT CẢ
            card.Click += OpenChiTiet;
            pic.Click += OpenChiTiet;
            lblTitle.Click += OpenChiTiet;
            lblCount.Click += OpenChiTiet;

            card.Controls.AddRange(new Control[] { pic, lblTitle, lblCount });

            return card;
        }

        // ================= DATA MẪU =================
        private List<BaiThiItem> GetSampleData()
        {
            return new()
            {
                new() { Title = "Đề Tiếng Anh lớp 1", Count = 23, Image = Properties.Resources.classdefault },
                new() { Title = "Đề Tiếng Việt lớp 1", Count = 18, Image = Properties.Resources.classdefault },
                new() { Title = "Đề Toán lớp 1", Count = 15, Image = Properties.Resources.classdefault }
            };
        }
    }

    class BaiThiItem
    {
        public string Title { get; set; }
        public int Count { get; set; }
        public Image Image { get; set; }
    }
}
