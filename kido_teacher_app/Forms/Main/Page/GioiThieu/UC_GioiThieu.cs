
using kido_teacher_app.Forms.Main.Page.GioiThieu;
using kido_teacher_app.Forms.Main.Page.QuanLyNoiDung;
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

        }
        private void InitSlideShow()
        {
            slideImages = new Image[]
            {
        Properties.Resources.slide10,
        Properties.Resources.slide9,
       Properties.Resources.slide11,
       Properties.Resources.slide8,
       Properties.Resources.slide1
            };

            currentSlideIndex = 0;
            banner.Image = slideImages[currentSlideIndex];

            slideTimer = new System.Windows.Forms.Timer();
            slideTimer.Interval = 4000; // 4 giây
            slideTimer.Tick += SlideTimer_Tick;
            slideTimer.Start(); // ⭐ BẮT BUỘC
        }
        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            currentSlideIndex++;

            if (currentSlideIndex >= slideImages.Length)
                currentSlideIndex = 0;

            banner.Image = slideImages[currentSlideIndex];
        }
    }
}
