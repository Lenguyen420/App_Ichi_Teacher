using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using kido_teacher_app.Model;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    public partial class UC_Lop_KetQua : UserControl
    {
        private readonly Panel parentContainer;
        private   List<QuestionAnswerDto> results;
        private readonly ClassDto currentClass;

        public UC_Lop_KetQua(
    Panel parentContainer,
    List<QuestionAnswerDto> results,
    ClassDto currentClass)
        {
            InitializeComponent();
             flowKetQua.FlowDirection = FlowDirection.TopDown;
            flowKetQua.WrapContents = false;
            flowKetQua.AutoScroll = true;

            this.parentContainer = parentContainer;
            this.results = results ?? new List<QuestionAnswerDto>();
            this.currentClass = currentClass;

            flowKetQua.SizeChanged += (s, e) => ResizeCards();

            if (this.results.Count == 0)
                LoadFakeData();   // 👈 tạo DATA MẪU

            LoadKetQua();         // 👈 CHỈ GỌI 1 HÀM
            LoadThongKe();
            LoadDeThiMau();
        }
       
       
        private void LoadDeThiMau()
        {
            panelDeThi.Controls.Clear();

            for (int i = 1; i <= 3; i++)
            {
                var btn = new Button
                {
                    Text = $"Đề số {i:00}",
                    Width = panelDeThi.Width - 25,
                    Height = 45,
                    BackColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(5)
                };

                btn.FlatAppearance.BorderColor = Color.LightGray;

                panelDeThi.Controls.Add(btn);
            }
        }

        // ================= CENTER =================
        private void LoadKetQua()
        {
            flowKetQua.SuspendLayout();
            flowKetQua.Controls.Clear();
            flowKetQua.AutoScroll = true;

            int index = 1;

            foreach (var q in results)
            {
                var card = CreateResultCard(index, q);
                flowKetQua.Controls.Add(card);
                index++;
            }

            flowKetQua.ResumeLayout();
        }

        private Panel CreateResultCard(int index, QuestionAnswerDto q)
        {
            var card = new Panel
            {
                Width = flowKetQua.ClientSize.Width - 25,
                AutoSize = true,
                BackColor = Color.White,
                Padding = new Padding(20),
                Margin = new Padding(10),
                BorderStyle = BorderStyle.FixedSingle
            };

            int top = 10;

            // ================= CÂU HỎI =================
            var lblQuestion = new Label
            {
                Text = $"Câu {index}: {q.QuestionText ?? "Không có nội dung"}",
                AutoSize = false,
                Width = card.Width - 40,
                Height = 40,
                Location = new Point(10, top),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };

            card.Controls.Add(lblQuestion);
            top += 50;

            // ================= ĐÁP ÁN =================
            if (q.Answers != null && q.Answers.Count > 0)
            {
                foreach (var ans in q.Answers)
                {
                    var lblAnswer = new Label
                    {
                        Text = $"{ans.Key}. {ans.Value}",
                        Width = card.Width - 40,
                        Height = 35,
                        Location = new Point(10, top),
                        Font = new Font("Segoe UI", 10.5F),
                        BorderStyle = BorderStyle.FixedSingle,
                        Padding = new Padding(8),
                        BackColor = Color.White
                    };

                    bool isCorrect = ans.Key == q.CorrectAnswer;
                    bool isSelected = ans.Key == q.SelectedAnswer;

                    // ===== Highlight ĐÚNG =====
                    if (isCorrect)
                    {
                        lblAnswer.BackColor = Color.FromArgb(220, 245, 220);
                        lblAnswer.ForeColor = Color.DarkGreen;
                    }

                    // ===== Highlight SAI =====
                    if (isSelected && q.SelectedAnswer != q.CorrectAnswer)
                    {
                        lblAnswer.BackColor = Color.FromArgb(255, 220, 220);
                        lblAnswer.ForeColor = Color.DarkRed;
                    }

                    card.Controls.Add(lblAnswer);
                    top += 45;
                }
            }

            // ================= KẾT LUẬN =================
            var lblResult = new Label
            {
                Width = card.Width - 40,
                Height = 30,
                Location = new Point(10, top + 5),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };

            if (string.IsNullOrEmpty(q.SelectedAnswer))
            {
                lblResult.Text = "⚪ Chưa làm";
                lblResult.ForeColor = Color.Gray;
            }
            else if (q.SelectedAnswer == q.CorrectAnswer)
            {
                lblResult.Text = "✅ Trả lời đúng";
                lblResult.ForeColor = Color.Green;
            }
            else
            {
                lblResult.Text = $"❌ Sai | Đáp án đúng: {q.CorrectAnswer}";
                lblResult.ForeColor = Color.Red;
            }

            card.Controls.Add(lblResult);

            return card;
        }

        private void ResizeCards()
        {
            foreach (Control c in flowKetQua.Controls)
            {
                c.Width = flowKetQua.ClientSize.Width - 20;
            }
        }


        // ================= HEADER STATS =================
        private void LoadThongKe()
        {
            int tong = results.Count;
            int dung = results.Count(x => x.SelectedAnswer == x.CorrectAnswer && x.SelectedAnswer != null);
            int sai = results.Count(x => x.SelectedAnswer != null && x.SelectedAnswer != x.CorrectAnswer);
            int chuaLam = tong - dung - sai;

            lblTong.Text = $"Tất cả ({tong})";
            lblDung.Text = $"Đúng ({dung})";
            lblSai.Text = $"Sai ({sai})";
            lblChuaLam.Text = $"Chưa làm ({chuaLam})";
            lblTuLuan.Text = "Tự luận (0)";

            lblDiem.Text = (tong == 0 ? 0 : dung * 10.0 / tong).ToString("0.#");
        }

        // ================= LEFT =================
        

        // ================= EVENTS =================
        private void btnDong_Click(object sender, EventArgs e)
        {
            parentContainer.Controls.Clear();
            parentContainer.Controls.Add(new UC_LopChiTiet(parentContainer, currentClass)
            {
                Dock = DockStyle.Fill
            });
        }

        private void btnLamLai_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Làm lại đề thi");
        }

         

        private void LoadFakeData()
        {
            results = new List<QuestionAnswerDto>
    {
        new QuestionAnswerDto
        {
            QuestionText = "1 + 1 bằng mấy?",
            CorrectAnswer = "B",
            SelectedAnswer = "A",
            Answers = new Dictionary<string, string>
            {
                {"A","1"},
                {"B","2"},
                {"C","3"},
                {"D","4"}
            }
        },
        new QuestionAnswerDto
        {
            QuestionText = "Màu của lá cây là gì?",
            CorrectAnswer = "C",
            SelectedAnswer = "C",
            Answers = new Dictionary<string, string>
            {
                {"A","Đỏ"},
                {"B","Vàng"},
                {"C","Xanh"},
                {"D","Tím"}
            }
        },
        new QuestionAnswerDto
        {
            QuestionText = "Con mèo kêu như thế nào?",
            CorrectAnswer = "A",
            SelectedAnswer = null,
            Answers = new Dictionary<string, string>
            {
                {"A","Meo meo"},
                {"B","Gâu gâu"},
                {"C","Ò ó o"},
                {"D","Quạc quạc"}
            }
        }
    };
        }


    }
}
