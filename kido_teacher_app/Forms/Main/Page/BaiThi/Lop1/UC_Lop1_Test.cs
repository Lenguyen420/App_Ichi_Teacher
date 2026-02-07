using kido_teacher_app.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.BaiThi.Lop1
{
    public partial class UC_Lop1_Test : UserControl
    {
        private readonly Panel parentContainer;
        private readonly ClassDto currentClass;
        // ===== STATE =====
        private Dictionary<int, Button> questionIndexButtons = new();

        public UC_Lop1_Test(Panel parentContainer, ClassDto currentClass)
        {
            InitializeComponent();
            this.parentContainer = parentContainer;
            this.currentClass = currentClass;
        }


        private void BtnThoat_Click(object sender, EventArgs e)
        {
            parentContainer.Controls.Clear();

            var ucChiTiet = new UC_LopChiTiet(parentContainer, currentClass);
            ucChiTiet.Dock = DockStyle.Fill;

            parentContainer.Controls.Add(ucChiTiet);
        }


        // ===== REGISTER INDEX BUTTON =====
        private void RegisterQuestionIndexButton(int number, Button btn)
        {
            questionIndexButtons[number] = btn;
        }

        // ===== WHEN ANSWER SELECTED =====
        private void OnAnswerSelected(int questionIndex)
        {
            if (!questionIndexButtons.ContainsKey(questionIndex))
                return;

            var btn = questionIndexButtons[questionIndex];

            btn.UseVisualStyleBackColor = false;
            btn.BackColor = Color.DodgerBlue;
            btn.ForeColor = Color.White;
            btn.FlatAppearance.BorderColor = Color.DodgerBlue;
        }
    }
}
