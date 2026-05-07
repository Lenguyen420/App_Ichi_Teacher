using kido_teacher_app.Config;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;

namespace kido_teacher_app.Services
{
    public static class CacheDiagnosticService
    {
        public static DiagnosticReport ScanDatabase()
        {
            var report = new DiagnosticReport();

            try
            {
                if (!File.Exists(AppConfig.DbPath))
                {
                    report.AddInfo("Database does not exist.");
                    report.Success = true;
                    return report;
                }

                var fileInfo = new FileInfo(AppConfig.DbPath);
                report.AddInfo($"Database path: {AppConfig.DbPath}");
                report.AddInfo($"Database size: {fileInfo.Length} bytes");

                using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
                conn.Open();

                report.AddInfo($"Database encoding: {ExecuteScalar(conn, "PRAGMA encoding;")}");

                ScanApiCache(conn, report);
                ScanOfflineLectureCache(conn, report);

                report.Success = true;
            }
            catch (Exception ex)
            {
                report.AddError($"Error scanning database: {ex.Message}");
            }

            return report;
        }

        public static bool BackupAndResetDatabase()
        {
            try
            {
                if (!File.Exists(AppConfig.DbPath))
                    return false;

                var backupPath = AppConfig.DbPath + ".backup." + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                File.Copy(AppConfig.DbPath, backupPath);
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Backed up database to: {backupPath}");

                File.Delete(AppConfig.DbPath);
                System.Diagnostics.Debug.WriteLine("[Diagnostic] Deleted database");

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Diagnostic] Error backing up database: {ex.Message}");
                return false;
            }
        }

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
                    sb.AppendLine($"  - {err}");
                sb.AppendLine();
            }

            if (report.Infos.Count > 0)
            {
                sb.AppendLine("INFORMATION:");
                foreach (var info in report.Infos)
                    sb.AppendLine($"  - {info}");
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
                    sb.AppendLine($"  - Entry: {entry.LectureId}");
                    foreach (var issue in entry.Issues)
                        sb.AppendLine($"    * {issue}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("RECOMMENDATION:");
            sb.AppendLine(report.CorruptedEntries.Count > 0
                ? "  Backup and reset the database if these entries affect app behavior."
                : "  Database is healthy.");

            sb.AppendLine("========================================");
            return sb.ToString();
        }

        private static void ScanApiCache(SqliteConnection conn, DiagnosticReport report)
        {
            if (!TableExists(conn, "api_cache"))
            {
                report.AddInfo("api_cache table does not exist.");
                return;
            }

            var count = Convert.ToInt64(ExecuteScalar(conn, "SELECT COUNT(*) FROM api_cache;"));
            report.AddInfo($"api_cache entries: {count}");

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT cache_key, json FROM api_cache;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var key = SafeGetString(reader, 0) ?? "";
                var json = SafeGetString(reader, 1) ?? "";
                var issues = new List<string>();

                AddTextIssues(issues, "cache_key", key);
                AddTextIssues(issues, "json", json);

                if (issues.Count > 0)
                    report.CorruptedEntries.Add(new CacheEntryIssue { LectureId = key, Issues = issues });
                else
                    report.ValidEntries++;
            }
        }

        private static void ScanOfflineLectureCache(SqliteConnection conn, DiagnosticReport report)
        {
            if (!TableExists(conn, "offline_lecture_cache"))
            {
                report.AddInfo("offline_lecture_cache table does not exist.");
                return;
            }

            var count = Convert.ToInt64(ExecuteScalar(conn, "SELECT COUNT(*) FROM offline_lecture_cache;"));
            report.AddInfo($"offline_lecture_cache entries: {count}");

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT lecture_id, pdf_path, video_path, elearning_path, powerpoint_path, offline_zip_url
                                FROM offline_lecture_cache;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var lectureId = SafeGetString(reader, 0) ?? "";
                var paths = new Dictionary<string, string?>
                {
                    ["pdf_path"] = SafeGetString(reader, 1),
                    ["video_path"] = SafeGetString(reader, 2),
                    ["elearning_path"] = SafeGetString(reader, 3),
                    ["powerpoint_path"] = SafeGetString(reader, 4),
                    ["offline_zip_url"] = SafeGetString(reader, 5)
                };

                var issues = new List<string>();
                foreach (var item in paths)
                {
                    AddTextIssues(issues, item.Key, item.Value);

                    if (item.Key != "offline_zip_url" &&
                        !string.IsNullOrEmpty(item.Value) &&
                        !File.Exists(item.Value))
                    {
                        issues.Add($"{item.Key} file missing: {item.Value}");
                    }
                }

                if (issues.Count > 0)
                    report.CorruptedEntries.Add(new CacheEntryIssue { LectureId = lectureId, Issues = issues });
                else
                    report.ValidEntries++;
            }
        }

        private static void AddTextIssues(List<string> issues, string field, string? value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (HasEncodingIssue(value))
                issues.Add($"{field} has encoding issue: {TrimForReport(value)}");

            if (ContainsInvalidChars(value))
                issues.Add($"{field} has invalid characters: {TrimForReport(value)}");
        }

        private static object? ExecuteScalar(SqliteConnection conn, string commandText)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = commandText;
            return cmd.ExecuteScalar();
        }

        private static bool TableExists(SqliteConnection conn, string tableName)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name LIMIT 1;";
            cmd.Parameters.AddWithValue("$name", tableName);
            return cmd.ExecuteScalar() != null;
        }

        private static string? SafeGetString(DbDataReader reader, int ordinal)
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

        private static bool HasEncodingIssue(string value)
        {
            return value.Contains('\uFFFD') ||
                   value.Contains("ï¿½") ||
                   value.Contains("Ã") ||
                   value.Contains("Â") ||
                   value.Contains("â€") ||
                   value.Contains("â„") ||
                   value.Contains("âœ");
        }

        private static bool ContainsInvalidChars(string value)
        {
            return value.Contains('\0') ||
                   value.Contains('\x1A') ||
                   value.Length > 260 ||
                   value.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
        }

        private static string TrimForReport(string value)
        {
            return value.Length <= 300 ? value : value.Substring(0, 300) + "...";
        }
    }

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
        public string LectureId { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
    }
}
