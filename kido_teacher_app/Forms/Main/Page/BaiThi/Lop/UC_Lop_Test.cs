using kido_teacher_app.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    public partial class UC_Lop_Test : UserControl
    {
        // ====== INDEX BUTTON MAP ======
        private Dictionary<int, Button> questionIndexButtons
            = new Dictionary<int, Button>();

        // ====== QUESTION RESULT MAP ======
        private Dictionary<int, QuestionAnswerDto> questionResults
            = new Dictionary<int, QuestionAnswerDto>();

        private Panel parentContainer;
        private ClassDto currentClass;

        public UC_Lop_Test()
        {
            InitializeComponent();
        }

        public UC_Lop_Test(Panel parentContainer, ClassDto currentClass)
        {
            InitializeComponent();

            this.parentContainer = parentContainer;
            this.currentClass = currentClass;
        }

        // =====================================================
        // ================= ANSWER SELECT =====================
        // =====================================================

        /// <summary>
        /// index: số câu hỏi (1,2,3...)
        /// answerIndex: 0=A, 1=B, 2=C, 3=D
        /// </summary>
        private void OnAnswerSelected(int index, int answerIndex)
        {
            if (!questionResults.ContainsKey(index))
            {
                questionResults[index] = new QuestionAnswerDto
                {
                    QuestionIndex = index,

                    // TODO: sau này lấy từ data thật
                    CorrectAnswer = "A"
                };
            }

            // Convert int -> A/B/C/D
            string selectedAnswer = ((char)('A' + answerIndex)).ToString();

            questionResults[index].SelectedAnswer = selectedAnswer;
            questionResults[index].IsMarked = true;

            UpdateQuestionIndexUI(index);
        }

        // =====================================================
        // ================= INDEX REGISTER ====================
        // =====================================================

        private void RegisterQuestionIndexButton(int index, Button btn)
        {
            if (!questionIndexButtons.ContainsKey(index))
            {
                questionIndexButtons[index] = btn;
            }
        }

        private void UpdateQuestionIndexUI(int index)
        {
            if (questionIndexButtons.TryGetValue(index, out var btn))
            {
                btn.BackColor = Color.DodgerBlue;
                btn.ForeColor = Color.White;
            }
        }

        // =====================================================
        // ===================== TIMER =========================
        // =====================================================

        private void ExamTimer_Tick(object sender, EventArgs e)
        {
            if (remainingTime.TotalSeconds <= 0)
            {
                examTimer.Stop();
                MessageBox.Show("Hết giờ làm bài!");

                SubmitExam();
                return;
            }

            remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds(1));
            lblTime.Text = $"Thời gian làm bài:\n{remainingTime:mm\\:ss}";
        }

        // =====================================================
        // ================== SUBMIT LOGIC =====================
        // =====================================================

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            SubmitExam();
        }

        private void SubmitExam()
        {
            examTimer.Stop();

            // Nếu câu nào chưa làm → tạo record rỗng
            for (int i = 1; i <= 10; i++)
            {
                if (!questionResults.ContainsKey(i))
                {
                    questionResults[i] = new QuestionAnswerDto
                    {
                        QuestionIndex = i,
                        SelectedAnswer = null,
                        CorrectAnswer = "A",
                        IsMarked = false
                    };
                }
            }

            var resultList = questionResults.Values.ToList();

            parentContainer.Controls.Clear();
            parentContainer.Controls.Add(
                new UC_Lop_KetQua(parentContainer, resultList, currentClass)
                {
                    Dock = DockStyle.Fill
                }
            );
        }

        // =====================================================
        // ================== BUTTON THOÁT =====================
        // =====================================================

        private void BtnThoat_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc muốn thoát bài thi?",
                "Xác nhận",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                examTimer.Stop();

                parentContainer.Controls.Clear();
                parentContainer.Controls.Add(
                    new UC_LopChiTiet(parentContainer, currentClass)
                    {
                        Dock = DockStyle.Fill
                    }
                );
            }
        }
    }
}
