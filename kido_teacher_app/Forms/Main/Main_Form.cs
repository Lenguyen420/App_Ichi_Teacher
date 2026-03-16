using kido_teacher_app.Forms.Main.Page;
//using kido_teacher_app.Forms.Main.Page.QuanLyTaiKhoan;
using kido_teacher_app.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app
{
    public partial class Main_Form : Form
    {
        private const int WM_SETREDRAW = 0x000B;

        private Panel? selectedMenu;
        private UserControl? currentControl;

        private UC_GioiThieu? gioiThieuControl;
        private UC_TaiKhoan? taiKhoanControl;
        private UC_GiaoAn? giaoAnControl;

        private readonly HashSet<UserControl> persistentControls = new HashSet<UserControl>();

        public Main_Form()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            SetDoubleBuffered(panelMain);

            SelectMenu(menuGioiThieu);
            gioiThieuControl = new UC_GioiThieu();
            ShowPersistentControl(gioiThieuControl);
        }

        private async void menu_Click(object sender, EventArgs e)
        {
            Panel? clickedMenu = sender as Panel;

            // Nếu click vào Label hoặc PictureBox -> lấy panel cha
            if (clickedMenu == null)
                clickedMenu = (sender as Control)?.Parent as Panel;

            if (clickedMenu == null)
                return;

            if (clickedMenu == menuGioiThieu)
            {
                SelectMenu(menuGioiThieu);
                gioiThieuControl ??= new UC_GioiThieu();
                ShowPersistentControl(gioiThieuControl);
            }
            else if (clickedMenu == menuTaiKhoan)
            {
                SelectMenu(menuTaiKhoan);
                await LoadTaiKhoanAsync();
            }
            else if (clickedMenu == menuGiaoAn)
            {
                SelectMenu(menuGiaoAn);
                await LoadGiaoAnAsync();
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
            ReplaceMainControl(uc, keepAlive: false);
        }

        public void LoadUserControl(UserControl uc)
        {
            ReplaceMainControl(uc, keepAlive: false);
        }

        private void ShowPersistentControl(UserControl uc)
        {
            ReplaceMainControl(uc, keepAlive: true);
        }

        private async Task LoadTaiKhoanAsync()
        {
            var user = await UserService.GetByIdAsync(AuthSession.UserId);

            taiKhoanControl ??= new UC_TaiKhoan();
            taiKhoanControl.LoadUser(user);
            ShowPersistentControl(taiKhoanControl);
        }

        private async Task LoadGiaoAnAsync()
        {
            if (giaoAnControl == null)
            {
                var courseId = await GetFirstCourseIdAsync();
                giaoAnControl = new UC_GiaoAn(courseId);
            }

            ShowPersistentControl(giaoAnControl);
        }

        private void ReplaceMainControl(UserControl uc, bool keepAlive)
        {
            if (ReferenceEquals(currentControl, uc))
                return;

            var previousControl = currentControl;
            var shouldDisposePrevious =
                previousControl != null && !persistentControls.Contains(previousControl);

            SetRedraw(panelMain, false);
            panelMain.SuspendLayout();

            try
            {
                if (previousControl != null)
                {
                    panelMain.Controls.Remove(previousControl);
                }

                if (keepAlive)
                    persistentControls.Add(uc);

                uc.Dock = DockStyle.Fill;
                panelMain.Controls.Add(uc);
                currentControl = uc;
            }
            finally
            {
                panelMain.ResumeLayout(true);
                SetRedraw(panelMain, true);
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() => panelMain.Invalidate(true)));
                }
                else
                {
                    panelMain.Invalidate(true);
                }
            }

            if (shouldDisposePrevious)
            {
                if (IsHandleCreated)
                {
                    BeginInvoke(new Action(() => previousControl?.Dispose()));
                }
                else
                {
                    previousControl?.Dispose();
                }
            }
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
                )
                ?.SetValue(control, true, null);
        }

        private static void SetRedraw(Control control, bool enabled)
        {
            if (!control.IsHandleCreated)
                return;

            SendMessage(control.Handle, WM_SETREDRAW, enabled ? 1 : 0, 0);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
    }
}
