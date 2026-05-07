using kido_teacher_app.Services;
using System;
using System.Diagnostics;

namespace kido_teacher_app.Debug
{
    /// <summary>
    /// Debug helper - dùng để test cache database
    /// Gọi từ Program.cs hoặc tạo menu option
    /// </summary>
    public static class CacheDebugHelper
    {
        public static void RunDiagnostics()
        {
            Console.WriteLine("\n========== CACHE DIAGNOSTICS ==========\n");

            // Scan database
            Console.WriteLine("Scanning database...");
            var report = CacheDiagnosticService.ScanDatabase();

            // Print report
            var reportText = CacheDiagnosticService.GetReportText(report);
            Console.WriteLine(reportText);

            // Log to debug window
            System.Diagnostics.Debug.WriteLine(reportText);

            // Save to file
            var logPath = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "KIDO",
                "cache_diagnostic_report.txt"
            );

            try
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(logPath));
                System.IO.File.WriteAllText(logPath, reportText);
                Console.WriteLine($"\nReport saved to: {logPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving report: {ex.Message}");
            }

            // Ask to fix if corrupted
            if (report.CorruptedEntries.Count > 0)
            {
                Console.WriteLine("\n⚠ Found corrupted cache entries!");
                Console.WriteLine("Do you want to backup and reset database? (Y/N)");
                if (Console.ReadLine()?.ToUpper() == "Y")
                {
                    if (CacheDiagnosticService.BackupAndResetDatabase())
                    {
                        Console.WriteLine("✓ Database backed up and reset successfully");
                        Console.WriteLine("App will reinitialize cache on next start");
                    }
                    else
                    {
                        Console.WriteLine("✗ Failed to backup/reset database");
                    }
                }
            }

            Console.WriteLine("\n========== END DIAGNOSTICS ==========\n");
        }
    }
}
