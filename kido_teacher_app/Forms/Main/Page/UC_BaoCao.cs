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
        private ComboBox cboGroup = null!;
        private ComboBox cboStudent = null!;
        private DateTimePicker dtFrom = null!;
        private DateTimePicker dtTo = null!;
        private Button btnView = null!;
        private Button btnExport = null!;
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
        private int pageSize = 10;
        private string? activeGroupId;
        private string? activeStudentId;

        public UC_BaoCao()
        {
            BuildUi();
            BindGroups(Array.Empty<AttemptReportGroupDto>());
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Chon nhom va hoc sinh de xem bao cao.");
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
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 132));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 94));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            root.Controls.Add(new Label
            {
                Text = "Chao Mung Ban Den Voi KIDO",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(230, 230, 230),
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            root.Controls.Add(new Label
            {
                Text = "Bao cao hoc sinh",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                BackColor = Color.FromArgb(146, 208, 80),
                Padding = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 1);

            var gb = new GroupBox { Text = "Bo loc bao cao", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f), Padding = new Padding(10, 8, 10, 10) };
            var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 5, RowCount = 2, Padding = new Padding(6, 12, 6, 6) };
            for (var i = 0; i < 5; i++) filters.ColumnStyles.Add(new ColumnStyle(i < 4 ? SizeType.Percent : SizeType.Absolute, i < 4 ? 25 : 240));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
            filters.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            cboGroup = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            cboStudent = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10f) };
            dtFrom = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 10f) };
            dtTo = new DateTimePicker { Dock = DockStyle.Fill, Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy", Font = new Font("Segoe UI", 10f) };
            btnView = CreateButton("Xem bao cao", true);
            btnExport = CreateButton("Xuat Excel", false);
            filters.Controls.Add(MakeField("Nhom", cboGroup), 0, 0);
            filters.Controls.Add(MakeField("Hoc sinh", cboStudent), 1, 0);
            filters.Controls.Add(MakeField("Tu ngay", dtFrom), 2, 0);
            filters.Controls.Add(MakeField("Den ngay", dtTo), 3, 0);
            var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = new Padding(6, 19, 0, 0) };
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            actions.Controls.Add(btnView, 0, 0);
            actions.Controls.Add(btnExport, 1, 0);
            filters.Controls.Add(actions, 4, 0);
            var hint = new Label { Text = "Chi hien thi hoc sinh thuoc cac nhom ma giao vien dang quan ly.", Dock = DockStyle.Fill, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0) };
            filters.SetColumnSpan(hint, 5);
            filters.Controls.Add(hint, 0, 1);
            gb.Controls.Add(filters);
            root.Controls.Add(gb, 0, 2);

            var summary = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            for (var i = 0; i < 4; i++) summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            lblTotal = MakeMetric(summary, 0, "Tong lan lam");
            lblAverage = MakeMetric(summary, 1, "Diem trung binh");
            lblHighest = MakeMetric(summary, 2, "Diem cao nhat");
            lblLatest = MakeMetric(summary, 3, "Lan gan nhat");
            root.Controls.Add(summary, 0, 3);

            lblStatus = new Label { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10, 0, 0, 0), TextAlign = ContentAlignment.MiddleLeft };
            root.Controls.Add(lblStatus, 0, 4);

            var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };
            var tabHistory = new TabPage("Lich su lam bai");
            var tabTrend = new TabPage("Xu huong diem");
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
            foreach (var c in new[] { "Thoi gian", "Bai hoc", "Diem", "Thoi luong", "Ket qua" }) dgvHistory.Columns.Add(Guid.NewGuid().ToString("N"), c);
            var historyHost = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle };
            lblHistoryEmpty = new Label { Text = "Chua co du lieu lich su trong khoang thoi gian da chon.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DimGray };
            historyHost.Controls.Add(dgvHistory);
            historyHost.Controls.Add(lblHistoryEmpty);
            lblHistoryEmpty.BringToFront();
            historyRoot.Controls.Add(historyHost, 0, 0);
            var pager = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pager.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            btnPrev = CreateButton("Trang truoc", false);
            btnNext = CreateButton("Trang sau", false);
            lblPage = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            pager.Controls.Add(btnPrev, 0, 0);
            pager.Controls.Add(lblPage, 1, 0);
            pager.Controls.Add(btnNext, 2, 0);
            historyRoot.Controls.Add(pager, 0, 1);
            tabHistory.Controls.Add(historyRoot);

            pnlTrend = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pnlTrend.Paint += DrawTrend;
            lblTrendEmpty = new Label { Text = "Chua co du lieu xu huong diem.", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Color.DimGray };
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
            cboGroup.SelectedIndexChanged += async (_, __) => await OnGroupChangedAsync();
            cboStudent.SelectedIndexChanged += (_, __) => OnStudentChanged();
            btnView.Click += async (_, __) => await ViewReportAsync();
            btnPrev.Click += async (_, __) => await ChangePageAsync(-1);
            btnNext.Click += async (_, __) => await ChangePageAsync(1);
            btnExport.Click += (_, __) => MessageBox.Show("Backend chua ho tro xuat Excel cho man hinh bao cao nay.", "Thong bao", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async Task EnsureGroupsLoadedAsync()
        {
            if (!Visible || groupsLoaded || isBusy) return;
            SetBusy(true, "Dang tai danh sach nhom...", Color.DarkOrange);
            try
            {
                BindGroups(await AttemptReportService.GetGroupsAsync());
                groupsLoaded = true;
                SetStatus(HasSelectedGroup() ? "Nhan chon hoc sinh de xem bao cao." : "Chon nhom va hoc sinh de xem bao cao.", Color.FromArgb(55, 55, 55));
            }
            catch (Exception ex)
            {
                BindGroups(Array.Empty<AttemptReportGroupDto>());
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                SetStatus(BuildErrorMessage(ex, "Khong tai duoc danh sach nhom."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private async Task OnGroupChangedAsync()
        {
            if (suppressEvents || isBusy) return;
            activeGroupId = null;
            activeStudentId = null;
            BindStudents(Array.Empty<AttemptReportStudentDto>());
            ResetReport("Chon hoc sinh de xem bao cao.");
            if (!HasSelectedGroup()) { UpdateActions(); return; }
            SetBusy(true, "Dang tai danh sach hoc sinh...", Color.DarkOrange);
            try
            {
                BindStudents(await AttemptReportService.GetStudentsByGroupAsync(GetSelectedValue(cboGroup)!));
                SetStatus("Nhan 'Xem bao cao' sau khi chon hoc sinh.", Color.FromArgb(55, 55, 55));
            }
            catch (Exception ex)
            {
                BindStudents(Array.Empty<AttemptReportStudentDto>());
                SetStatus(BuildErrorMessage(ex, "Khong tai duoc danh sach hoc sinh."), Color.Firebrick);
            }
            finally { SetBusy(false); }
        }

        private void OnStudentChanged()
        {
            if (suppressEvents) return;
            activeStudentId = null;
            ResetReport(HasSelectedStudent() ? "Nhan 'Xem bao cao' de tai du lieu." : "Chon hoc sinh de xem bao cao.");
            UpdateActions();
        }

        private async Task ViewReportAsync()
        {
            if (!HasSelectedGroup() || !HasSelectedStudent())
            {
                SetStatus("Can chon day du nhom va hoc sinh.", Color.Firebrick);
                return;
            }
            if (dtFrom.Value.Date > dtTo.Value.Date)
            {
                SetStatus("Khoang ngay khong hop le: 'Tu ngay' phai nho hon hoac bang 'Den ngay'.", Color.Firebrick);
                return;
            }
            activeGroupId = GetSelectedValue(cboGroup);
            activeStudentId = GetSelectedValue(cboStudent);
            await LoadReportAsync(1);
        }

        private async Task ChangePageAsync(int delta)
        {
            if (!reportLoaded || string.IsNullOrWhiteSpace(activeGroupId) || string.IsNullOrWhiteSpace(activeStudentId)) return;
            var next = currentPage + delta;
            if (next < 1 || next > totalPages) return;
            await LoadReportAsync(next);
        }

        private async Task LoadReportAsync(int page)
        {
            SetBusy(true, "Dang tai bao cao hoc sinh...", Color.DarkOrange);
            try
            {
                ApplyReport(await AttemptReportService.GetStudentReportAsync(activeGroupId!, activeStudentId!, dtFrom.Value.Date, dtTo.Value.Date, page, pageSize));
            }
            catch (Exception ex)
            {
                reportLoaded = false;
                SetStatus(BuildErrorMessage(ex, "Khong tai duoc bao cao hoc sinh."), Color.Firebrick);
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
            SetStatus(hasData ? $"Da tai bao cao cho {report.student?.fullName ?? "hoc sinh da chon"}." : "Khong co du lieu trong khoang thoi gian da chon.", hasData ? Color.FromArgb(55, 55, 55) : Color.DimGray);
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
            btnView.Enabled = !isBusy && HasSelectedGroup() && HasSelectedStudent();
            btnExport.Enabled = !isBusy;
            btnPrev.Enabled = !isBusy && reportLoaded && currentPage > 1;
            btnNext.Enabled = !isBusy && reportLoaded && currentPage < totalPages;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }

        private void BindGroups(IEnumerable<AttemptReportGroupDto> groups)
        {
            suppressEvents = true;
            cboGroup.DataSource = BuildOptions("Chon nhom", groups.OrderBy(x => x.name).Select(x => new ComboItem(x.id, x.name)).ToList());
            cboGroup.DisplayMember = nameof(ComboItem.Text);
            cboGroup.ValueMember = nameof(ComboItem.Value);
            cboGroup.SelectedIndex = 0;
            suppressEvents = false;
        }

        private void BindStudents(IEnumerable<AttemptReportStudentDto> students)
        {
            suppressEvents = true;
            cboStudent.DataSource = BuildOptions("Chon hoc sinh", students.OrderBy(x => x.fullName).Select(x => new ComboItem(x.id, string.IsNullOrWhiteSpace(x.code) ? x.fullName : $"{x.fullName} ({x.code})")).ToList());
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
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
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

        private bool HasSelectedGroup() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboGroup));
        private bool HasSelectedStudent() => !string.IsNullOrWhiteSpace(GetSelectedValue(cboStudent));
        private static string? GetSelectedValue(ComboBox combo) => (combo.SelectedItem as ComboItem)?.Value;
        private static string AttemptTitle(AttemptHistoryDto x) => !string.IsNullOrWhiteSpace(x.questionBankName) && !string.IsNullOrWhiteSpace(x.examSetName) ? $"{x.questionBankName} / {x.examSetName}" : (!string.IsNullOrWhiteSpace(x.questionBankName) ? x.questionBankName : (!string.IsNullOrWhiteSpace(x.examSetName) ? x.examSetName : "-"));
        private static string AttemptStatus(string? status) => status?.ToUpperInvariant() switch { "SUBMITTED" => "Da nop", "IN_PROGRESS" => "Dang lam", "EXPIRED" => "Het han", _ => string.IsNullOrWhiteSpace(status) ? "-" : status };
        private static string FormatScore(double? score) => score.HasValue ? score.Value.ToString("0.0", CultureInfo.InvariantCulture) : "-";
        private static string FormatTrendDate(DateTime? value) => value.HasValue ? Normalize(value.Value).ToString("dd/MM", CultureInfo.InvariantCulture) : "-";
        private static string FormatDateTime(DateTime? value) => value.HasValue ? Normalize(value.Value).ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : "-";
        private static string FormatDuration(DateTime? startedAt, DateTime? submittedAt) { if (!startedAt.HasValue || !submittedAt.HasValue) return "-"; var span = Normalize(submittedAt.Value) - Normalize(startedAt.Value); return span.TotalHours >= 1 ? $"{(int)span.TotalHours}h {span.Minutes:D2}m" : (span.TotalSeconds < 0 ? "-" : $"{span.Minutes:D2}m {span.Seconds:D2}s"); }
        private static DateTime Normalize(DateTime value) => value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        private static string BuildErrorMessage(Exception ex, string fallback) => ex is UnauthorizedAccessException ? "Ban chua dang nhap hoac token khong hop le." : ex is AttemptReportApiException api ? api.StatusCode switch { HttpStatusCode.BadRequest => string.IsNullOrWhiteSpace(api.Message) ? "Bo loc bao cao khong hop le." : api.Message, HttpStatusCode.Forbidden => "Ban khong co quyen xem bao cao cho nhom nay.", HttpStatusCode.NotFound => "Khong tim thay nhom, hoc sinh hoac du lieu bao cao.", _ => string.IsNullOrWhiteSpace(api.Message) ? fallback : api.Message } : IsNetworkException(ex) ? "Khong the ket noi den may chu bao cao." : fallback;
        private static bool IsNetworkException(Exception ex) => ex is System.Net.Http.HttpRequestException || ex is TaskCanceledException || ex is System.Net.Sockets.SocketException || (ex.InnerException != null && IsNetworkException(ex.InnerException));

        private sealed record ComboItem(string? Value, string Text);
        private sealed record TrendPoint(string Label, float Score);
    }
}
