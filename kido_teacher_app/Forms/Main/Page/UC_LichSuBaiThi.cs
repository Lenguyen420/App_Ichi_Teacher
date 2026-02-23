using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_LichSuBaiThi : UserControl
    {
        private List<HistoryItem> historyData = new();

        public UC_LichSuBaiThi()
        {
            InitializeComponent();
            LoadFakeData();
            LoadFilterData();
            LoadHistory();
            btnSearch.Click += BtnSearch_Click;

            this.Load += (s, e) =>
            {
                ApplyRoundedCorner(panelTableWrapper, 15);
            };

            panelTableWrapper.Resize += (s, e) =>
            {
                ApplyRoundedCorner(panelTableWrapper, 15);
            };
        }

        private void ApplyRoundedCorner(Panel panel, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            path.StartFigure();

            path.AddArc(0, 0, radius, radius, 180, 90);
            path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
            path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
            path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);

            path.CloseFigure();
            panel.Region = new Region(path);
        }
        private void LoadFilterData()
        {
            cboClass.Items.Add("Tất cả");
            cboClass.Items.Add("Bài 1 - Alphabet");
            cboClass.Items.Add("Bài 2 - Numbers");
            cboClass.Items.Add("Bài 3 - Colors");
            cboClass.SelectedIndex = 0;
        }

        private void LoadFakeData()
        {
            historyData = new()
            {
                new("Bài 1 - Alphabet",1,DateTime.Now.AddDays(-5),8,"5 phút 20 giây"),
                new("Bài 1 - Alphabet",2,DateTime.Now.AddDays(-4),6,"4 phút 12 giây"),
                new("Bài 2 - Numbers",1,DateTime.Now.AddDays(-3),9,"3 phút 40 giây"),
                new("Bài 3 - Colors",1,DateTime.Now.AddDays(-2),4,"6 phút 10 giây"),
                new("Bài 3 - Colors",2,DateTime.Now.AddDays(-1),7,"4 phút 55 giây"),
            };
        }

        private void LoadHistory()
        {
            panelBodyTable.Controls.Clear();
            panelHeaderTable.Controls.Clear();

            panelHeaderTable.Controls.Add(CreateHeader());

            var filtered = historyData
                .OrderByDescending(x => x.Date)
                .ToList();

            for (int i = filtered.Count - 1; i >= 0; i--)
            {
                panelBodyTable.Controls.Add(CreateRow(filtered[i]));
            }
        }
        private Panel CreateHeader()
        {
            Panel header = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(240, 240, 240)
            };

           // header.Controls.Add(CreateCell("Hành động", 170, true));
            header.Controls.Add(CreateCell("Ngày làm", 120, true));
            header.Controls.Add(CreateCell("Thời gian", 150, true));
            header.Controls.Add(CreateCell("Số lần", 80, true));
            header.Controls.Add(CreateCell("Điểm", 70, true));
            header.Controls.Add(CreateCell("Tên bài thi", 250, true));

            return header;
        }

        private Panel CreateRow(HistoryItem item)
        {
            Panel row = new Panel
            {
                Height = 45,
                Dock = DockStyle.Top,
                BackColor = Color.White
            };

            row.MouseEnter += (s, e) =>
                row.BackColor = Color.FromArgb(245, 250, 255);

            row.MouseLeave += (s, e) =>
                row.BackColor = Color.White;

            // Action panel trước (Dock Right)
            FlowLayoutPanel actionPanel = new FlowLayoutPanel
            {
                Width = 170,
                Dock = DockStyle.Right
            };

            Button btnXem = new Button
            {
                Text = "XEM",
                Width = 70,
                Height = 28,
                FlatStyle = FlatStyle.Flat
            };

            Button btnLamLai = new Button
            {
                Text = "LÀM LẠI",
                Width = 80,
                Height = 28,
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnLamLai.FlatAppearance.BorderSize = 0;

            actionPanel.Controls.Add(btnXem);
            actionPanel.Controls.Add(btnLamLai);

            row.Controls.Add(actionPanel);
            row.Controls.Add(CreateCell(item.Date.ToString("dd/MM/yyyy"), 120, false));
            row.Controls.Add(CreateCell(item.Duration, 150, false));
            row.Controls.Add(CreateCell(item.Attempt.ToString(), 80, false));
            row.Controls.Add(CreateCell(item.Score.ToString(), 70, false, item.Score));
            row.Controls.Add(CreateCell(item.Title, 250, false));

            return row;
        }
        private Label CreateCell(string text, int width, bool isHeader, int score = 0)
        {
            Label lbl = new()
            {
                Text = text,
                Width = width,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10,
                        isHeader ? FontStyle.Bold : FontStyle.Regular)
            };

            if (!isHeader && score != 0)
                lbl.ForeColor = score >= 5 ? Color.Green : Color.Red;

            return lbl;
        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            LoadHistory();
        }
    }

    public class HistoryItem
    {
        public string Title { get; set; }
        public int Attempt { get; set; }
        public DateTime Date { get; set; }
        public int Score { get; set; }
        public string Duration { get; set; }

        public HistoryItem(string t, int a, DateTime d, int s, string du)
        {
            Title = t;
            Attempt = a;
            Date = d;
            Score = s;
            Duration = du;
        }
    }
}