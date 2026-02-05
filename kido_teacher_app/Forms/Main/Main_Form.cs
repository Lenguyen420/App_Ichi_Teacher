using kido_teacher_app.Forms.Main.Page;
//using kido_teacher_app.Forms.Main.Page.QuanLyTaiKhoan;
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

            RegisterMenuEvents();   //Gán sự kiện menu click

            // Load mặc định
            SelectMenu(menuGioiThieu);
            ShowControl(new UC_GioiThieu());
        }

        private void RegisterMenuEvents()
        {
            menuGioiThieu.Click += menu_Click;
            menuTaiKhoan.Click += menu_Click;
            //menuThemMoi.Click += menu_Click;
            menuGiaoAn.Click += menu_Click;
            //menuQLTaiKhoan.Click += menu_Click;
            //menuQLBaiGiang.Click += menu_Click;
        }

        private async void menu_Click(object sender, EventArgs e)
        {
            Panel clickedMenu = sender as Panel;

            // Nếu click vào Label hoặc PictureBox → lấy panel cha
            if (clickedMenu == null)
                clickedMenu = (sender as Control).Parent as Panel;

            if (clickedMenu == menuGioiThieu)
            {
                SelectMenu(menuGioiThieu);
                ShowControl(new UC_GioiThieu());
            }
            else if (clickedMenu == menuTaiKhoan)
            {
                //SelectMenu(menuTaiKhoan);
                //ShowControl(new UC_TaiKhoan());
                SelectMenu(menuTaiKhoan);
                await LoadTaiKhoanAsync();
            }
            //else if (clickedMenu == menuThemMoi)
            //{
            //    SelectMenu(menuThemMoi);
            //    ShowControl(new UC_ThemMoiBaiGiang());
            //}
            else if (clickedMenu == menuGiaoAn)
            {
                //SelectMenu(menuGiaoAn);
                //ShowControl(new UC_GiaoAn());
                SelectMenu(menuGiaoAn);

                var courseId = await GetFirstCourseIdAsync();

                ShowControl(new UC_GiaoAn(courseId));
            }
            //else if (clickedMenu == menuQLTaiKhoan)
            //{
            //    SelectMenu(menuQLTaiKhoan);
            //    ShowControl(new UC_QuanLyTaiKhoan_Main());
            //}
            //else if (clickedMenu == menuQLBaiGiang)
            //{
            //    SelectMenu(menuQLBaiGiang);
            //    ShowControl(new UC_TaiKhoan_QLBG());
            //}
            //else if (clickedMenu == menuThongBao)
            //{
            //    SelectMenu(menuThongBao);
            //    ShowControl(new UC_ThongBao());
            //}
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
            ReplaceMainControl(uc);
        }

        public void LoadUserControl(UserControl uc)
        {
            ReplaceMainControl(uc);
        }

        private async Task LoadTaiKhoanAsync()
        {
            var user = await UserService.GetByIdAsync(AuthSession.UserId);

            var uc = new UC_TaiKhoan();
            uc.LoadUser(user);

            ReplaceMainControl(uc);
        }

        private void ReplaceMainControl(UserControl uc)
        {
            panelMain.SuspendLayout();

            // ControlCollection.Clear() không đảm bảo giải phóng object cũ.
            // Dispose explicit để tránh giữ timer/image trong RAM.
            for (int i = panelMain.Controls.Count - 1; i >= 0; i--)
            {
                panelMain.Controls[i].Dispose();
            }

            panelMain.Controls.Clear();
            uc.Dock = DockStyle.Fill;
            panelMain.Controls.Add(uc);

            panelMain.ResumeLayout();
        }

    }
}
