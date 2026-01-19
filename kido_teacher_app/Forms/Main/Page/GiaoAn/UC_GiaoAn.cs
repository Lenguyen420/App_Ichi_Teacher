using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Caching;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_GiaoAn : UserControl
    {
        private readonly Color[] hoverColors =
        {
            Color.FromArgb(0, 120, 215),
            Color.FromArgb(0, 180, 120),
            Color.FromArgb(255, 140, 0),
            Color.FromArgb(220, 53, 69),
            Color.FromArgb(111, 66, 193)
        };

        private const int BORDER_RADIUS = 14;
        private const int BORDER_THICKNESS = 6;
        private readonly string _courseId;
        public UC_GiaoAn(string courseId)
        {
            InitializeComponent();
            _courseId = courseId;

            this.Load += async (s, e) => await LoadClassesAsync();
        }

        private string GetClassImageFile(ClassDto c)
        {
            if (!string.IsNullOrEmpty(c.currentImage))
                return c.currentImage;

            if (!string.IsNullOrEmpty(c.avatarImage))
                return c.avatarImage;

            if (!string.IsNullOrEmpty(c.avatar))
                return c.avatar;

            // imageUrl là link tuyệt đối (fallback cuối)
            return c.imageUrl;
        }


        // ================= LOAD CARD TỪ API =================
        private async Task LoadClassesAsync()
        {
            try
            {
                flowGiaoAn.Controls.Clear();

                var classes = await ClassService.GetAllAsync();

                if (classes == null || classes.Count == 0)
                {
                    // ⭐ KHÔNG HIỂN THỊ GÌ, CHỈ ĐỂ TRỐNG
                    return;
                }

                // ⭐ DEBUG: Log số lượng classes
                System.Diagnostics.Debug.WriteLine($"[UC_GiaoAn] Loaded {classes.Count} classes");

                foreach (var c in classes)
                {
                    // ⭐ DEBUG: Log image fields
                    System.Diagnostics.Debug.WriteLine($"[Class {c.code}] currentImage={c.currentImage}, avatarImage={c.avatarImage}, avatar={c.avatar}");

                    Panel card = new Panel
                    {
                        BackColor = Color.White,
                        Size = new Size(240, 240),
                        Margin = new Padding(20),
                        Padding = new Padding(6),
                        Cursor = Cursors.Hand
                    };

                    card.Paint += ItemTemplate_Paint;
                    card.MouseEnter += Item_MouseEnter;
                    card.MouseLeave += Item_MouseLeave;
                    card.Click += Item_Click;

                    card.Tag = new CardTag
                    {
                        Data = c,
                        BorderColor = Color.LightGray,
                        OriginalSize = card.Size,
                        OriginalLocation = card.Location
                    };

                    PictureBox pic = new PictureBox
                    {
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.StretchImage,
                        Cursor = Cursors.Hand,
                    };

                    pic.MouseEnter += Item_MouseEnter;
                    pic.MouseLeave += Item_MouseLeave;
                    pic.Click += Item_Click;

                    var file = GetClassImageFile(c);

                    // Ảnh mặc định hiển thị ngay
                    pic.Image = Properties.Resources.coursedefault;

                    if (!string.IsNullOrEmpty(file))
                    {
                        // ⭐ Tải ảnh song song — dùng cache
                        _ = Task.Run(async () =>
                        {
                            Image img = null;

                            try
                            {
                                img = await ClassImageCacheService.GetOrDownloadImageAsync(c.id, file);
                            }
                            catch { }

                            if (img != null)
                            {
                                pic.Invoke(new Action(() =>
                                {
                                    pic.Image = img;
                                }));
                            }
                        });
                    }
                    else
                    {
                        pic.Image = Properties.Resources.coursedefault;
                    }


                    card.Controls.Add(pic);
                    flowGiaoAn.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] LoadClassesAsync: {ex.Message}");
            }
        }


        // ================= UTIL =================
        private Panel GetCardFromSender(object sender)
        {
            Control ctrl = sender as Control;

            while (ctrl != null && !(ctrl is Panel))
            {
                ctrl = ctrl.Parent;
            }

            return ctrl as Panel;
        }
        private Color GetRandomHoverColor()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            return hoverColors[rnd.Next(hoverColors.Length)];
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        // ================= HOVER =================
        private void Item_MouseEnter(object sender, EventArgs e)
        {
            var card = GetCardFromSender(sender);
            if (card?.Tag is not CardTag tag) return;

            tag.BorderColor = GetRandomHoverColor();
            card.Invalidate();
        }

        private void Item_MouseLeave(object sender, EventArgs e)
        {
            var card = GetCardFromSender(sender);
            if (card?.Tag is not CardTag tag) return;

            tag.BorderColor = Color.LightGray;
            card.Invalidate();
        }

        // ================= PAINT =================
        private void ItemTemplate_Paint(object sender, PaintEventArgs e)
        {
            var panel = sender as Panel;
            if (panel == null) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = panel.ClientRectangle;
            rect.Width -= 1;
            rect.Height -= 1;

            var tag = (CardTag)panel.Tag;

            using (GraphicsPath path = GetRoundedRect(rect, BORDER_RADIUS))
            using (Pen pen = new Pen(tag.BorderColor, BORDER_THICKNESS))
            {
                panel.Region = new Region(path);
                e.Graphics.DrawPath(pen, path);
            }
        }

        // ================= CLICK =================
        private void Item_Click(object sender, EventArgs e)
        {
            var card = GetCardFromSender(sender);
            if (card == null) return;

            if (card.Tag is not CardTag tag) return;
            if (tag.Data == null) return;

            Main_Form main = this.FindForm() as Main_Form;
            if (main == null) return;

            main.LoadUserControl(
    new UC_GiaoAnTheoThang(
        tag.Data.name,
        tag.Data.id,     // classId
        _courseId        // ⭐ lấy từ constructor ở trên
    )
);
        }

        class CardTag
        {
            public ClassDto Data { get; set; }
            public Color BorderColor { get; set; }

            // ⭐ LƯU KÍCH THƯỚC GỐC
            public Size OriginalSize { get; set; }

            // ⭐ LƯU VỊ TRÍ GỐC
            public Point OriginalLocation { get; set; }
        }
    }
}
