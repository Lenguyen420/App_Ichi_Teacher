using kido_teacher_app.Forms.Main.Page.GiaoAn;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Caching;
using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
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
            public string CourseName { get; set; }
        }
        public UC_GiaoAnTheoThang(string className, string classId, string courseId)
        {
            InitializeComponent();

            _className = className;
            _classId = classId;
            _courseId = courseId;

            this.Load += UC_GiaoAnTheoThang_Load;
            flowMonths.SizeChanged += (s, e) => UpdateMonthFlowPadding();
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

            lblTitle.Text = $"Giáo Án / {_className} / {tag.CourseName}";

            Main_Form main = this.FindForm() as Main_Form;

            main?.LoadUserControl(
                new UC_GiaoAn_TheoThangChiTiet(
                    tag.Month,
                    _className,
                    _classId,
                    tag.CourseId,
                    tag.CourseName
                )
            );
        }
        private void BtnBack_Click(object sender, EventArgs e)
        {
            Main_Form main = this.FindForm() as Main_Form;
            if (main == null) return;

            main.LoadUserControl(new UC_GiaoAn(_courseId));
        }

        // ===============================
        // ⭐ LOAD THÁNG TỪ API
        // ===============================
        private async void UC_GiaoAnTheoThang_Load(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] START LOADING");
                
                var courses = await CourseService.GetByClassIdAsync(_classId);

                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Total courses: {courses?.Count ?? 0}");
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] ClassId: {_classId}");

                if (courses == null || courses.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] No courses loaded");
                    courses = await LoadCoursesFromLectureCacheAsync();
                    if (courses == null || courses.Count == 0)
                        return;
                }

                // ⭐ DEBUG: In ra tất cả courses nhận từ API theo classId
                foreach (var c in courses)
                {
                    System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Course: id={c.id}, code={c.code}, name={c.name}, ClassId={c.ClassId}");
                }

                var data = courses
                    .Where(x => !string.IsNullOrWhiteSpace(x.name))
                    .Select(x => new
                    {
                        CourseId = x.id,
                        Name = x.name,
                        Image = x.image
                    })
                    .ToList();

                var parsedData = data
                    .OrderBy(x => x.Name, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] Loaded by name - Count: {parsedData.Count}");

                flowMonths.Controls.Clear();

                foreach (var c in parsedData)
                {
                    Panel wrap = new Panel
                    {
                        Width = 213,
                        Height = 213,
                        Margin = new Padding(8),
                        BackColor = Color.Transparent
                    };

                    PictureBox pic = new PictureBox
                    {
                        Size = normalSize,
                        Location = new Point(4, 4),
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Cursor = Cursors.Hand,
                        Tag = new MonthTag
                        {
                            Month = 0,
                            CourseId = c.CourseId,
                            CourseName = c.Name
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

                UpdateMonthFlowPadding();
                lblTitle.Text = $"Giáo Án / {_className}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"LỖI LOAD:\n{ex.Message}\n\nStack:\n{ex.StackTrace}", "ERROR");
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAnTheoThang] EXCEPTION: {ex}");
            }
        }

        private async Task<List<CourseDto>> LoadCoursesFromLectureCacheAsync()
        {
            var prefix = $"lectures_class_{_classId}_course_";
            var keys = await DbCacheService.GetKeysByPrefixAsync(prefix);
            if (keys == null || keys.Count == 0)
                return new List<CourseDto>();

            var courseIds = keys
                .Select(k => k.StartsWith(prefix) ? k.Substring(prefix.Length) : null)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (courseIds.Count == 0)
                return new List<CourseDto>();

            var cachedCourses = await DbCacheService.GetAsync<List<CourseDto>>($"courses_class_{_classId}")
                               ?? await DbCacheService.GetAsync<List<CourseDto>>("courses_all")
                               ?? new List<CourseDto>();

            var result = new List<CourseDto>();
            int fallbackMonth = 1;

            foreach (var id in courseIds)
            {
                var c = cachedCourses.FirstOrDefault(x => x.id == id);
                if (c == null)
                {
                    c = new CourseDto
                    {
                        id = id,
                        code = fallbackMonth.ToString(),
                        name = $"Khóa học (offline) {fallbackMonth}",
                        image = null
                    };
                    fallbackMonth++;
                }

                result.Add(c);
            }

            return result;
        }

        private void UpdateMonthFlowPadding()
        {
            if (flowMonths == null)
                return;

            const int itemWidth = 229;
            const int minHorizontalPadding = 8;
            int availableWidth = Math.Max(0, flowMonths.ClientSize.Width);

            if (availableWidth <= itemWidth)
            {
                flowMonths.Padding = new Padding(minHorizontalPadding, 20, minHorizontalPadding, 20);
                return;
            }

            int itemsPerRow = Math.Max(1, availableWidth / itemWidth);
            int usedWidth = itemsPerRow * itemWidth;
            int horizontalPadding = Math.Max(minHorizontalPadding, (availableWidth - usedWidth) / 2);
            flowMonths.Padding = new Padding(horizontalPadding, 20, horizontalPadding, 20);
        }

    }
}
