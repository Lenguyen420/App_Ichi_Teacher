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
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show(e.Exception.ToString(), "UI Exception");
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                MessageBox.Show(e.ExceptionObject?.ToString(), "Fatal Exception");
            };

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            ApplicationConfiguration.Initialize();

            string basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "KidoTeacherApp"
            );

            AppConfig.DownloadFolder = Path.Combine(basePath, "Downloads");
            AppConfig.LectureExtractFolder = Path.Combine(basePath, "Lectures");

            Directory.CreateDirectory(AppConfig.DownloadFolder);
            Directory.CreateDirectory(AppConfig.LectureExtractFolder);

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
