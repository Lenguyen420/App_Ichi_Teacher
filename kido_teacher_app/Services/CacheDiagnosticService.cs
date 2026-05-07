using Microsoft.Data.Sqlite;
using kido_teacher_app.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace kido_teacher_app.Services
{
    /// <summary>
    /// Diagnostic service để kiểm tra và fix database cache bị lỗi encoding
    /// </summary>
    public static class CacheDiagnosticService
    {
        // ======================
        // SCAN DATABASE FOR ISSUES
        // ======================
        public static DiagnosticReport ScanDatabase()
        {
            var report = new DiagnosticReport();

            try
            {
                if (!File.Exists(AppConfig.DbPath))
                {
                    report.AddInfo("Database không tồn tại");
                    return report;
                }

                var fileInfo = new FileInfo(AppConfig.DbPath);
                report.AddInfo($"Database size: {fileInfo.Length} bytes");

                using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
                conn.Open();

                // Check encoding
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA encoding;";
                var encoding = cmd.ExecuteScalar();
                report.AddInfo($"Database encoding: {encoding}");

                // Scan cache table
                cmd.CommandText = @"SELECT COUNT(*) FROM offline_lecture_cache;";
                var cacheCount = (long)cmd.ExecuteScalar();
                report.AddInfo($"Cache entries: {cacheCount}");

                // Check each entry
                cmd.CommandText = @"SELECT lecture_id, pdf_path, video_path, elearning_path, powerpoint_path, offline_zip_url
                                   FROM offline_lecture_cache;";

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var lectureId = reader.GetString(0);
                        var pdfPath = SafeGetString(reader, 1);
                        var videoPath = SafeGetString(reader, 2);
                        var elearningPath = SafeGetString(reader, 3);
                        var powerpointPath = SafeGetString(reader, 4);
                        var zipUrl = SafeGetString(reader, 5);

                        // Check for corruption
                        var issues = new List<string>();

                        if (HasEncodingIssue(pdfPath))
                            issues.Add($"PDF has encoding issue: {pdfPath}");
                        if (HasEncodingIssue(videoPath))
                            issues.Add($"Video has encoding issue: {videoPath}");
                        if (HasEncodingIssue(elearningPath))
                            issues.Add($"Elearning has encoding issue: {elearningPath}");
                        if (HasEncodingIssue(powerpointPath))
                            issues.Add($"PowerPoint has encoding issue: {powerpointPath}");

                        if (ContainsInvalidChars(pdfPath))
                            issues.Add($"PDF has invalid chars: {pdfPath}");
                        if (ContainsInvalidChars(videoPath))
                            issues.Add($"Video has invalid chars: {videoPath}");
                        if (ContainsInvalidChars(elearningPath))
                            issues.Add($"Elearning has invalid chars: {elearningPath}");
                        if (ContainsInvalidChars(powerpointPath))
                            issues.Add($"PowerPoint has invalid chars: {powerpointPath}");

                        // Check if files actually exist
                        if (!string.IsNullOrEmpty(pdfPath) && !File.Exists(pdfPath))
                            issues.Add($"PDF file missing: {pdfPath}");
                        if (!string.IsNullOrEmpty(videoPath) && !File.Exists(videoPath))
                            issues.Add($"Video file missing: {videoPath}");
                        if (!string.IsNullOrEmpty(elearningPath) && !File.Exists(elearningPath))
                            issues.Add($"Elearning file missing: {elearningPath}");
                        if (!string.IsNullOrEmpty(powerpointPath) && !File.Exists(powerpointPath))
                            issues.Add($"PowerPoint file missing: {powerpointPath}");

                        if (issues.Count > 0)
                        {
                            report.CorruptedEntries.Add(new CacheEntryIssue
                            {
                                LectureId = lectureId,
                                Issues = issues
                            });
                        }
                        else
                        {
                            report.ValidEntries++;
                        }
                    }
                }

                report.Success = true;
            }
            catch (Exception ex)
            {
                report.AddError($"Error scanning database: {ex.Message}");
            }

            return report;
        }

        // ======================
        // BACKUP AND RESET DATABASE
        // ======================
        public static bool BackupAndResetDatabase()
        {
            try
            {
                if (!File.Exists(AppConfig.DbPath))
                    return false;

                var backupPath = AppConfig.DbPath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Copy(AppConfig.DbPath, backupPath);
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Backed up database to: {backupPath}");

                // Delete old database
                File.Delete(AppConfig.DbPath);
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Deleted corrupted database");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Error backing up database: {ex.Message}");
                return false;
            }
        }

        // ======================
        // EXPORT DIAGNOSTIC REPORT
        // ======================
        public static string GetReportText(DiagnosticReport report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("CACHE DIAGNOSTIC REPORT");
            sb.AppendLine("========================================");
            sb.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();

            if (report.Errors.Count > 0)
            {
                sb.AppendLine("ERRORS:");
                foreach (var err in report.Errors)
                    sb.AppendLine($"  ✗ {err}");
                sb.AppendLine();
            }

            if (report.Infos.Count > 0)
            {
                sb.AppendLine("INFORMATION:");
                foreach (var info in report.Infos)
                    sb.AppendLine($"  ℹ {info}");
                sb.AppendLine();
            }

            sb.AppendLine($"Valid Entries: {report.ValidEntries}");
            sb.AppendLine($"Corrupted Entries: {report.CorruptedEntries.Count}");
            sb.AppendLine();

            if (report.CorruptedEntries.Count > 0)
            {
                sb.AppendLine("CORRUPTED ENTRIES:");
                foreach (var entry in report.CorruptedEntries)
                {
                    sb.AppendLine($"  - Lecture: {entry.LectureId}");
                    foreach (var issue in entry.Issues)
                        sb.AppendLine($"    • {issue}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("RECOMMENDATION:");
            if (report.CorruptedEntries.Count > 0)
                sb.AppendLine("  → Call BackupAndResetDatabase() to fix corrupted entries");
            else
                sb.AppendLine("  → Database is healthy");

            sb.AppendLine("========================================");
            return sb.ToString();
        }

        // ======================
        // HELPERS
        // ======================
        private static string? SafeGetString(System.Data.Common.DbDataReader reader, int ordinal)
        {
            try
            {
                if (reader.IsDBNull(ordinal))
                    return null;
                var value = reader.GetString(ordinal);
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            catch
            {
                return null;
            }
        }

        private static bool HasEncodingIssue(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.Contains("Ã") || path.Contains("â") || path.Contains("ï") ||
                   path.Contains("Â") || path.Contains("º") || path.Contains("ß") ||
                   path.Contains("ƒ") || path.Contains("€") || path.Contains("'") ||
                   path.Contains("'") || path.Contains(""") || path.Contains(""");
        }

        private static bool ContainsInvalidChars(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.Contains('\0') || path.Contains('\x1A') ||
                   (path.Length > 260) || path.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
        }
    }

    // ======================
    // MODELS
    // ======================
    public class DiagnosticReport
    {
        public bool Success { get; set; }
        public int ValidEntries { get; set; }
        public List<string> Infos { get; } = new();
        public List<string> Errors { get; } = new();
        public List<CacheEntryIssue> CorruptedEntries { get; } = new();

        public void AddInfo(string message) => Infos.Add(message);
        public void AddError(string message) => Errors.Add(message);
    }

    public class CacheEntryIssue
    {
        public string LectureId { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}
