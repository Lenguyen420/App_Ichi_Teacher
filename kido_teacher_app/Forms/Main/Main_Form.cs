using kido_teacher_app.Forms.Main.Page;
using kido_teacher_app.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app
{
    public partial class Main_Form : Form
    {
        private Panel selectedMenu = null;


        public Main_Form()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

            RegisterMenuEvents();

            SelectMenu(menuGioiThieu);
            ShowControl(new UC_GioiThieu());
        }

        private void RegisterMenuEvents()
        {
            menuGioiThieu.Click += menu_Click;
            menuTaiKhoan.Click += menu_Click;
            menuGiaoAn.Click += menu_Click;
        }

        private async void menu_Click(object sender, EventArgs e)
        {
            Panel clickedMenu = sender as Panel;

            if (clickedMenu == null)
                clickedMenu = (sender as Control).Parent as Panel;

            if (clickedMenu == menuGioiThieu)
            {
                SelectMenu(menuGioiThieu);
                ShowControl(new UC_GioiThieu());
            }
            else if (clickedMenu == menuTaiKhoan)
            {
                SelectMenu(menuTaiKhoan);
                await LoadTaiKhoanAsync();
            }
            else if (clickedMenu == menuGiaoAn)
            {
                SelectMenu(menuGiaoAn);

                var courseId = await GetFirstCourseIdAsync();

                ShowControl(new UC_GiaoAn(courseId));
            }
        }

        private async Task<string> GetFirstCourseIdAsync()
        {
            var list = await CourseService.GetAllAsync();
            return list.FirstOrDefault()?.id ?? "";
        }

        private void SelectMenu(Panel menu)
        {
            if (selectedMenu != null)
                selectedMenu.BackColor = Color.FromArgb(102, 178, 76);

            selectedMenu = menu;
            selectedMenu.BackColor = Color.FromArgb(144, 238, 144);
        }

        public void ShowControl(UserControl uc)
        {
            panelMain.SuspendLayout();

            panelMain.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelMain.Controls.Add(uc);

            panelMain.ResumeLayout();
        }

        public void LoadUserControl(UserControl uc)
        {
            panelMain.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelMain.Controls.Add(uc);
        }

        private async Task LoadTaiKhoanAsync()
        {
            var user = await UserService.GetByIdAsync(AuthSession.UserId);

            var uc = new UC_TaiKhoan();
            uc.LoadUser(user);

            panelMain.Controls.Clear();
            panelMain.Controls.Add(uc);
        }

    }
}
