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
using kido_teacher_app.Services;
using kido_teacher_app.Shared.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kido_teacher_app
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ===== INIT DATA FOLDER =====
            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidoTeacherApp"
            );

            AppConfig.DownloadFolder = Path.Combine(basePath, "Downloads");
            AppConfig.LectureExtractFolder = Path.Combine(basePath, "Lectures");

            // ===== MIGRATE IMAGE CACHE FROM ROAMING -> LOCAL =====
            MigrateImageCacheToLocal();

            Directory.CreateDirectory(AppConfig.DownloadFolder);
            Directory.CreateDirectory(AppConfig.LectureExtractFolder);
            Directory.CreateDirectory(AppConfig.DbFolder);
            Directory.CreateDirectory(AppConfig.ClassImageCacheFolder);
            Directory.CreateDirectory(AppConfig.CourseImageCacheFolder);
            Directory.CreateDirectory(AppConfig.LectureImageCacheFolder);
            CleanZeroByteFiles(AppConfig.ClassImageCacheFolder);
            CleanZeroByteFiles(AppConfig.CourseImageCacheFolder);
            CleanZeroByteFiles(AppConfig.LectureImageCacheFolder);
            // ===========================

            var autoLoggedIn = AuthService.TryLoginWithSavedTokenAsync().GetAwaiter().GetResult();
            if (autoLoggedIn)
            {
                var prefetched = OfflinePrefetchService
                    .PrefetchTeacherOfflineAsync(prefetchImages: true)
                    .GetAwaiter()
                    .GetResult();

                if (!prefetched.Success)
                    FileLog.Error($"Prefetch khi auto-login thất bại, bỏ qua: {prefetched.Message}");

                Application.Run(new Main_Form());
                return;
            }

            // ===== LOGIN FLOW =====
            using (var login = new Form_Login())
            {
                if (login.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new Main_Form());
                }
            }
        }

        private static void MigrateImageCacheToLocal()
        {
            try
            {
                var roamingRoot = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "KidoTeacherApp"
                );

                var classSrc = Path.Combine(roamingRoot, "Class");
                var courseSrc = Path.Combine(roamingRoot, "Course");
                var lectureSrc = Path.Combine(roamingRoot, "Lecture");

                CopyCacheFolder(classSrc, AppConfig.ClassImageCacheFolder);
                CopyCacheFolder(courseSrc, AppConfig.CourseImageCacheFolder);
                CopyCacheFolder(lectureSrc, AppConfig.LectureImageCacheFolder);

                DeleteFolderSafe(classSrc);
                DeleteFolderSafe(courseSrc);
                DeleteFolderSafe(lectureSrc);
            }
            catch
            {
                // ignore migration errors
            }
        }

        private static void CopyCacheFolder(string sourceDir, string destDir)
        {
            if (!Directory.Exists(sourceDir))
                return;

            Directory.CreateDirectory(destDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relative = GetRelativePath(sourceDir, file);
                var dest = Path.Combine(destDir, relative);
                var destParent = Path.GetDirectoryName(dest);
                if (!Directory.Exists(destParent))
                    Directory.CreateDirectory(destParent!);

                if (!File.Exists(dest))
                {
                    File.Copy(file, dest, false);
                }
            }
        }

        private static string GetRelativePath(string basePath, string path)
        {
            var baseUri = new Uri(AppendDirectorySeparator(basePath));
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(baseUri.MakeRelativeUri(pathUri).ToString())
                .Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                ? path
                : path + Path.DirectorySeparatorChar;
        }

        private static void DeleteFolderSafe(string sourceDir)
        {
            try
            {
                if (Directory.Exists(sourceDir))
                {
                    Directory.Delete(sourceDir, true);
                }
            }
            catch
            {
                // ignore delete errors
            }
        }

        private static void CleanZeroByteFiles(string rootDir)
        {
            try
            {
                if (!Directory.Exists(rootDir))
                    return;

                foreach (var file in Directory.GetFiles(rootDir, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        var info = new FileInfo(file);
                        if (info.Length == 0)
                        {
                            info.Delete();
                        }
                    }
                    catch
                    {
                        // ignore per-file errors
                    }
                }
            }
            catch
            {
                // ignore
            }
        }
    }
}
