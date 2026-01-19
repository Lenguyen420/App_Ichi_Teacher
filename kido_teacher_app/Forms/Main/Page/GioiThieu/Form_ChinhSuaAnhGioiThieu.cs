using System;
using System.Drawing;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page.QuanLyNoiDung
{
    public partial class Form_ChinhSuaAnhGioiThieu : Form
    {
        // Đếm số hình
        private int imageIndex = 1;

        public Form_ChinhSuaAnhGioiThieu()
        {
            InitializeComponent();

            // Gán sự kiện
            this.Load += Form_ChinhSuaAnhGioiThieu_Load;
            btnThemHinh.Click += BtnThemHinh_Click;
        }

        // ================= LOAD =================
        private void Form_ChinhSuaAnhGioiThieu_Load(object sender, EventArgs e)
        {
            // Load mặc định Hình 1
            flowImages.Controls.Add(CreateImageBlock(imageIndex));
        }

        // ================= THÊM HÌNH =================
        private void BtnThemHinh_Click(object sender, EventArgs e)
        {
            imageIndex++;
            flowImages.Controls.Add(CreateImageBlock(imageIndex));
        }

        // ================= TẠO 1 KHUNG HÌNH =================
        private Panel CreateImageBlock(int index)
        {
            Panel panel = new Panel
            {
                Width = 1090,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(8)
            };

            // ===== TITLE =====
            Label lblBlock = new Label
            {
                Text = $"Hình {index}",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            // ===== DÒNG 1 =====
            Label lblTieuDe = new Label
            {
                Text = "Tiêu Đề:",
                Location = new Point(20, 42),
                AutoSize = true
            };

            TextBox txtTieuDe = new TextBox
            {
                Location = new Point(90, 38),
                Width = 650
            };

            // ===== DÒNG 2 =====
            Label lblLienKet = new Label
            {
                Text = "Liên Kết:",
                Location = new Point(20, 72),
                AutoSize = true
            };

            TextBox txtLienKet = new TextBox
            {
                Location = new Point(90, 68),
                Width = 650
            };

            // ===== DÒNG 3 =====
            Label lblHinhAnh = new Label
            {
                Text = "Hình Ảnh:",
                Location = new Point(20, 102),
                AutoSize = true
            };

            TextBox txtHinhAnh = new TextBox
            {
                Location = new Point(90, 98),
                Width = 650
            };

            // ===== THỜI GIAN =====
            Label lblThoiGian = new Label
            {
                Text = "Thời Gian:",
                Location = new Point(760, 42),
                AutoSize = true
            };

            NumericUpDown numTime = new NumericUpDown
            {
                Location = new Point(840, 38),   // cách label ra
                Width = 60,
                Minimum = 1,
                Maximum = 999,
                Value = 30                      // ✅ mặc định 30
            };

            // ===== STOP =====
            CheckBox chkStop = new CheckBox
            {
                Text = "Stop",
                Location = new Point(760, 72),
                AutoSize = true
            };

            // ===== XÓA =====
            LinkLabel lnkXoa = new LinkLabel
            {
                Text = "Xóa",
                Location = new Point(840, 72),
                AutoSize = true,
                LinkColor = Color.Red
            };

            // ===== PREVIEW =====
            PictureBox picPreview = new PictureBox
            {
                Location = new Point(920, 30),
                Size = new Size(150, 90),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White
            };

            // ===== SỰ KIỆN XÓA =====
            lnkXoa.Click += (s, e) =>
            {
                flowImages.Controls.Remove(panel);
                panel.Dispose();
            };

            // ===== ADD =====
            panel.Controls.AddRange(new Control[]
            {
        lblBlock,
        lblTieuDe, txtTieuDe,
        lblLienKet, txtLienKet,
        lblHinhAnh, txtHinhAnh,
        lblThoiGian, numTime,
        chkStop, lnkXoa,
        picPreview
            });

            return panel;
        }

    }
}
