
using kido_teacher_app.Forms.Main.Page.GioiThieu;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace kido_teacher_app.Forms.Main.Page
{
    public partial class UC_GioiThieu : UserControl
    {
        // ===== SLIDESHOW =====
        private static readonly object SlideCacheLock = new object();
        private static Image[]? slideCache;
        private static Size slideCacheSize = Size.Empty;

        private System.Windows.Forms.Timer slideTimer;
        private Image[] slideImages;
        private int currentSlideIndex = 0;
        private bool isSlideRunning = true;
        public UC_GioiThieu()
        {
            InitializeComponent();
            InitSlideShow();
            Disposed += (_, __) => CleanupSlideShow();

        }
        private void InitSlideShow()
        {
            var targetSize = GetSlideTargetSize();
            slideImages = GetOrCreateSlideCache(targetSize);

            currentSlideIndex = 0;
            banner.Image = slideImages[currentSlideIndex];

            slideTimer = new System.Windows.Forms.Timer();
            slideTimer.Interval = 4000; // 4 giây
            slideTimer.Tick += SlideTimer_Tick;
            slideTimer.Start(); // ⭐ BẮT BUỘC

        }

        // xử lý logic dừng slide 
        private void BtnToggleSlide_Click(object sender, EventArgs e)
        {
            if (slideTimer == null) return;

            if (isSlideRunning)
            {
                slideTimer.Stop();
                isSlideRunning = false;

                ((Button)sender).Text = "Tiếp tục";
            }
            else
            {
                slideTimer.Start();
                isSlideRunning = true;

                ((Button)sender).Text = "Dừng";
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

            if (banner != null)
                banner.Image = null;

            slideImages = Array.Empty<Image>();
        }

        private Size GetSlideTargetSize()
        {
            var screenWidth = Screen.PrimaryScreen?.WorkingArea.Width ?? 1920;
            var width = Math.Max(1280, Math.Min(1920, screenWidth));
            var height = (int)(width * 9.0 / 16.0);
            return new Size(width, height);
        }

        private static Image[] GetOrCreateSlideCache(Size targetSize)
        {
            lock (SlideCacheLock)
            {
                if (slideCache != null && slideCacheSize == targetSize)
                    return slideCache;

                if (slideCache != null)
                {
                    foreach (var img in slideCache)
                        img?.Dispose();
                }

                slideCache = new Image[]
                {
                    LoadAndResizeSlide("slide1.png", targetSize.Width, targetSize.Height, Properties.Resources.slide1),
                    LoadAndResizeSlide("slide2.jpg", targetSize.Width, targetSize.Height, Properties.Resources.slide2),
                };

                slideCacheSize = targetSize;
                return slideCache;
            }
        }

        private static Image LoadAndResizeSlide(string fileName, int maxWidth, int maxHeight, Image fallback)
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", fileName);
            if (File.Exists(fullPath))
            {
                using var source = Image.FromFile(fullPath);
                return ResizeForSlide(source, maxWidth, maxHeight);
            }

            return ResizeForSlide(fallback, maxWidth, maxHeight);
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
