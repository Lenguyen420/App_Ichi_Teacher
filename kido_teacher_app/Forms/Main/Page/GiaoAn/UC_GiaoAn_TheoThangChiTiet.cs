using kido_teacher_app.Config;
using kido_teacher_app.Forms.GiaoAn;
using kido_teacher_app.Helpers;
using kido_teacher_app.Model;
using kido_teacher_app.Models;
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Caching;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace kido_teacher_app.Forms.Main.Page.GiaoAn
{
    public partial class UC_GiaoAn_TheoThangChiTiet : UserControl
    {
        private readonly string _classId;
        private readonly string _courseId;
        private readonly int _month;
        private readonly string _className;

        //private LessonDto _lesson;

        private readonly LectureResourceService _resourceService = new LectureResourceService();
        public UC_GiaoAn_TheoThangChiTiet(
            int month,
            string className,
            string classId,
            string courseId
            )
        {
            InitializeComponent();
            //this.BackColor = Color.Red;

            _month = month;
            _classId = classId;
            _courseId = courseId;
            _className = className;

           
            //LoadLessons();


            lblInfo.Text = $"Giáo Án / {className} / Tháng {month}";
            this.Load += async (s, e) => await LoadLecturesAsync();
            
            // Xử lý resize để card tự động co giãn
            this.Resize += (s, e) =>
            {
                flowList.SuspendLayout();
                foreach (Control ctrl in flowList.Controls)
                {
                    if (ctrl is Panel card)
                    {
                        card.Width = flowList.ClientSize.Width - 40;
                    }
                }
                flowList.ResumeLayout();
            };
        }



        // =========================
        // LOAD BÀI HỌC TỪ API
        // =========================
        private async Task LoadLecturesAsync()
        {
            try
            {
                flowList.Controls.Clear();

                var lectures =
                    await LectureService.GetByClassCourseAsync(_classId, _courseId);

                if (lectures == null || lectures.Count == 0)
                {
                    // ⭐ KHÔNG HIỂN THỊ GÌ, CHỈ ĐỂ TRỐNG
                    return;
                }

                foreach (var lec in lectures)
                {
                    var detail = await LectureService.GetByIdAsync(lec.id);
                    if (detail == null) continue;

                    // ⭐ BẮT BUỘC
                    if (detail.resources == null)
                        detail.resources = new List<LectureResourceDto>();

                    //load cache
                    // =========================
                    // 🔥 LOAD OFFLINE CACHE (BƯỚC 4)
                    // =========================
                    var cache = LectureOfflineCacheService.Load(lec.id);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Load for lecture {lec.id}: {(cache != null ? "Found" : "Not found")}");

                    if (cache != null)
                    {
                        if (!string.IsNullOrEmpty(cache.PdfPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "PDF",
                                source = "LOCAL",
                                url = cache.PdfPath
                            });
                        }

                        if (!string.IsNullOrEmpty(cache.VideoPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "VIDEO",
                                source = "LOCAL",
                                url = cache.VideoPath
                            });
                        }

                        if (!string.IsNullOrEmpty(cache.ElearningPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "LESSON",
                                source = "LOCAL",
                                url = cache.ElearningPath
                            });
                        }
                    }


                    //end

                    string pdfOnline = null, videoOnline = null, lessonOnline = null;
                    string pdfOffline = null, videoOffline = null, lessonOffline = null;

                    foreach (var r in detail.resources)
                    {
                        if (r.type == "PDF" && r.source == "ONLINE") pdfOnline = r.url;
                        if (r.type == "VIDEO" && r.source == "ONLINE") videoOnline = r.url;
                        if (r.type == "LESSON" && r.source == "ONLINE") lessonOnline = r.url;


                        //offline

                        if (r.type == "PDF" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            pdfOffline = r.url;

                        if (r.type == "VIDEO" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            videoOffline = r.url;

                        if (r.type == "LESSON" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            lessonOffline = r.url;
                    }

                    flowList.Controls.Add(
                        CreateLessonItem(
                            detail,
                            lec.id,
                            detail.title ?? "(Không có tiêu đề)",
                            detail.code ?? "---",
                            pdfOnline,
                            videoOnline,
                            lessonOnline,
                            pdfOffline,
                            videoOffline,
                            lessonOffline
                        )
                    );
                }
            }
            catch (Exception ex)
            {
                // ⭐ LOG LỖI RA FILE (KHÔNG HIỂN THỊ MESSAGEBOX)
                System.Diagnostics.Debug.WriteLine($"[ERROR] LoadLecturesAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[STACK] {ex.StackTrace}");
            }
        }



        // =========================
        // HELPERS
        // =========================



        private void OpenOnline(string url, string title)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            // PDF
            if (url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                new Form_PdfViewer(url, title).Show();
                return;
            }

            // E-LEARNING (story.html)
            if (url.EndsWith("story.html", StringComparison.OrdinalIgnoreCase))
            {
                new Form_ElearningViewer(url, title).Show();
                return;
            }

            // VIDEO / LINK KHÁC
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                MessageBox.Show("Không mở được nội dung");
            }
        }

        private void OpenLocal(string filePath, string title)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                MessageBox.Show("File không tồn tại");
                return;
            }

            // PDF local
            if (filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                new Form_PdfViewer(filePath, title).Show();
                return;
            }

            // E-Learning local
            if (filePath.EndsWith("story.html", StringComparison.OrdinalIgnoreCase))
            {
                new Form_ElearningViewer(filePath, title).Show();
                return;
            }

            // VIDEO local
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không mở được file\n" + ex.Message);
            }
        }

        // =========================
        // UI HELPERS – BẮT BUỘC
        // =========================


        private Panel CreateLessonItem(
            LectureDto lesson,
            string lectureId,
            string title,
            string code,
            string pdfOnline,
            string videoOnline,
            string lessonOnline,
            string pdfOffline,
            string videoOffline,
            string lessonOffline
        )
        {
            // CARD CHA
            Panel card = new Panel
            {
                Height = 190,
                Width = flowList.ClientSize.Width - 40,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            // TABLE CHÍNH
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                RowCount = 1,
                Padding = new Padding(10)
            };

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); // Ảnh
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // Info (auto resize)
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Online
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200)); // Offline
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); // Xóa

            // =======================
            // CỘT 0: ẢNH
            // =======================
            PictureBox pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.lessondefault
            };
            table.Controls.Add(pic, 0, 0);

            // phần cột 2
            Panel info = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };

            // ===== LABEL MÃ SỐ (NẰM DƯỚI CÙNG) =====
            Label lblCode = new Label
            {
                Text = $"Mã số: {code ?? "---"}",
                ForeColor = Color.Blue,
                Dock = DockStyle.Bottom,
                Height = 22,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ===== LABEL TIÊU ĐỀ (CHIẾM HẾT PHẦN TRÊN) =====
            Label lblTitle = new Label
            {
                Text = title ?? "(Không có tiêu đề)",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.TopLeft
            };

            // THỨ TỰ ADD RẤT QUAN TRỌNG
            info.Controls.Add(lblTitle); // Fill
            info.Controls.Add(lblCode);  // Bottom
            table.Controls.Add(info, 1, 0);
            
            // =======================
            // CỘT 2: XEM ONLINE
            // =======================
            Panel online = new Panel { Dock = DockStyle.Fill };

            online.Controls.Add(new Label
            {
                Text = "Xem Online",
                ForeColor = Color.Green,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            });

            Button btnPdfOn = CreateSimpleButton("Giáo án PDF", Color.Green);
            Button btnVideoOn = CreateSimpleButton("Video dạy mẫu", Color.Green);
            Button btnLessonOn = CreateSimpleButton("Bài giảng E-Learning", Color.Green);

            btnPdfOn.Top = 30;
            btnVideoOn.Top = 65;
            btnLessonOn.Top = 100;

            btnPdfOn.Click += (s, e) => OpenOnline(pdfOnline, title);
            btnVideoOn.Click += (s, e) => OpenOnline(videoOnline, title);
            btnLessonOn.Click += (s, e) => OpenOnline(lessonOnline, title);

            online.Controls.AddRange(new Control[] { btnPdfOn, btnVideoOn, btnLessonOn });
            table.Controls.Add(online, 2, 0);

            // =======================
            // CỘT 3: XEM OFFLINE
            // =======================
            Panel offline = new Panel { Dock = DockStyle.Fill };

            offline.Controls.Add(new Label
            {
                Text = "Xem Offline",
                ForeColor = Color.Blue,
                Dock = DockStyle.Top,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            });

                      

            // OFFLINE BUTTON
            Button btnPdfOff = CreateSimpleButton("Giáo án PDF", Color.Red);
            Button btnVideoOff = CreateSimpleButton("Video dạy mẫu", Color.Red);
            Button btnLessonOff = CreateSimpleButton("Bài giảng E-Learning", Color.Red);

            btnPdfOff.Enabled = false;
            btnVideoOff.Enabled = false;
            btnLessonOff.Enabled = false;

            btnPdfOff.Top = 30;
            btnVideoOff.Top = 65;
            btnLessonOff.Top = 100;

            offline.Controls.AddRange(new Control[] { btnPdfOff, btnVideoOff, btnLessonOff });

            table.Controls.Add(offline, 3, 0);

            // =======================
            // CỘT 4: XÓA + CHƯA TẢI
            // =======================
            Panel deleteCol = new Panel { Dock = DockStyle.Fill };

            Button btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Width = 120,
                Height = 28,
                ForeColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Top = 0,
                Left = 20
            };

            
            btnDelete.Click -= BtnDeleteOffline_Click;
            btnDelete.Click += BtnDeleteOffline_Click;

            Button btnDown1 = CreateSimpleGrayButton("Chưa tải", 35);
            Button btnDown2 = CreateSimpleGrayButton("Chưa tải", 70);
            Button btnDown3 = CreateSimpleGrayButton("Chưa tải", 105);


            var offlineState = new OfflineLectureState();

            btnDown1.Tag = new object[] { lesson, btnPdfOff, btnVideoOff, btnLessonOff, btnDown1, btnDown2, btnDown3 };
            btnDown2.Tag = btnDown1.Tag;
            btnDown3.Tag = btnDown1.Tag;

            btnDown1.Click += BtnDownload_Click;
            btnDown2.Click += BtnDownload_Click;
            btnDown3.Click += BtnDownload_Click;

            deleteCol.Controls.AddRange(new Control[] { btnDelete, btnDown1, btnDown2, btnDown3 });
            table.Controls.Add(deleteCol, 4, 0);


            // =========================
            // ✅ ENABLE OFFLINE BUTTON NẾU ĐÃ TẢI
            // =========================

            // PDF
            if (!string.IsNullOrEmpty(pdfOffline) && File.Exists(pdfOffline))
            {
                btnPdfOff.Enabled = true;
                btnPdfOff.ForeColor = Color.Blue;
                btnPdfOff.FlatAppearance.BorderColor = Color.Blue;
                btnPdfOff.Click += (s, e) => OpenLocal(pdfOffline, title);

                btnDown1.Text = "Đã tải";
            }

            // VIDEO
            if (!string.IsNullOrEmpty(videoOffline) && File.Exists(videoOffline))
            {
                btnVideoOff.Enabled = true;
                btnVideoOff.ForeColor = Color.Blue;
                btnVideoOff.FlatAppearance.BorderColor = Color.Blue;
                btnVideoOff.Click += (s, e) => OpenLocal(videoOffline, title);

                btnDown2.Text = "Đã tải";
            }

            // LESSON
            if (!string.IsNullOrEmpty(lessonOffline) && File.Exists(lessonOffline))
            {
                btnLessonOff.Enabled = true;
                btnLessonOff.ForeColor = Color.Blue;
                btnLessonOff.FlatAppearance.BorderColor = Color.Blue;
                btnLessonOff.Click += (s, e) => OpenLocal(lessonOffline, title);

                btnDown3.Text = "Đã tải";
            }

            btnDelete.Tag = new object[]
            {
                lesson,
                btnPdfOff,
                btnVideoOff,
                btnLessonOff,
                btnDown1,
                btnDown2,
                btnDown3
            };

            // GẮN VÀO CARD
            card.Controls.Add(table);
            return card;
        }

        // =====================================================
        // BTN DOWNLOAD OFFLINE - OPTIMIZED (KHÔNG SỬA SERVICE)
        // =====================================================
        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not object[] data || data.Length < 7) return;

            var lesson = data[0] as LectureDto;
            var btnPdfOff = data[1] as Button;
            var btnVideoOff = data[2] as Button;
            var btnLessonOff = data[3] as Button;

            var btnDown1 = data[4] as Button;
            var btnDown2 = data[5] as Button;
            var btnDown3 = data[6] as Button;

            if (lesson == null) return;

            // =========================
            // 1️⃣ TÌM ZIP OFFLINE
            // =========================
            var offlineZip = lesson.resources
                .FirstOrDefault(r => r.source == "OFFLINE");

            if (offlineZip == null)
            {
                MessageBox.Show("Không có tài nguyên offline");
                return;
            }

            // =========================
            // 2️⃣ CHUẨN BỊ UI
            // =========================
            btnDown1.Enabled = btnDown2.Enabled = btnDown3.Enabled = false;
            btnDown1.Text = btnDown2.Text = btnDown3.Text = "Đang tải...";
            btnDown1.ForeColor = btnDown2.ForeColor = btnDown3.ForeColor = Color.Orange;

            // =========================
            // 3️⃣ PROGRESS (ĐÃ TỐI ƯU)
            // NOTE: tránh update UI liên tục
            // =========================
            int lastPercent = -1;
            var progress = new Progress<int>(percent =>
            {
                if (percent == lastPercent) return;
                lastPercent = percent;

                // NOTE: chỉ update 1 button để giảm lag UI
                btnDown1.Text = $"Đang tải {percent}%";
            });

            // =========================
            // 4️⃣ DOWNLOAD + EXTRACT (SỬ DỤNG PATH TỪ API)
            // NOTE: Đẩy toàn bộ await nặng sang background thread
            // =========================
            string? extractPath = null;

            await Task.Run(async () =>
            {
                extractPath = await LectureService
                    .DownloadAndExtractZipAsync(offlineZip.url, lesson.id, progress);
            });

            // =========================
            // 5️⃣ UPDATE UI SAU KHI XONG
            // =========================
            btnDown1.Text = btnDown2.Text = btnDown3.Text = "Đã tải";
            btnDown1.ForeColor = btnDown2.ForeColor = btnDown3.ForeColor = Color.Green;
            btnDown1.Enabled = btnDown2.Enabled = btnDown3.Enabled = false;

            if (string.IsNullOrEmpty(extractPath))
            {
                MessageBox.Show("Giải nén thất bại");
                return;
            }

            // =========================
            // 6️⃣ MAP FILE VÀ LƯU CACHE
            // =========================
            LectureFiles files = _resourceService.MapLectureFiles(extractPath);

            LectureOfflineCacheService.Save(
                lesson.id,
                files.PdfPath,
                files.VideoPath,
                files.ElearningPath
            );

            // =========================
            // 7️⃣ ENABLE OFFLINE BUTTON
            // =========================
            void EnableOfflineButton(Button btn, Action clickAction)
            {
                btn.Enabled = true;
                btn.ForeColor = Color.Blue;
                btn.FlatAppearance.BorderColor = Color.Blue;
                btn.FlatAppearance.BorderSize = 1;

                // NOTE: tránh gán click nhiều lần
                btn.Click -= (s, e) => clickAction();
                btn.Click += (s, e) => clickAction();
            }

            // PDF OFFLINE
            if (!string.IsNullOrEmpty(files.PdfPath))
            {
                EnableOfflineButton(
                    btnPdfOff,
                    () => OpenLocal(files.PdfPath, lesson.title)
                );
            }

            // VIDEO OFFLINE
            if (!string.IsNullOrEmpty(files.VideoPath))
            {
                EnableOfflineButton(
                    btnVideoOff,
                    () => OpenLocal(files.VideoPath, lesson.title)
                );
            }

            // E-LEARNING OFFLINE
            if (!string.IsNullOrEmpty(files.ElearningPath))
            {
                EnableOfflineButton(
                    btnLessonOff,
                    () => OpenLocal(files.ElearningPath, lesson.title)
                );
            }

        }

        private Button CreateSimpleButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = 170,
                Height = 30,
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Left = 20
            };
        }

        private Button CreateSimpleGrayButton(string text, int top)
        {
            return new Button
            {
                Text = text,
                Width = 120,
                Height = 28,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Top = top,
                Left = 20
            };
        }




        // sự kiện xóa
        private async void BtnDeleteOffline_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not object[] data || data.Length < 7) return;

            var lesson = data[0] as LectureDto;
            var btnPdfOff = data[1] as Button;
            var btnVideoOff = data[2] as Button;
            var btnLessonOff = data[3] as Button;

            var btnDown1 = data[4] as Button;
            var btnDown2 = data[5] as Button;
            var btnDown3 = data[6] as Button;

            if (lesson == null) return;

            // =========================
            // XÁC NHẬN XÓA
            // =========================
            var confirm = MessageBox.Show(
                $"Bạn có chắc muốn xóa giáo án offline:\n\n{lesson.title}",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirm != DialogResult.Yes) return;

            // =========================
            // 1️⃣ XÓA CACHE OFFLINE (BAO GỒM FILE VẬT LÝ)
            // =========================
            LectureOfflineCacheService.Delete(lesson.id);

            // =========================
            // 2️⃣ RELOAD LẠI TOÀN BỘ DANH SÁCH ĐỂ CẬP NHẬT UI
            // =========================
            await LoadLecturesAsync();

            MessageBox.Show("Đã gỡ giáo án offline!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        //end


        class OfflineLectureState
        {
            public bool IsDownloaded { get; set; }
            public LectureFiles? Files { get; set; }
        }
    }
}
