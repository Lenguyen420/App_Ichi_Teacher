using kido_teacher_app.Config;
using kido_teacher_app.Forms.GiaoAn;
using kido_teacher_app.Helpers;
using kido_teacher_app.Model;
using kido_teacher_app.Models;
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Caching;
using kido_teacher_app.Shared.Network;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private bool _loadStarted;
        private string? _lastRenderedLectureSignature;
        private readonly Dictionary<Button, EventHandler> _offlineButtonHandlers = new();

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
            _classId = classId;
            _courseId = courseId;
            _month = month;
            _className = className;
            _courseName = courseName;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer
                    | ControlStyles.UserPaint,
                true
            );
            UpdateStyles();
            SetDoubleBuffered(flowList);

            lblInfo.Text = $"Giáo Án / {className} / {courseName}";
            this.Load += UC_GiaoAn_TheoThangChiTiet_Load;
            this.flowList.SizeChanged += (s, e) => UpdateCardWidths();
        }

        private void UC_GiaoAn_TheoThangChiTiet_Load(object? sender, EventArgs e)
        {
            if (_loadStarted)
                return;

            _loadStarted = true;
            BeginInvoke(new Action(async () => await LoadLecturesAsync()));
        }



        // =========================
        // LOAD BÀI HỌC TỪ API
        // =========================
        private async Task LoadLecturesAsync()
        {
            int loadVersion = Interlocked.Increment(ref _loadVersion);
            string cacheKey = $"lectures_class_{_classId}_course_{_courseId}";

            try
            {
                if (loadVersion != _loadVersion)
                    return;

                var cachedLecturesRaw = await DbCacheService.GetAsync<List<LectureDto>>(cacheKey);
                var cachedLectures = LectureService.NormalizeLectures(cachedLecturesRaw);
                bool renderedFromCache = false;

                if (cachedLectures.Count > 0)
                {
                    await RenderLecturesAsync(cachedLectures, loadVersion);
                    renderedFromCache = true;
                }

                if (OfflineState.IsOffline())
                {
                    if (!renderedFromCache)
                        flowList.Controls.Clear();
                    return;
                }

                var lecturesRaw = await LectureService.GetByClassCourseAsync(_classId, _courseId);
                var lectures = LectureService.NormalizeLectures(lecturesRaw);

                if (loadVersion != _loadVersion)
                    return;

                if (lectures.Count == 0)
                {
                    if (!renderedFromCache)
                        flowList.Controls.Clear();
                    return;
                }

                if (
                    renderedFromCache
                    && string.Equals(
                        _lastRenderedLectureSignature,
                        BuildLectureSignature(lectures),
                        StringComparison.Ordinal
                    )
                )
                {
                    return;
                }

                await RenderLecturesAsync(lectures, loadVersion);
            }
            catch (Exception ex)
            {
            }
        }

        private async Task RenderLecturesAsync(
            List<LectureDto> lectures,
            int loadVersion)
        {
            if (loadVersion != _loadVersion)
                return;

            _lastRenderedLectureSignature = BuildLectureSignature(lectures);
            flowList.SuspendLayout();
            flowList.Controls.Clear();

            foreach (var lec in lectures)
            {
                try
                {
                    if (loadVersion != _loadVersion)
                        return;

                    var detail = lec;

                    if (detail.resources == null)
                        detail.resources = new List<LectureResourceDto>();

                    string? currentOfflineZipUrl = detail.resources
                        .FirstOrDefault(r => r.source == "OFFLINE")
                        ?.url;

                    var cache = LectureOfflineCacheService.Load(lec.id);

                    if (cache != null)
                    {
                        // Only add resources if file actually exists
                        if (!string.IsNullOrEmpty(cache.PdfPath) && File.Exists(cache.PdfPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "PDF",
                                source = "LOCAL",
                                url = cache.PdfPath
                            });
                        }

                        if (!string.IsNullOrEmpty(cache.VideoPath) && File.Exists(cache.VideoPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "VIDEO",
                                source = "LOCAL",
                                url = cache.VideoPath
                            });
                        }

                        if (!string.IsNullOrEmpty(cache.ElearningPath) && File.Exists(cache.ElearningPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "LESSON",
                                source = "LOCAL",
                                url = cache.ElearningPath
                            });
                        }

                        if (!string.IsNullOrEmpty(cache.PowerPointPath) && File.Exists(cache.PowerPointPath))
                        {
                            detail.resources.Add(new LectureResourceDto
                            {
                                type = "POWERPOINT",
                                source = "LOCAL",
                                url = cache.PowerPointPath
                            });
                        }
                    }

                    string pdfOffline = null, videoOffline = null, lessonOffline = null, powerPointOffline = null;
                    string? cachedOfflineZipUrl = cache?.OfflineZipUrl;

                    foreach (var r in detail.resources)
                    {
                        if (r.type == "PDF" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            pdfOffline = r.url;

                        if (r.type == "VIDEO" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            videoOffline = r.url;

                        if (r.type == "LESSON" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            lessonOffline = r.url;

                        if (r.type == "POWERPOINT" && (r.source == "OFFLINE" || r.source == "LOCAL"))
                            powerPointOffline = r.url;
                    }

                    bool needsUpdate =
                        cache != null &&
                        !string.IsNullOrWhiteSpace(cachedOfflineZipUrl) &&
                        !string.IsNullOrWhiteSpace(currentOfflineZipUrl) &&
                        !string.Equals(
                            cachedOfflineZipUrl,
                            currentOfflineZipUrl,
                            StringComparison.OrdinalIgnoreCase
                        );

                    flowList.Controls.Add(
                        CreateLessonItem(
                            detail,
                            lec.id,
                            detail.title ?? "(Không có tiêu đề)",
                            detail.code ?? "---",
                            pdfOffline,
                            videoOffline,
                            lessonOffline,
                            powerPointOffline,
                            needsUpdate,
                            cache != null
                        )
                    );
                }
                catch (Exception ex)
                {
                }
            }

            flowList.ResumeLayout();
            UpdateCardWidths();
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

            if (
                filePath.EndsWith(".pptx", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".ppsx", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".ppt", StringComparison.OrdinalIgnoreCase)
                || filePath.EndsWith(".pps", StringComparison.OrdinalIgnoreCase)
            )
            {
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
                }
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
            string lessonOffline,
            string powerPointOffline,
            bool needsUpdate,
            bool isDownloaded
        )
        {
            int actionButtonHeight = Scale(34);
            int actionButtonSpacing = Scale(6);
            int downloadButtonHeight = Scale(132);

            // CARD CHA
            Panel card = new Panel
            {
                Height = GetCardHeight(needsUpdate, actionButtonHeight, actionButtonSpacing, downloadButtonHeight),
                Width = GetCardWidth(),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(Scale(5))
            };

            // THÔNG BÁO CẬP NHẬT NẾU CẦN
            Label? lblUpdateNotification = null;
            if (needsUpdate)
            {
                lblUpdateNotification = new Label
                {
                    Text = "Có cập nhật",
                    ForeColor = Color.Red,
                    BackColor = Color.Yellow,
                    Font = new Font("Segoe UI", ScaleFont(8), FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(Scale(5), Scale(5)),
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(Scale(2))
                };
                card.Controls.Add(lblUpdateNotification);
                lblUpdateNotification.BringToFront();
            }

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
            Panel imageHost = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(0)
            };

            PictureBox pic = new PictureBox
            {
                Size = new Size(Scale(150), Scale(150)),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = Properties.Resources.giaoan2
            };

            void CenterImage()
            {
                pic.Left = Math.Max(0, (imageHost.ClientSize.Width - pic.Width) / 2);
                pic.Top = Math.Max(0, (imageHost.ClientSize.Height - pic.Height) / 2);
            }

            imageHost.Controls.Add(pic);
            imageHost.Resize += (s, e) => CenterImage();
            CenterImage();

            table.Controls.Add(imageHost, 0, 0);

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
                AutoEllipsis = false,
                UseCompatibleTextRendering = false,
                TextAlign = ContentAlignment.TopLeft
            };

            // THỨ TỰ ADD RẤT QUAN TRỌNG
            info.Controls.Add(lblTitle); // Top (single line)
            info.Controls.Add(lblCode);  // Bottom
            info.Controls.Add(lblSpeed); // Bottom (dưới mã số)
            table.Controls.Add(info, 1, 0);
            UpdateTitleLayout(lblTitle, info, lblCode, lblSpeed);
            info.SizeChanged += (s, e) => UpdateTitleLayout(lblTitle, info, lblCode, lblSpeed);
            // =======================
            // CỘT 3: XEM OFFLINE
            // =======================
            FlowLayoutPanel offline = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, Scale(4)),
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
            Button btnPowerPointOff = CreateSimpleButton("Bài giảng PowerPoint", Color.Red);

            btnPdfOff.Enabled = false;
            btnVideoOff.Enabled = false;
            btnLessonOff.Enabled = false;
            btnPowerPointOff.Enabled = false;

            btnPdfOff.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnVideoOff.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnLessonOff.Margin = new Padding(Scale(20), 0, 0, Scale(6));
            btnPowerPointOff.Margin = new Padding(Scale(20), 0, 0, 0);

            offline.Controls.AddRange(new Control[] { btnPdfOff, btnVideoOff, btnLessonOff, btnPowerPointOff });

            Label lblUpdate = new Label
            {
                Text = "Vui lòng cập nhật giáo án mới",
                ForeColor = Color.Red,
                Height = Scale(20),
                Width = Scale(170),
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, Scale(6), 0, 0),
                Visible = needsUpdate
            };

            offline.Controls.Add(lblUpdate);

            table.Controls.Add(offline, 2, 0);

            // =======================
            // CỘT 4: XÓA + TẢI OFFLINE
            // =======================
            FlowLayoutPanel deleteCol = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Padding = new Padding(0, 0, 0, Scale(4)),
                AutoScroll = false
            };

            Button btnDelete = new Button
            {
                Text = "Xóa",
                Width = Scale(120),
                Height = actionButtonHeight,
                ForeColor = Color.Red,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(Scale(20), 0, 0, actionButtonSpacing)
            };

            
            btnDelete.Click -= BtnDeleteOffline_Click;
            btnDelete.Click += BtnDeleteOffline_Click;

            Button btnDownload = CreateSimpleGrayButton(
                isDownloaded ? "Đã tải" : "Tải offline"
            );

            btnDownload.Height = downloadButtonHeight;
            btnDownload.Margin = new Padding(Scale(20), 0, 0, 0);


            var offlineState = new OfflineLectureState();

            btnDownload.Tag = new object[]
            {
                lesson,
                btnPdfOff,
                btnVideoOff,
                btnLessonOff,
                btnPowerPointOff,
                btnDownload,
                lblSpeed,
                lblUpdate
            };

            btnDownload.Click += BtnDownload_Click;

            deleteCol.Controls.AddRange(new Control[] { btnDelete, btnDownload });
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
            }

            // VIDEO
            if (!string.IsNullOrEmpty(videoOffline) && File.Exists(videoOffline))
            {
                btnVideoOff.Enabled = true;
                btnVideoOff.ForeColor = Color.Blue;
                btnVideoOff.FlatAppearance.BorderColor = Color.Blue;
                btnVideoOff.Click += (s, e) => OpenLocal(videoOffline, title);
            }

            // LESSON
            if (!string.IsNullOrEmpty(lessonOffline) && File.Exists(lessonOffline))
            {
                btnLessonOff.Enabled = true;
                btnLessonOff.ForeColor = Color.Blue;
                btnLessonOff.FlatAppearance.BorderColor = Color.Blue;
                btnLessonOff.Click += (s, e) => OpenLocal(lessonOffline, title);
            }

            if (!string.IsNullOrEmpty(powerPointOffline) && File.Exists(powerPointOffline))
            {
                btnPowerPointOff.Enabled = true;
                btnPowerPointOff.ForeColor = Color.Blue;
                btnPowerPointOff.FlatAppearance.BorderColor = Color.Blue;
                btnPowerPointOff.Click += (s, e) => OpenLocal(powerPointOffline, title);
            }

            btnDelete.Tag = new object[]
            {
                lesson,
                btnPdfOff,
                btnVideoOff,
                btnLessonOff,
                btnPowerPointOff,
                btnDownload
            };

            lblUpdate.Tag = btnDownload.Tag;
            lblUpdate.Click += BtnUpdate_Click;
            if (lblUpdateNotification != null)
            {
                lblUpdateNotification.Tag = btnDownload.Tag;
                lblUpdateNotification.Click += BtnUpdate_Click;
            }

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

            await DownloadOrUpdateAsync(data, isUpdate: false);
        }

        private async void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (sender is not Label lbl) return;
            if (lbl.Tag is not object[] data || data.Length < 8) return;

            await DownloadOrUpdateAsync(data, isUpdate: true);
        }

        private async Task DownloadOrUpdateAsync(object[] data, bool isUpdate)
        {
            var lesson = data[0] as LectureDto;
            var btnPdfOff = data[1] as Button;
            var btnVideoOff = data[2] as Button;
            var btnLessonOff = data[3] as Button;
            var btnPowerPointOff = data[4] as Button;

            var btnDownload = data[5] as Button;
            var lblSpeed = data[6] as Label;
            var lblUpdate = data[7] as Label;

            if (lesson == null) return;
            if (btnDownload == null) return;

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
            btnDownload.Enabled = false;
            btnDownload.Text = "Đang tải...";
            btnDownload.ForeColor = Color.Orange;
            if (lblUpdate != null)
            {
                lblUpdate.Enabled = false;
                lblUpdate.Visible = isUpdate ? true : lblUpdate.Visible;
                if (isUpdate) lblUpdate.Text = "Đang cập nhật...";
            }
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
                btnDownload.Text = $"Đang tải {percent}%";
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
                if (isUpdate)
                {
                    // Tạo temp lectureId
                    var tempLectureId = lesson.id + "_temp";
                    var tempExtractPath = await LectureService
                        .DownloadAndExtractZipAsync(offlineZip.url, tempLectureId, progress, statsProgress);

                    // Move từ temp vào final
                    var tempPath = Path.Combine(AppConfig.LectureExtractFolder, tempLectureId);
                    var finalPath = Path.Combine(AppConfig.LectureExtractFolder, lesson.id);

                    if (Directory.Exists(finalPath))
                        Directory.Delete(finalPath, true);

                    if (Directory.Exists(tempPath))
                        Directory.Move(tempPath, finalPath);

                    extractPath = finalPath;
                }
                else
                {
                    extractPath = await LectureService
                        .DownloadAndExtractZipAsync(offlineZip.url, lesson.id, progress, statsProgress);
                }
            });

            // =========================
            // 5️⃣ UPDATE UI SAU KHI XONG
            // =========================
            btnDownload.Text = "Đã tải";
            btnDownload.ForeColor = Color.Green;
            btnDownload.Enabled = false;
            if (lblSpeed != null)
            {
                lblSpeed.ForeColor = Color.Gray;
                lblSpeed.Visible = false;
            }
            if (lblUpdate != null)
            {
                lblUpdate.Text = "Vui lòng cập nhật giáo án mới";
                lblUpdate.Visible = false;
                lblUpdate.Enabled = true;
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
            // 6️⃣ MAP FILE VÀ VALIDATE
            // =========================
            LectureFiles files = _resourceService.MapLectureFiles(extractPath);

            // Validate: Chỉ lưu cache nếu file thực sự tồn tại
            string? validPdfPath = !string.IsNullOrEmpty(files.PdfPath) && File.Exists(files.PdfPath) ? files.PdfPath : null;
            string? validVideoPath = !string.IsNullOrEmpty(files.VideoPath) && File.Exists(files.VideoPath) ? files.VideoPath : null;
            string? validElearningPath = !string.IsNullOrEmpty(files.ElearningPath) && File.Exists(files.ElearningPath) ? files.ElearningPath : null;
            string? validPowerPointPath = !string.IsNullOrEmpty(files.PowerPointPath) && File.Exists(files.PowerPointPath) ? files.PowerPointPath : null;

            // Cảnh báo nếu không tìm thấy bất kỳ file nào
            if (string.IsNullOrEmpty(validPdfPath) && string.IsNullOrEmpty(validVideoPath) && 
                string.IsNullOrEmpty(validElearningPath) && string.IsNullOrEmpty(validPowerPointPath))
            {
                System.Diagnostics.Debug.WriteLine($"[Download] Warning: No valid files found in extracted path: {extractPath}");
            }

            // =========================
            // 7️⃣ LƯU CACHE
            // =========================
            LectureOfflineCacheService.Save(
                lesson.id,
                validPdfPath,
                validVideoPath,
                validElearningPath,
                validPowerPointPath,
                offlineZip.url
            );

            // =========================
            // 8️⃣ ENABLE OFFLINE BUTTON
            // =========================
            void EnableOfflineButton(Button btn, Action clickAction)
            {
                btn.Enabled = true;
                btn.ForeColor = Color.Blue;
                btn.FlatAppearance.BorderColor = Color.Blue;
                btn.FlatAppearance.BorderSize = 1;

                if (_offlineButtonHandlers.TryGetValue(btn, out var existingHandler))
                    btn.Click -= existingHandler;

                EventHandler handler = (s, e) => clickAction();
                _offlineButtonHandlers[btn] = handler;
                btn.Click += handler;
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

            if (!string.IsNullOrEmpty(files.PowerPointPath))
            {
                EnableOfflineButton(
                    btnPowerPointOff,
                    () => OpenLocal(files.PowerPointPath, lesson.title)
                );
            }

            if (isUpdate)
            {
                MessageBox.Show("Cập nhật giáo án thành công!");
            }
        }

        private Button CreateSimpleButton(string text, Color color)
        {
            return new Button
            {
                Text = text,
                Width = Scale(170),
                Height = Scale(34),
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", ScaleFont(9), FontStyle.Regular)
            };
        }

        private Button CreateSimpleGrayButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = Scale(120),
                Height = Scale(34),
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", ScaleFont(9), FontStyle.Regular)
            };
        }

        private int GetCardHeight(
            bool needsUpdate,
            int actionButtonHeight,
            int actionButtonSpacing,
            int downloadButtonHeight)
        {
            int imageColumnHeight = Scale(150);
            int infoColumnHeight = Scale(150);

            int offlineColumnHeight =
                Scale(24) + actionButtonSpacing +
                (actionButtonHeight * 4) + (actionButtonSpacing * 3);

            if (needsUpdate)
                offlineColumnHeight += Scale(20) + Scale(6);

            int deleteColumnHeight =
                actionButtonHeight + actionButtonSpacing + downloadButtonHeight;

            int contentHeight = Math.Max(
                Math.Max(imageColumnHeight, infoColumnHeight),
                Math.Max(offlineColumnHeight, deleteColumnHeight)
            );

            return contentHeight + Scale(32);
        }

        // sự kiện xóa
        private async void BtnDeleteOffline_Click(object? sender, EventArgs e)
        {
            if (sender is not Button btn) return;
            if (btn.Tag is not object[] data || data.Length < 6) return;

            var lesson = data[0] as LectureDto;
            var btnPdfOff = data[1] as Button;
            var btnVideoOff = data[2] as Button;
            var btnLessonOff = data[3] as Button;
            var btnPowerPointOff = data[4] as Button;
            var btnDownload = data[5] as Button;

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
            // This screen is hosted inside a fixed-pixel shell, so applying an
            // extra DeviceDpi multiplier here makes it visually over-scale.
            return value;
        }

        private float ScaleFont(float size)
        {
            return size;
        }

        private static void SetDoubleBuffered(Control control)
        {
            typeof(Control)
                .GetProperty(
                    "DoubleBuffered",
                    BindingFlags.Instance | BindingFlags.NonPublic
                )
                ?.SetValue(control, true, null);
        }

        private static string BuildLectureSignature(List<LectureDto> lectures)
        {
            return string.Join(
                "||",
                lectures.Select(lecture =>
                {
                    var resources = lecture.resources ?? new List<LectureResourceDto>();
                    var resourceSignature = string.Join(
                        "|",
                        resources
                            .OrderBy(r => r.type ?? string.Empty)
                            .ThenBy(r => r.source ?? string.Empty)
                            .ThenBy(r => r.url ?? string.Empty)
                            .Select(r => $"{r.type}:{r.source}:{r.url}")
                    );

                    return string.Join(
                        "::",
                        lecture.id ?? string.Empty,
                        lecture.code ?? string.Empty,
                        lecture.title ?? string.Empty,
                        resourceSignature
                    );
                })
            );
        }

        private void UpdateTitleLayout(Label lblTitle, Panel info, Label lblCode, Label lblSpeed)
        {
            if (lblTitle == null || info == null) return;

            int horizontalPadding = info.Padding.Horizontal;
            int availableWidth = Math.Max(10, info.ClientSize.Width - horizontalPadding);

            int bottomHeight = 0;
            if (lblCode != null) bottomHeight += lblCode.Height;
            if (lblSpeed != null && lblSpeed.Visible) bottomHeight += lblSpeed.Height;

            int availableHeight = Math.Max(Scale(24), info.ClientSize.Height - bottomHeight - info.Padding.Vertical);

            lblTitle.Width = availableWidth;
            lblTitle.MaximumSize = new Size(availableWidth, 0);
            var size = TextRenderer.MeasureText(
                lblTitle.Text,
                lblTitle.Font,
                new Size(availableWidth, availableHeight),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl
            );
            lblTitle.Height = Math.Min(availableHeight, size.Height);
        }
    }
}
