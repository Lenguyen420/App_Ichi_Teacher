using kido_teacher_app.Forms.Main.Page.GiaoAn;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Caching;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_GiaoAnTheoThang : UserControl
    {
        private readonly Size normalSize = new Size(205, 205);
        private readonly Size hoverSize = new Size(211, 212);

        private readonly string _className;
        private readonly string _classId;
        private readonly string _courseId;  
        public class MonthTag
        {
            public int Month { get; set; }
            public string CourseId { get; set; }
        }
        public UC_GiaoAnTheoThang(string className, string classId, string courseId)
        {
            InitializeComponent();

            _className = className;
            _classId = classId;
            _courseId = courseId;

            this.Load += UC_GiaoAnTheoThang_Load;
        }
        private void Month_MouseEnter(object sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                pic.Size = hoverSize;
                pic.Left = (pic.Parent.Width - pic.Width) / 2;
                pic.Top = (pic.Parent.Height - pic.Height) / 2;
                pic.BringToFront();
            }
        }

        private void Month_MouseLeave(object sender, EventArgs e)
        {
            if (sender is PictureBox pic)
            {
                pic.Size = normalSize;
                pic.Left = (pic.Parent.Width - pic.Width) / 2;
                pic.Top = (pic.Parent.Height - pic.Height) / 2;
            }
        }
        private void Month_Click(object sender, EventArgs e)
        {
            if (sender is not PictureBox pic || pic.Tag is not MonthTag tag)
                return;

            lblTitle.Text = $"Giáo Án / {_className} / Tháng {tag.Month}";

            Main_Form main = this.FindForm() as Main_Form;

            main?.LoadUserControl(
                new UC_GiaoAn_TheoThangChiTiet(
                    tag.Month,
                    _className,
                    _classId,
                    tag.CourseId
                )
            );
        }
        private void BtnBack_Click(object sender, EventArgs e)
        {
            Control parent = this.Parent;
            if (parent == null) return;

            parent.Controls.Remove(this);
            parent.Controls.Add(new UC_GiaoAn(_courseId) { Dock = DockStyle.Fill });
        }
        private void UC_GiaoAnTheoThang_Resize(object sender, EventArgs e)
        {
            int x = this.Width - btnBack.Width - 20;
            int y = lblWelcome.Height + (lblTitle.Height - btnBack.Height) / 2;

            btnBack.Location = new Point(x, y);
            btnBack.BringToFront();
        }

        // ===============================
        // ⭐ LOAD THÁNG TỪ API
        // ===============================
        private async void UC_GiaoAnTheoThang_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] START LOADING");
                
                var courses = await CourseService.GetAllAsync();

                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Total courses: {courses?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] ClassId filter: {_classId}");

                if (courses == null || courses.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] No courses loaded");
                    MessageBox.Show($"Không có khóa học nào.\nTotal: {courses?.Count ?? 0}", "Debug");
                    return;
                }

                // ⭐ DEBUG: In ra tất cả courses và ClassId của chúng
                foreach (var c in courses)
                {
                    System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Course: id={c.id}, code={c.code}, name={c.name}, ClassId={c.ClassId}");
                }

                var data = courses
                    .Where(x => x.ClassId == _classId)          // ⭐ LỌC THEO LỚP
                    .Where(x => !string.IsNullOrEmpty(x.code))
                    .Select(x => new
                    {
                        Course = x,  // ⭐ Giữ object gốc để debug
                        Month = 0,   // placeholder
                        CourseId = x.id,
                        x.name,
                        x.image
                    })
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Before parse - Count: {data.Count}");

                // ⭐ Parse month sau, có try-catch
                var parsedData = new List<(int Month, string CourseId, string Name, string Image)>();
                foreach (var item in data)
                {
                    try
                    {
                        int month = int.Parse(item.Course.code);
                        parsedData.Add((month, item.CourseId, item.name, item.image));
                        System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Parsed: code={item.Course.code} → month={month}, name={item.name}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Parse failed for code={item.Course.code}: {ex.Message}");
                    }
                }

                parsedData = parsedData.OrderBy(x => x.Month).ToList();

                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] After parse - Count: {parsedData.Count}");

                flowMonths.Controls.Clear();

                foreach (var c in parsedData)
                {
                    Panel wrap = new Panel
                    {
                        Width = 213,
                        Height = 213,
                        Margin = new Padding(15),
                        BackColor = Color.Transparent
                    };

                    PictureBox pic = new PictureBox
                    {
                        Size = normalSize,
                        Location = new Point(4, 4),
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Cursor = Cursors.Hand,
                        Tag = new MonthTag
                        {
                            Month = c.Month,
                            CourseId = c.CourseId
                        },
                        Image = Properties.Resources.coursedefault  // ⭐ Ảnh mặc định ngay
                    };

                    pic.MouseEnter += Month_MouseEnter;
                    pic.MouseLeave += Month_MouseLeave;
                    pic.Click += Month_Click;

                    // ⭐ Dùng CourseImageCacheService với classId subfolder
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var img = await CourseImageCacheService.GetOrDownloadImageAsync(
                                c.CourseId, 
                                c.Image,
                                _classId  // ⭐ Truyền classId để tạo subfolder
                            );
                            
                            if (img != null)
                            {
                                pic.Invoke(new Action(() => pic.Image = img));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Load image failed: {ex.Message}");
                        }
                    });

                    wrap.Controls.Add(pic);
                    flowMonths.Controls.Add(wrap);
                }

                lblTitle.Text = $"Giáo Án / {_className}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"LỖI LOAD:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", "ERROR");
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] EXCEPTION: {ex}");
            }
        }

    }
}
