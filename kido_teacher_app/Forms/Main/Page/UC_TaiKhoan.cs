using kido_teacher_app.Model;
using kido_teacher_app.Services;
using System;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_TaiKhoan : UserControl
    {
        public UC_TaiKhoan()
        {
            InitializeComponent();
        }

        //  HÀM NHẬN DỮ LIỆU USER VÀ ĐỔ LÊN GIAO DIỆN
        public void LoadUser(UserDto u)
        {
            if (u == null) return;

            // --- THÔNG TIN TÀI KHOẢN ---
            txtMaTK.Text = u.id ?? "";
            txtTaiKhoan.Text = u.userName ?? "";
            txtTen.Text = u.fullName ?? "";
            txtEmail.Text = u.email ?? "";

            // --- LOẠI / NGÀY ---
            txtLoaiTK.Text = u.userType ?? "";
            txtNgayKichHoat.Text = u.activatedDate ?? "";
            txtNgayHetHan.Text = u.expiredDate ?? "";
        }

    }
}
