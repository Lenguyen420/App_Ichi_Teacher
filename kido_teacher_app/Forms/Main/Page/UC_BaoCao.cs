using kido_teacher_app.Model;
using kido_teacher_app.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public class UC_BaoCao : UserControl
    {
        private ComboBox cboZone = null!;
        private ComboBox cboSchool = null!;
        private ComboBox cboGroup = null!;
        private ComboBox cboStudent = null!;
        private DateTimePicker dtFrom = null!;
        private DateTimePicker dtTo = null!;
        private Button btnView = null!;
        private Button btnExport = null!;
        private Button btnSelectAll = null!;
        private Label lblStatus = null!;
        private Label lblTotal = null!;
        private Label lblAverage = null!;
        private Label lblHighest = null!;
        private Label lblLatest = null!;
        private Label lblPage = null!;
        private Button btnPrev = null!;
        private Button btnNext = null!;
        private DataGridView dgvHistory = null!;
        private Label lblHistoryEmpty = null!;
        private Panel pnlTrend = null!;
        private Label lblTrendEmpty = null!;

        private readonly List<TrendPoint> trendPoints = new();
        private bool isBusy;
        private bool groupsLoaded;
        private bool suppressEvents;
        private bool reportLoaded;
        private int currentPage = 1;
        private int totalPages = 1;
        private int pageSize = 100;
        private string? activeZoneId;
        private string? activeSchoolId;
        private string? activeGroupId;
        private string? activeStudentId;

        public UC_BaoCao()
        {
            BuildUi();
            BindGroups(Array.Empty<StudentGroupNode>());
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Chọn nhóm để xem báo cáo.");
            dtTo.Value = DateTime.Today;
            dtFrom.Value = DateTime.Today.AddDays(-30);
            WireEvents();
        }

        private void BuildUi()
        {
            Dock = DockStyle.Fill;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Padding = new Padding(18) };
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 140));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Chào Mừng Bạn Đến Với KIDO",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            root.Controls.Add(new Label
            {
                Text = "Báo cáo học sinh",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(146, 208, 80),
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            var gb = new GroupBox
            {
                Text = "Bộ lọc báo cáo",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                Padding = new Padding(10, 14, 10, 10),
                Margin = new Padding(0, 4, 0, 0)
            };
            var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 2, Padding = new Padding(6, 16, 6, 6) };
            for (var i = 0; i < 7; i++) filters.ColumnStyles.Add(new ColumnStyle(i < 6 ? SizeType.Percent : SizeType.Absolute, i < 6 ? (100f / 6f) : 240));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            cboZone = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboSchool = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboGroup = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboStudent = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            dtFrom = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 10f) };
            dtTo = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 10f) };
            btnView = CreateButton("Xem báo cáo", true);
            btnExport = CreateButton("Xuất Excel", false);
            btnSelectAll = CreateButton("Tất cả", false);
            filters.Controls.Add(MakeField("Khu vực", cboZone), 0, 0);
            filters.Controls.Add(MakeField("Trường", cboSchool), 1, 0);
            filters.Controls.Add(MakeField("Nhóm/Lớp", cboGroup), 2, 0);
            filters.Controls.Add(MakeField("Học sinh", cboStudent), 3, 0);
            filters.Controls.Add(MakeField("Từ ngày", dtFrom), 4, 0);
            filters.Controls.Add(MakeField("Đến ngày", dtTo), 5, 0);
            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Margin = new Padding(6, 19, 0, 0) };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            actions.Controls.Add(btnView, 0, 0);
            actions.Controls.Add(btnExport, 1, 0);
            actions.Controls.Add(btnSelectAll, 2, 0);
            filters.Controls.Add(actions, 6, 0);
            var hint = new Label { Text = "Chỉ hiển thị học sinh thuộc các nhóm mà giáo viên đang quản lý.", Dock = DockStyle.Fill, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            filters.SetColumnSpan(hint, 7);
            filters.Controls.Add(hint, 0, 1);
            gb.Controls.Add(filters);
            root.Controls.Add(gb, 0, 2);

            var summary = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            for (var i = 0; i < 4; i++) summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            lblTotal = MakeMetric(summary, 0, "Tổng lần làm");
            lblAverage = MakeMetric(summary, 1, "Điểm trung bình");
            lblHighest = MakeMetric(summary, 2, "Điểm cao nhất");
            lblLatest = MakeMetric(summary, 3, "Lần gần nhất");
            root.Controls.Add(summary, 0, 3);

            lblStatus = new Label { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft };
            root.Controls.Add(lblStatus, 0, 4);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            var tabHistory = new TabPage("Lịch sử làm bài");
            var tabTrend = new TabPage("Xu hướng điểm");
            tabs.TabPages.Add(tabHistory);
            tabs.TabPages.Add(tabTrend);
            root.Controls.Add(tabs, 0, 5);

            var historyRoot = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Padding = new Padding(8) };
            historyRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            historyRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            dgvHistory = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            foreach (var c in new[] { "Thời gian", "Bài học", "Điểm", "Thời lượng", "Kết quả" }) dgvHistory.Columns.Add(Guid.NewGuid().ToString("N"), c);
            var historyHost = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            lblHistoryEmpty = new Label { Text = "Chưa có dữ liệu lịch sử trong khoảng thời gian đã chọn.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DimGray };
            historyHost.Controls.Add(dgvHistory);
            historyHost.Controls.Add(lblHistoryEmpty);
            lblHistoryEmpty.BringToFront();
            historyRoot.Controls.Add(historyHost, 0, 0);
            var pager = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            btnPrev = CreateButton("Trang trước", false);
            btnNext = CreateButton("Trang sau", false);
            lblPage = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pager.Controls.Add(btnPrev, 0, 0);
            pager.Controls.Add(lblPage, 1, 0);
            pager.Controls.Add(btnNext, 2, 0);
            historyRoot.Controls.Add(pager, 0, 1);
            tabHistory.Controls.Add(historyRoot);

            pnlTrend = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlTrend.Paint += DrawTrend;
            lblTrendEmpty = new Label { Text = "Chưa có dữ liệu xu hướng điểm.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DimGray };
            var trendHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            var trendBorder = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            trendBorder.Controls.Add(pnlTrend);
            trendBorder.Controls.Add(lblTrendEmpty);
            lblTrendEmpty.BringToFront();
            trendHost.Controls.Add(trendBorder);
            tabTrend.Controls.Add(trendHost);
        }

        private void WireEvents()
        {
            Load += async (_, __) => await EnsureGroupsLoadedAsync();
            VisibleChanged += async (_, __) => await EnsureGroupsLoadedAsync();
            cboZone.SelectedIndexChanged += async (_, __) => await OnZoneChangedAsync();
            cboSchool.SelectedIndexChanged += async (_, __) => await OnSchoolChangedAsync();
            cboGroup.SelectedIndexChanged += async (_, __) => await OnGroupChangedAsync();
            cboStudent.SelectedIndexChanged += (_, __) => OnStudentChanged();
            btnView.Click += async (_, __) => await ViewReportAsync();
            btnPrev.Click += async (_, __) => await ChangePageAsync(-1);
            btnNext.Click += async (_, __) => await ChangePageAsync(1);
            btnExport.Click += async (_, __) => await ExportReportAsync();
            btnSelectAll.Click += async (_, __) => await ExportSchoolStatSheetAsync();
        }

        private async Task ExportReportAsync()
        {
            if (!HasSelectedGroup())
            {
                SetStatus("Cần chọn nhóm để xuất báo cáo.", Color.Firebrick);
                return;
            }
            if (dtFrom.Value.Date > dtTo.Value.Date)
            {
                SetStatus("Khoảng ngày không hợp lệ: 'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.", Color.Firebrick);
                return;
            }
            SetBusy(true, "Đang tạo file Excel...", Color.DarkOrange);
            try
            {
                var groupId = GetSelectedValue(cboGroup)!;
                var excelData = await AttemptReportService.ExportClassSheetAsync(
                    groupId,
                    examSetId: null,  // Optional filters
                    questionBankId: null,
                    fromDate: dtFrom.Value.Date,
                    toDate: dtTo.Value.Date);
                SaveExcelFile(excelData, groupId);
            }
            catch (Exception ex)
            {
                SetStatus(BuildErrorMessage(ex, "Không thể xuất báo cáo."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private void SaveExcelFile(byte[] excelData, string groupId)
        {
            using var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*",
                DefaultExt = ".xlsx",
                FileName = $"BaoCao_{groupId}_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    System.IO.File.WriteAllBytes(saveDialog.FileName, excelData);
                    SetStatus($"Xuất báo cáo thành công: {saveDialog.FileName}", Color.Green);
                    MessageBox.Show($"File báo cáo đã được lưu:\n{saveDialog.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    SetStatus($"Lỗi khi lưu file: {ex.Message}", Color.Firebrick);
                    MessageBox.Show($"Không thể lưu file:\n{ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async Task EnsureGroupsLoadedAsync()
        {
            if (!Visible || groupsLoaded || isBusy) return;
            SetBusy(true, "Đang tải danh sách khu vực, trường và nhóm...", Color.DarkOrange);
            try
            {
                var zonePayload = await AttemptReportService.GetZoneDetailAsync();
                BindZonesFromDetail(zonePayload);
                groupsLoaded = true;
                SetStatus("Chọn khu vực, trường và nhóm để xem báo cáo.", Color.FromArgb(55, 55, 55));
            }
            catch (Exception ex)
            {
                BindZones(Array.Empty<ZoneDetailItem>());
                BindSchools(Array.Empty<SchoolNode>());
                BindGroups(Array.Empty<StudentGroupNode>());
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                SetStatus(BuildErrorMessage(ex, "Không tải được danh sách khu vực, trường và nhóm."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private async Task OnZoneChangedAsync()
        {
            if (suppressEvents || isBusy) return;
            activeZoneId = GetSelectedValue(cboZone);
            activeSchoolId = null;
            activeGroupId = null;
            activeStudentId = null;
            
            if (!HasSelectedZone())
            {
                BindSchools(Array.Empty<SchoolNode>());
                BindGroups(Array.Empty<StudentGroupNode>());
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                ResetReport("Chọn khu vực, trường và nhóm để xem báo cáo.");
                UpdateActions();
                return;
            }

            // Filter schools by selected zone
            var zonePayload = await AttemptReportService.GetZoneDetailAsync();
            var selectedZone = zonePayload.data.FirstOrDefault(z => z.zone.id == activeZoneId);
            var schools = selectedZone?.schools ?? new List<SchoolNode>();
            BindSchools(schools);
            BindGroups(Array.Empty<StudentGroupNode>());
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Chọn trường và nhóm để xem báo cáo.");
            UpdateActions();
        }

        private async Task OnSchoolChangedAsync()
        {
            if (suppressEvents || isBusy) return;
            activeSchoolId = GetSelectedValue(cboSchool);
            activeGroupId = null;
            activeStudentId = null;
            
            if (!HasSelectedSchool())
            {
                BindGroups(Array.Empty<StudentGroupNode>());
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                ResetReport("Chọn trường và nhóm để xem báo cáo.");
                UpdateActions();
                return;
            }

            // Filter groups by selected school
            var zonePayload = await AttemptReportService.GetZoneDetailAsync();
            var selectedSchool = zonePayload.data
                .SelectMany(z => z.schools)
                .FirstOrDefault(s => s.id == activeSchoolId);
            var studentGroups = selectedSchool?.studentGroups ?? new List<StudentGroupNode>();
            BindGroups(studentGroups);
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Chọn nhóm để xem báo cáo hoặc học sinh để xem báo cáo cá nhân.");
            UpdateActions();
        }

        private async Task OnGroupChangedAsync()
        {
            if (suppressEvents || isBusy) return;
            activeGroupId = GetSelectedValue(cboGroup);
            activeStudentId = null;
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Nhấn 'Xem báo cáo' để xem báo cáo nhóm hoặc chọn học sinh để xem báo cáo cá nhân.");
            if (!HasSelectedGroup()) { UpdateActions(); return; }
            SetBusy(true, "Đang tải danh sách học sinh...", Color.DarkOrange);
            try
            {
                BindStudents(await AttemptReportService.GetStudentsByGroupAsync(activeGroupId!));
                SetStatus("Nhấn 'Xem báo cáo' để xem báo cáo nhóm hoặc chọn học sinh để xem báo cáo cá nhân.", Color.FromArgb(55, 55, 55));
            }
            catch (Exception ex)
            {
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                SetStatus(BuildErrorMessage(ex, "Không tải được danh sách học sinh."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private void OnStudentChanged()
        {
            if (suppressEvents) return;
            activeStudentId = null;
            ResetReport(HasSelectedStudent() ? "Nhấn 'Xem báo cáo' để tải dữ liệu." : "Nhấn 'Xem báo cáo' để xem báo cáo nhóm hoặc chọn học sinh để xem báo cáo cá nhân.");
            UpdateActions();
        }

        private async Task ViewReportAsync()
        {
            if (!HasSelectedGroup())
            {
                SetStatus("Cần chọn nhóm để xem báo cáo.", Color.Firebrick);
                return;
            }
            if (dtFrom.Value.Date > dtTo.Value.Date)
            {
                SetStatus("Khoảng ngày không hợp lệ: 'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.", Color.Firebrick);
                return;
            }
            activeGroupId = GetSelectedValue(cboGroup);
            activeStudentId = GetSelectedValue(cboStudent);
            await LoadReportAsync(1);
        }

        private async Task ChangePageAsync(int delta)
        {
            if (!reportLoaded || string.IsNullOrWhiteSpace(activeGroupId)) return;
            var next = currentPage + delta;
            if (next < 1 || next > totalPages) return;
            await LoadReportAsync(next);
        }

        private async Task LoadReportAsync(int page)
        {
            SetBusy(true, "Đang tải báo cáo học sinh...", Color.DarkOrange);
            try
            {
                ApplyReport(await AttemptReportService.GetStudentReportAsync(
                    zoneId: activeZoneId,
                    schoolId: activeSchoolId,
                    groupId: activeGroupId,
                    studentId: activeStudentId,
                    fromDate: dtFrom.Value.Date,
                    toDate: dtTo.Value.Date,
                    page: page,
                    limit: pageSize));
            }
            catch (Exception ex)
            {
                reportLoaded = false;
                SetStatus(BuildErrorMessage(ex, "Không tải được báo cáo học sinh."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private void ApplyReport(StudentAttemptReportDto report)
        {
            reportLoaded = true;
            currentPage = Math.Max(1, report.page);
            pageSize = Math.Max(1, report.limit);
            totalPages = Math.Max(1, (int)Math.Ceiling(report.total / (double)pageSize));
            lblTotal.Text = (report.summary?.totalAttempts ?? 0).ToString(CultureInfo.InvariantCulture);
            lblAverage.Text = FormatScore(report.summary?.averageScore);
            lblHighest.Text = FormatScore(report.summary?.highestScore);
            lblLatest.Text = FormatDateTime(report.summary?.latestAttemptAt);
            dgvHistory.Rows.Clear();
            foreach (var x in report.attempts ?? new List<AttemptHistoryDto>())
                dgvHistory.Rows.Add(FormatDateTime(x.submittedAt ?? x.startedAt), AttemptTitle(x), FormatScore(x.score), FormatDuration(x.startedAt, x.submittedAt), AttemptStatus(x.status));
            lblHistoryEmpty.Visible = dgvHistory.Rows.Count == 0;
            trendPoints.Clear();
            foreach (var x in (report.trend ?? new List<AttemptReportTrendDto>()).Where(x => x.averageScore.HasValue || x.highestScore.HasValue).OrderBy(x => x.date ?? DateTime.MinValue))
                trendPoints.Add(new TrendPoint(FormatTrendDate(x.date), (float)(x.averageScore ?? x.highestScore ?? 0)));
            lblTrendEmpty.Visible = trendPoints.Count == 0;
            pnlTrend.Invalidate();
            lblPage.Text = $"Trang {currentPage}/{totalPages}";
            var hasData = (report.summary?.totalAttempts ?? 0) > 0 || dgvHistory.Rows.Count > 0 || trendPoints.Count > 0;
            SetStatus(hasData ? $"Đã tải báo cáo cho {report.student?.fullName ?? "học sinh đã chọn"}." : "Không có dữ liệu trong khoảng thời gian đã chọn.", hasData ? Color.FromArgb(55, 55, 55) : Color.DimGray);
            UpdateActions();
        }

        private void ResetReport(string message)
        {
            reportLoaded = false;
            currentPage = 1;
            totalPages = 1;
            lblTotal.Text = "-";
            lblAverage.Text = "-";
            lblHighest.Text = "-";
            lblLatest.Text = "-";
            dgvHistory?.Rows.Clear();
            lblHistoryEmpty?.BringToFront();
            if (lblHistoryEmpty != null) lblHistoryEmpty.Visible = true;
            trendPoints.Clear();
            if (lblTrendEmpty != null) lblTrendEmpty.Visible = true;
            pnlTrend?.Invalidate();
            if (lblPage != null) lblPage.Text = "Trang 1/1";
            SetStatus(message, Color.FromArgb(55, 55, 55));
            UpdateActions();
        }

        private void SetBusy(bool busy, string? message = null, Color? color = null)
        {
            isBusy = busy;
            cboGroup.Enabled = !busy;
            cboStudent.Enabled = !busy && HasSelectedGroup();
            dtFrom.Enabled = !busy;
            dtTo.Enabled = !busy;
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            if (message != null) SetStatus(message, color ?? Color.DarkOrange);
            UpdateActions();
        }

        private void UpdateActions()
        {
            btnView.Enabled = !isBusy && HasSelectedGroup();
            btnExport.Enabled = !isBusy && HasSelectedGroup();
            btnSelectAll.Enabled = !isBusy && HasSelectedSchool();
            btnPrev.Enabled = !isBusy && reportLoaded && currentPage > 1;
            btnNext.Enabled = !isBusy && reportLoaded && currentPage < totalPages;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private void BindGroups(IEnumerable<StudentGroupNode> groups)
        {
            suppressEvents = true;
            cboGroup.DataSource = BuildOptions("Chọn nhóm", groups.OrderBy(x => x.name).Select(x => new ComboItem(x.id, x.name)).ToList());
            cboGroup.DisplayMember = nameof(ComboItem.Text);
            cboGroup.ValueMember = nameof(ComboItem.Value);
            cboGroup.SelectedIndex = 0;
            suppressEvents = false;
        }

        private void BindStudents(IEnumerable<AttemptReportStudentDto> students)
        {
            suppressEvents = true;
            cboStudent.DataSource = BuildOptions("All", students.OrderBy(x => x.fullName).Select(x => new ComboItem(x.id, string.IsNullOrWhiteSpace(x.code) ? x.fullName : $"{x.fullName} ({x.code})")).ToList());
            cboStudent.DisplayMember = nameof(ComboItem.Text);
            cboStudent.ValueMember = nameof(ComboItem.Value);
            cboStudent.SelectedIndex = 0;
            suppressEvents = false;
        }

        private static List<ComboItem> BuildOptions(string placeholder, List<ComboItem> items)
        {
            items.Insert(0, new ComboItem(null, placeholder));
            return items;
        }

        private static Button CreateButton(string text, bool primary)
        {
            var button = new Button { Text = text, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, Margin = new Padding(6, 0, 0, 0), Height = 34, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
            button.FlatAppearance.BorderSize = primary ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.Silver;
            button.BackColor = primary ? Color.FromArgb(82, 171, 63) : Color.FromArgb(245, 245, 245);
            button.ForeColor = primary ? Color.White : Color.FromArgb(55, 55, 55);
            return button;
        }

        private static Control MakeField(string label, Control input)
        {
            input.Margin = new Padding(0);
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(6, 0, 6, 0) };
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            panel.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9.5f) }, 0, 0);
            panel.Controls.Add(input, 0, 1);
            return panel;
        }

        private static Label MakeMetric(TableLayoutPanel host, int column, string title)
        {
            var card = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, column < 3 ? 10 : 0, 0), Padding = new Padding(14, 10, 14, 10), BorderStyle = BorderStyle.FixedSingle, BackColor = Color.FromArgb(251, 252, 254) };
            card.Controls.Add(new Label { Text = title, Dock = DockStyle.Top, Height = 24, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft });
            var value = new Label { Text = "-", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = Color.FromArgb(43, 87, 151), TextAlign = ContentAlignment.MiddleLeft };
            card.Controls.Add(value);
            value.BringToFront();
            host.Controls.Add(card, column, 0);
            return value;
        }

        private void DrawTrend(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlTrend.ClientRectangle;
            if (rect.Width < 60 || rect.Height < 60 || trendPoints.Count == 0) return;
            var plot = new Rectangle(44, 18, Math.Max(10, rect.Width - 64), Math.Max(10, rect.Height - 48));
            using var axisPen = new Pen(Color.Silver);
            using var gridPen = new Pen(Color.FromArgb(235, 235, 235));
            using var linePen = new Pen(Color.FromArgb(72, 149, 239), 2.5f);
            using var pointBrush = new SolidBrush(Color.FromArgb(72, 149, 239));
            using var labelBrush = new SolidBrush(Color.FromArgb(90, 90, 90));
            using var font = new Font("Segoe UI", 8.5f);
            for (var i = 0; i <= 5; i++)
            {
                var y = plot.Top + plot.Height * i / 5;
                g.DrawLine(gridPen, plot.Left, y, plot.Right, y);
                g.DrawString((10 - 2 * i).ToString(CultureInfo.InvariantCulture), font, labelBrush, 8, y - 6);
            }
            g.DrawRectangle(axisPen, plot);
            var points = new PointF[trendPoints.Count];
            for (var i = 0; i < trendPoints.Count; i++)
            {
                var xRatio = trendPoints.Count == 1 ? 0.5f : (float)i / (trendPoints.Count - 1);
                var yRatio = 1f - Math.Clamp(trendPoints[i].Score, 0f, 10f) / 10f;
                points[i] = new PointF(plot.Left + plot.Width * xRatio, plot.Top + plot.Height * yRatio);
            }
            if (points.Length > 1) g.DrawLines(linePen, points);
            foreach (var p in points) g.FillEllipse(pointBrush, p.X - 3.5f, p.Y - 3.5f, 7, 7);
            g.DrawString(trendPoints[0].Label, font, labelBrush, points[0].X - 12, plot.Bottom + 6);
            if (trendPoints.Count > 1) g.DrawString(trendPoints[^1].Label, font, labelBrush, points[^1].X - 12, plot.Bottom + 6);
        }

        /// <summary>
        /// Xuất file Excel thống kê điểm toàn trường theo lớp/nhóm.
        /// </summary>
        private async Task ExportSchoolStatSheetAsync()
        {
            if (!HasSelectedSchool())
            {
                SetStatus("Cần chọn trường để xuất báo cáo toàn trường.", Color.Firebrick);
                return;
            }

            if (dtFrom.Value.Date > dtTo.Value.Date)
            {
                SetStatus("Khoảng ngày không hợp lệ: 'Từ ngày' phải nhỏ hơn hoặc bằng 'Đến ngày'.", Color.Firebrick);
                return;
            }

            var schoolId = GetSelectedValue(cboSchool)!;
            var schoolName = (cboSchool.SelectedItem as ComboItem)?.Text ?? "Trường";
            var fromDate = dtFrom.Value.Date;
            var toDate = dtTo.Value.Date;

            SetBusy(true, $"Đang tạo file Excel thống kê cho {schoolName}...", Color.DarkOrange);
            try
            {
                var excelData = await AttemptReportService.ExportSchoolStatSheetAsync(
                    schoolId,
                    examSetId: null,
                    questionBankId: null,
                    fromDate: fromDate,
                    toDate: toDate);
                SaveExcelFile(excelData, $"school-{schoolName}");
            }
            catch (Exception ex)
            {
                SetStatus(BuildErrorMessage(ex, "Không thể xuất báo cáo toàn trường."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private void BindZonesFromDetail(ZoneDetailPayload payload)
        {
            suppressEvents = true;
            var zones = payload.data
                .Select(z => new ComboItem(z.zone.id, z.zone.name))
                .OrderBy(x => x.Text)
                .ToList();
            cboZone.DataSource = BuildOptions("Chọn khu vực", zones);
            cboZone.DisplayMember = nameof(ComboItem.Text);
            cboZone.ValueMember = nameof(ComboItem.Value);
            cboZone.SelectedIndex = 0;
            suppressEvents = false;
        }

        private void BindZones(IEnumerable<ZoneDetailItem> zones)
        {
            suppressEvents = true;
            var zoneItems = zones
                .Select(z => new ComboItem(z.zone.id, z.zone.name))
                .OrderBy(x => x.Text)
                .ToList();
            cboZone.DataSource = BuildOptions("Chọn khu vực", zoneItems);
            cboZone.DisplayMember = nameof(ComboItem.Text);
            cboZone.ValueMember = nameof(ComboItem.Value);
            cboZone.SelectedIndex = 0;
            suppressEvents = false;
        }

        private void BindSchools(IEnumerable<SchoolNode> schools)
        {
            suppressEvents = true;
            var schoolItems = schools
                .Select(s => new ComboItem(s.id, s.name))
                .OrderBy(x => x.Text)
                .ToList();
            cboSchool.DataSource = BuildOptions("Chọn trường", schoolItems);
            cboSchool.DisplayMember = nameof(ComboItem.Text);
            cboSchool.ValueMember = nameof(ComboItem.Value);
            cboSchool.SelectedIndex = 0;
            suppressEvents = false;
        }

        private bool HasSelectedGroup() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboGroup));
        private bool HasSelectedStudent() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboStudent));
        private bool HasSelectedSchool() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboSchool));
        private bool HasSelectedZone() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboZone));
        private static string? GetSelectedValue(ComboBox combo) => (combo.SelectedItem as ComboItem)?.Value;
        private static string AttemptTitle(AttemptHistoryDto x) => !string.IsNullOrWhiteSpace(x.questionBankName) && !string.IsNullOrWhiteSpace(x.examSetName) ? $"{x.questionBankName} / {x.examSetName}" : (!string.IsNullOrWhiteSpace(x.questionBankName) ? x.questionBankName : (!string.IsNullOrWhiteSpace(x.examSetName) ? x.examSetName : "-"));
        private static string AttemptStatus(string? status) => status?.ToUpperInvariant() switch { "SUBMITTED" => "Đã nộp", "IN_PROGRESS" => "Đang làm", "EXPIRED" => "Hết hạn", _ => string.IsNullOrWhiteSpace(status) ? "-" : status };
        private static string FormatScore(double? score) => score.HasValue ? score.Value.ToString("0.0", CultureInfo.InvariantCulture) : "-";
        private static string FormatTrendDate(DateTime? value) => value.HasValue ? Normalize(value.Value).ToString("dd/MM", CultureInfo.InvariantCulture) : "-";
        private static string FormatDateTime(DateTime? value) => value.HasValue ? Normalize(value.Value).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : "-";
        private static string FormatDuration(DateTime? startedAt, DateTime? submittedAt) { if (!startedAt.HasValue || !submittedAt.HasValue) return "-"; var span = Normalize(submittedAt.Value) - Normalize(startedAt.Value); return span.TotalHours >= 1 ? $"{(int)span.TotalHours}h {span.Minutes:D2}m" : (span.TotalSeconds < 0 ? "-" : $"{span.Minutes:D2}m {span.Seconds:D2}s"); }
        private static DateTime Normalize(DateTime value) => value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        private static string BuildErrorMessage(Exception ex, string fallback) => ex is UnauthorizedAccessException ? "Bạn chưa đăng nhập hoặc token không hợp lệ." : ex is AttemptReportApiException api ? api.StatusCode switch { HttpStatusCode.BadRequest => string.IsNullOrWhiteSpace(api.Message) ? "Bộ lọc báo cáo không hợp lệ." : api.Message, HttpStatusCode.Forbidden => "Bạn không có quyền xem báo cáo cho nhóm này.", HttpStatusCode.NotFound => "Không tìm thấy nhóm, học sinh hoặc dữ liệu báo cáo.", _ => string.IsNullOrWhiteSpace(api.Message) ? fallback : api.Message } : IsNetworkException(ex) ? "Không thể kết nối đến máy chủ báo cáo." : fallback;
        private static bool IsNetworkException(Exception ex) => ex is System.Net.Http.HttpRequestException || ex is TaskCanceledException || ex is System.Net.Sockets.SocketException || (ex.InnerException != null && IsNetworkException(ex.InnerException));

        private sealed record ComboItem(string? Value, string Text);
        private sealed record TrendPoint(string Label, float Score);
    }
}
