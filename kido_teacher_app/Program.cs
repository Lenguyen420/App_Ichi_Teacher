//using kido_teacher_app.Config;
//using System;
//using System.IO;
//using System.Windows.Forms;

//namespace kido_teacher_app
//{
//    internal static class Program
//    {
//        [STAThread]
//        static void Main()
//        {
//            //bắt lỗi khi đăng nhập bị tắt 
//            Application.ThreadException += (s, e) =>
//            {
//                MessageBox.Show(e.Exception.ToString(), "UI Exception");
//            };

//            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
//            {
//                MessageBox.Show(e.ExceptionObject?.ToString(), "Fatal Exception");
//            };

//            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

//            ApplicationConfiguration.Initialize(); 
//            Application.Run(new Form_Login());


//            //end


//            ApplicationConfiguration.Initialize();
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            // === KHỞI TẠO THƯ MỤC DỮ LIỆU ===
//            string basePath = Path.Combine(
//                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
//                "KidoTeacherApp"
//            );

//            AppConfig.DownloadFolder = Path.Combine(basePath, "Downloads");
//            AppConfig.LectureExtractFolder = Path.Combine(basePath, "Lectures");

//            Directory.CreateDirectory(AppConfig.DownloadFolder);
//            Directory.CreateDirectory(AppConfig.LectureExtractFolder);
//            // =================================

//            using (Form_Login login = new Form_Login())
//            {
//                if (login.ShowDialog() == DialogResult.OK)
//                {
//                    Application.Run(new Main_Form());
//                }
//                else
//                {
//                    Application.Exit();
//                }
//            }
//        }
//    }
//}
using kido_teacher_app.Config;
using System;
using System.IO;
using System.Windows.Forms;

namespace kido_teacher_app
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            // ===== GLOBAL EXCEPTION =====
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "UI Exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show(e.ExceptionObject?.ToString(), "Fatal Exception");
            };

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            //  CHỈ GỌI 1 LẦN – TRƯỚC MỌI FORM
            ApplicationConfiguration.Initialize();

            // ===== INIT DATA FOLDER =====
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidoTeacherApp"
            );

            AppConfig.DownloadFolder = Path.Combine(basePath, "Downloads");
            AppConfig.LectureExtractFolder = Path.Combine(basePath, "Lectures");

            Directory.CreateDirectory(AppConfig.DownloadFolder);
            Directory.CreateDirectory(AppConfig.LectureExtractFolder);
            // ===========================

            // ===== LOGIN FLOW =====
            using (var login = new Form_Login())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new Main_Form());
                }
            }
        }
    }
}
