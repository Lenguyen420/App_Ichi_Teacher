
using kido_teacher_app.Forms.Main.Page.GioiThieu;
using kido_teacher_app.Forms.Main.Page.QuanLyNoiDung;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_GioiThieu : UserControl
    {
        // ===== SLIDESHOW =====
        private System.Windows.Forms.Timer slideTimer;
        private Image[] slideImages;
        private int currentSlideIndex = 0;
        public UC_GioiThieu()
        {
            InitializeComponent();
            InitSlideShow();
            Disposed += (_, __) => CleanupSlideShow();
        }
        private void InitSlideShow()
        {
            // Giảm RAM: tạo bản ảnh đã thu nhỏ theo kích thước hiển thị,
            // tránh giữ nhiều bitmap full-size cho slideshow.
            var targetSize = GetSlideTargetSize();

            slideImages = new Image[]
            {
                ResizeForSlide(Properties.Resources.slide10, targetSize.Width, targetSize.Height),
                ResizeForSlide(Properties.Resources.slide9, targetSize.Width, targetSize.Height),
                ResizeForSlide(Properties.Resources.slide11, targetSize.Width, targetSize.Height),
                ResizeForSlide(Properties.Resources.slide8, targetSize.Width, targetSize.Height)
            };

            currentSlideIndex = 0;
            banner.Image = slideImages[currentSlideIndex];

            slideTimer = new System.Windows.Forms.Timer();
            slideTimer.Interval = 4000; // 4 giây
            slideTimer.Tick += SlideTimer_Tick;
            slideTimer.Start(); // ⭐ BẮT BUỘC
        }
        private void EditImg_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var frm = new Form_ChinhSuaAnhGioiThieu())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);   // mở popup modal
            }
        }

        private void EditInfo_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (var frm = new Form_ChinhSuaThongTinCongTy())
            {
                frm.StartPosition = FormStartPosition.CenterParent;
                frm.ShowDialog(this);
            }
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            currentSlideIndex++;

            if (currentSlideIndex >= slideImages.Length)
                currentSlideIndex = 0;

            banner.Image = slideImages[currentSlideIndex];
        }

        private void CleanupSlideShow()
        {
            if (slideTimer != null)
            {
                slideTimer.Stop();
                slideTimer.Tick -= SlideTimer_Tick;
                slideTimer.Dispose();
                slideTimer = null;
            }

            // Chỉ dispose ảnh đã resize do control này tự tạo.
            if (slideImages != null)
            {
                foreach (var img in slideImages)
                {
                    img?.Dispose();
                }
            }

            if (banner != null)
                banner.Image = null;

            slideImages = Array.Empty<Image>();
        }

        private Size GetSlideTargetSize()
        {
            var screenWidth = Screen.PrimaryScreen?.WorkingArea.Width ?? 1920;
            var width = Math.Max(1280, screenWidth);
            var height = (int)(width * 9.0 / 16.0);
            return new Size(width, height);
        }

        private static Image ResizeForSlide(Image source, int maxWidth, int maxHeight)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var ratioX = (double)maxWidth / source.Width;
            var ratioY = (double)maxHeight / source.Height;
            var ratio = Math.Min(ratioX, ratioY);
            ratio = Math.Min(ratio, 1.0); // không phóng to ảnh nhỏ

            var targetWidth = Math.Max(1, (int)(source.Width * ratio));
            var targetHeight = Math.Max(1, (int)(source.Height * ratio));

            var resized = new Bitmap(targetWidth, targetHeight, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(resized))
            {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(source, 0, 0, targetWidth, targetHeight);
            }

            return resized;
        }
    }
}
