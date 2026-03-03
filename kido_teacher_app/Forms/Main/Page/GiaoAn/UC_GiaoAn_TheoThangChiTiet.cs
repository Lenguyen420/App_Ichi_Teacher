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
using System.Threading;
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

        private readonly string _courseName;
        private int _loadVersion;
        private readonly float _dpiScale;

        //private LessonDto _lesson;

        private readonly LectureResourceService _resourceService = new LectureResourceService();
        public UC_GiaoAn_TheoThangChiTiet(
            int month,
            string className,
            string classId,
            string courseId,
            string courseName
            )
        {
            InitializeComponent();
            //this.BackColor = Color.Red;

            this.AutoScaleMode = AutoScaleMode.Dpi;
            _dpiScale = this.DeviceDpi / 96f;

            _classId = classId;
            _courseId = courseId;
            _className = className;
            _courseName = courseName;

           
            //LoadLessons();


            lblInfo.Text = $"Giáo Án / {className} / {courseName}";
            this.Load += async (s, e) => await LoadLecturesAsync();
            this.flowList.SizeChanged += (s, e) => UpdateCardWidths();
            ApplyDpiScaling();
            
        }



        // =========================
        // LOAD BÀI HỌC TỪ API
        // =========================
        private async Task LoadLecturesAsync()
        {
            int loadVersion = Interlocked.Increment(ref _loadVersion);
            System.Diagnostics.Debug.WriteLine(
                $"[UC_GiaoAn_TheoThangChiTiet] LoadLecturesAsync start v={loadVersion}, class={_classId}, course={_courseId}"
            );

            try
            {
                flowList.Controls.Clear();

                var lecturesRaw =
                    await LectureService.GetByClassCourseAsync(_classId, _courseId);
                var lectures = LectureService.NormalizeLectures(lecturesRaw);

                if (loadVersion != _loadVersion)
                    return;

                if (lectures == null || lectures.Count == 0)
                {
                    // ⭐ KHÔNG HIỂN THỊ GÌ, CHỈ ĐỂ TRỐNG
                    return;
                }

                foreach (var lec in lectures)
                {
                    if (loadVersion != _loadVersion)
                        return;

                    var detail = lec;
                    bool needFetchDetail =
                        detail.resources == null ||
                        detail.resources.Count == 0 ||
                        string.IsNullOrWhiteSpace(detail.title) ||
                        string.IsNullOrWhiteSpace(detail.code);

                    if (needFetchDetail)
                    {
                        var fetched = await LectureService.GetByIdAsync(lec.id);
                        if (loadVersion != _loadVersion)
                            return;

                        if (fetched != null)
                            detail = fetched;
                    }

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

                    string pdfOffline = null, videoOffline = null, lessonOffline = null;

                    foreach (var r in detail.resources)
                    {
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
                            pdfOffline,
                            videoOffline,
                            lessonOffline
                        )
                    );
                }

                UpdateCardWidths();
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
            string pdfOffline,
            string videoOffline,
            string lessonOffline
        )
        {
            // CARD CHA
            Panel card = new Panel
            {
                Height = Scale(190),
                Width = GetCardWidth(),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(Scale(5))
            };

            // TABLE CHÍNH
            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(Scale(10))
            };

            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(170))); // Ảnh
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 380)); // Info
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(200)));        // Offline
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Scale(160)));       // Xóa

            // =======================
            // CỘT 1: ẢNH
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
                Padding = new Padding(Scale(5))
            };

            // ===== LABEL MÃ SỐ (NẰM DƯỚI CÙNG) =====
            Label lblCode = new Label
            {
                Text = $"Mã số: {code ?? "---"}",
                ForeColor = Color.Blue,
                Dock = DockStyle.Bottom,
                Height = Scale(22),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // ===== LABEL TỐC ĐỘ MẠNG (DƯỚI MÃ SỐ) =====
            Label lblSpeed = new Label
            {
                Text = "Tốc độ: -- MB/s",
                ForeColor = Color.Gray,
                Dock = DockStyle.Bottom,
                Height = Scale(20),
                TextAlign = ContentAlignment.MiddleLeft,
                Visible = false
            };

            // ===== LABEL TIÊU ĐỀ (CHIẾM HẾT PHẦN TRÊN) =====
            Label lblTitle = new Label
            {
                Text = title ?? "(Không có tiêu đề)",
                Font = new Font("Segoe UI", ScaleFont(12), FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = Scale(32),
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // THỨ TỰ ADD RẤT QUAN TRỌNG
            info.Controls.Add(lblTitle); // Top (single line)
            info.Controls.Add(lblCode);  // Bottom
            info.Controls.Add(lblSpeed); // Bottom (dưới mã số)
            table.Controls.Add(info, 1, 0);
            // =======================
            // CỘT 3: XEM OFFLINE
            // =======================
            FlowLayoutPanel offline = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                AutoScroll = false
            };

            offline.Controls.Add(new Label
            {
                Text = "Xem Offline",
                ForeColor = Color.Blue,
                Height = Scale(24),
                Width = Scale(170),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", ScaleFont(9), FontStyle.Bold),
                Margin = new Padding(0, 0, 0, Scale(6))
            });

                      

            // OFFLINE BUTTON
            Button btnPdfOff = CreateSimpleButton("Giáo án PDF", Color.Red);
            Button btnVideoOff = CreateSimpleButton("Video dạy mẫu", Color.Red);
            Button btnLessonOff = CreateSimpleButton("Bài giảng E-Learning", Color.Red);

            btnPdfOff.Enabled = false;
            btnVideoOff.Enabled = false;
            btnLessonOff.Enabled = false;

            btnPdfOff.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnVideoOff.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnLessonOff.Margin = new Padding(Scale(20), 0, 0, 0);

            offline.Controls.AddRange(new Control[] { btnPdfOff, btnVideoOff, btnLessonOff });

            table.Controls.Add(offline, 2, 0);

            // =======================
            // CỘT 4: XÓA + CHƯA TẢI
            // =======================
            FlowLayoutPanel deleteCol = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0),
                AutoScroll = false
            };

            Button btnDelete = new Button
            {
                Text = "🗑 Xóa",
                Width = Scale(120),
                Height = Scale(28),
                ForeColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(Scale(20), 0, 0, Scale(6))
            };

            
            btnDelete.Click -= BtnDeleteOffline_Click;
            btnDelete.Click += BtnDeleteOffline_Click;

            Button btnDown1 = CreateSimpleGrayButton("Chưa tải");
            Button btnDown2 = CreateSimpleGrayButton("Chưa tải");
            Button btnDown3 = CreateSimpleGrayButton("Chưa tải");

            btnDown1.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnDown2.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnDown3.Margin = new Padding(Scale(20), 0, 0, 0);


            var offlineState = new OfflineLectureState();

            btnDown1.Tag = new object[]
            {
                lesson,
                btnPdfOff,
                btnVideoOff,
                btnLessonOff,
                btnDown1,
                btnDown2,
                btnDown3,
                lblSpeed
            };
            btnDown2.Tag = btnDown1.Tag;
            btnDown3.Tag = btnDown1.Tag;

            btnDown1.Click += BtnDownload_Click;
            btnDown2.Click += BtnDownload_Click;
            btnDown3.Click += BtnDownload_Click;

            deleteCol.Controls.AddRange(new Control[] { btnDelete, btnDown1, btnDown2, btnDown3 });
            table.Controls.Add(deleteCol, 3, 0);


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

        private int GetCardWidth()
        {
            if (flowList == null) return 800;

            int padding = flowList.Padding.Horizontal;
            int scrollbar = SystemInformation.VerticalScrollBarWidth;
            int width = flowList.ClientSize.Width - padding - scrollbar - 8;

            return Math.Max(Scale(700), width);
        }

        private void UpdateCardWidths()
        {
            if (flowList == null) return;
            int width = GetCardWidth();

            foreach (Control c in flowList.Controls)
            {
                if (c is Panel p)
                    p.Width = width;
            }
        }

        // =====================================================
        // BTN DOWNLOAD OFFLINE - OPTIMIZED (KHÔNG SỬA SERVICE)
        // =====================================================
        private async void BtnDownload_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not object[] data || data.Length < 8) return;

            var lesson = data[0] as LectureDto;
            var btnPdfOff = data[1] as Button;
            var btnVideoOff = data[2] as Button;
            var btnLessonOff = data[3] as Button;

            var btnDown1 = data[4] as Button;
            var btnDown2 = data[5] as Button;
            var btnDown3 = data[6] as Button;
            var lblSpeed = data[7] as Label;

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
            if (lblSpeed != null)
            {
                lblSpeed.ForeColor = Color.Orange;
                lblSpeed.Text = "Tốc độ: đang đo...";
                lblSpeed.Visible = true;
            }

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

            var statsProgress = new Progress<DownloadStats>(stat =>
            {
                if (lblSpeed == null) return;

                if (stat.Phase == "DOWNLOAD")
                {
                    lblSpeed.Text = $"Tốc độ: {stat.SpeedMbps:0.0} MB/s";
                }
                else if (stat.Phase == "EXTRACT")
                {
                    lblSpeed.Text = "Đang giải nén...";
                }
            });

            // =========================
            // 4️⃣ DOWNLOAD + EXTRACT (SỬ DỤNG PATH TỪ API)
            // NOTE: Đẩy toàn bộ await nặng sang background thread
            // =========================
            string? extractPath = null;

            await Task.Run(async () =>
            {
                extractPath = await LectureService
                    .DownloadAndExtractZipAsync(offlineZip.url, lesson.id, progress, statsProgress);
            });

            // =========================
            // 5️⃣ UPDATE UI SAU KHI XONG
            // =========================
            btnDown1.Text = btnDown2.Text = btnDown3.Text = "Đã tải";
            btnDown1.ForeColor = btnDown2.ForeColor = btnDown3.ForeColor = Color.Green;
            btnDown1.Enabled = btnDown2.Enabled = btnDown3.Enabled = false;
            if (lblSpeed != null)
            {
                lblSpeed.ForeColor = Color.Gray;
                lblSpeed.Visible = false;
            }

            if (string.IsNullOrEmpty(extractPath))
            {
                if (lblSpeed != null)
                {
                    lblSpeed.Text = "Tốc độ: -- MB/s";
                    lblSpeed.Visible = false;
                }
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
                Width = Scale(170),
                Height = Scale(30),
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9), FontStyle.Regular)
            };
        }

        private Button CreateSimpleGrayButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = Scale(120),
                Height = Scale(28),
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", ScaleFont(9), FontStyle.Regular)
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

        private int Scale(int value)
        {
            return (int)Math.Round(value * _dpiScale);
        }

        private float ScaleFont(float size)
        {
            return size * _dpiScale;
        }

        private void ApplyDpiScaling()
        {
            lblHeader.Height = Scale(50);
            lblHeader.Font = new Font("Segoe UI", ScaleFont(16), FontStyle.Bold);
            lblHeader.Padding = new Padding(Scale(20), 0, 0, 0);

            lblInfo.Height = Scale(40);
            lblInfo.Font = new Font("Segoe UI", ScaleFont(14), FontStyle.Bold);
            lblInfo.Padding = new Padding(Scale(20), 0, 0, 0);

            flowList.Padding = new Padding(Scale(15));

            if (btnBack != null)
            {
                btnBack.Width = Scale(30);
                btnBack.Height = Scale(30);
            }
        }
    }
}
