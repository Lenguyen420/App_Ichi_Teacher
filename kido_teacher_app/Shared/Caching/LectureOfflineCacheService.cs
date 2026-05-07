using kido_teacher_app.Config;
using kido_teacher_app.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace kido_teacher_app.Shared.Caching
{
    public static class LectureOfflineCacheService
    {
        private static readonly string ResourceMapPath =
            Path.Combine(AppConfig.CacheFolder, "resource-map.json");
        private static readonly object InitLock = new object();
        private static bool _initialized;
        private static bool _migrated;

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (InitLock)
            {
                if (_initialized) return;

                Directory.CreateDirectory(AppConfig.DbFolder);

                using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
                conn.Open();

                // Set UTF-8 encoding for the connection
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA encoding = 'UTF-8';";
                cmd.ExecuteNonQuery();

                cmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS offline_lecture_cache (
                        lecture_id TEXT PRIMARY KEY,
                        pdf_path TEXT,
                        video_path TEXT,
                        elearning_path TEXT,
                        powerpoint_path TEXT,
                        offline_zip_url TEXT,
                        updated_at TEXT NOT NULL
                      );";
                cmd.ExecuteNonQuery();

                EnsureSchemaUpToDate(conn);

                _initialized = true;
            }

            MigrateFromJsonIfExists();
            
            // Clean corrupted entries on first initialization
            try
            {
                CleanCorruptedEntries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Error during cleanup: {ex.Message}");
            }
        }

        private static void MigrateFromJsonIfExists()
        {
            if (_migrated) return;
            _migrated = true;

            if (!File.Exists(ResourceMapPath))
                return;

            try
            {
                var json = File.ReadAllText(ResourceMapPath);
                var map = JsonSerializer.Deserialize<Dictionary<string, LectureOfflineCache>>(json)
                    ?? new Dictionary<string, LectureOfflineCache>();

                if (map.Count == 0)
                {
                    File.Delete(ResourceMapPath);
                    return;
                }

                using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
                conn.Open();

                foreach (var kv in map)
                {
                    var cache = kv.Value;
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText =
                        @"INSERT INTO offline_lecture_cache (lecture_id, pdf_path, video_path, elearning_path, powerpoint_path, updated_at)
                          VALUES (@id, @pdf, @video, @elearn, @powerpoint, @t)
                          ON CONFLICT(lecture_id) DO UPDATE SET
                            pdf_path = @pdf,
                            video_path = @video,
                            elearning_path = @elearn,
                            powerpoint_path = @powerpoint,
                            updated_at = @t;";
                    cmd.Parameters.AddWithValue("@id", cache.LectureId);
                    cmd.Parameters.AddWithValue("@pdf", (object?)cache.PdfPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@video", (object?)cache.VideoPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@elearn", (object?)cache.ElearningPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@powerpoint", (object?)cache.PowerPointPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));
                    cmd.ExecuteNonQuery();
                }

                File.Delete(ResourceMapPath);
            }
            catch
            {
                // Keep JSON if migration fails
            }
        }

        // =========================
        // SAVE CACHE
        // =========================
        public static void Save(
            string lectureId,
            string? pdfPath,
            string? videoPath,
            string? elearningPath,
            string? powerpointPath,
            string? offlineZipUrl
        )
        {
            EnsureInitialized();

            // Log what we're about to save
            System.Diagnostics.Debug.WriteLine($"[Cache] Saving cache for lecture: {lectureId}");
            if (!string.IsNullOrEmpty(pdfPath))
                System.Diagnostics.Debug.WriteLine($"[Cache]   PDF: {pdfPath}");
            if (!string.IsNullOrEmpty(videoPath))
                System.Diagnostics.Debug.WriteLine($"[Cache]   Video: {videoPath}");
            if (!string.IsNullOrEmpty(elearningPath))
                System.Diagnostics.Debug.WriteLine($"[Cache]   Elearning: {elearningPath}");
            if (!string.IsNullOrEmpty(powerpointPath))
                System.Diagnostics.Debug.WriteLine($"[Cache]   PowerPoint: {powerpointPath}");
            if (!string.IsNullOrEmpty(offlineZipUrl))
                System.Diagnostics.Debug.WriteLine($"[Cache]   ZipUrl: {offlineZipUrl}");

            // Validate file paths - only save if file exists
            // Wrap in try-catch to handle invalid paths safely
            pdfPath = ValidatePath(pdfPath);
            videoPath = ValidatePath(videoPath);
            elearningPath = ValidatePath(elearningPath);
            powerpointPath = ValidatePath(powerpointPath);

            using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO offline_lecture_cache (lecture_id, pdf_path, video_path, elearning_path, powerpoint_path, offline_zip_url, updated_at)
                  VALUES (@id, @pdf, @video, @elearn, @powerpoint, @zip, @t)
                  ON CONFLICT(lecture_id) DO UPDATE SET
                    pdf_path = @pdf,
                    video_path = @video,
                    elearning_path = @elearn,
                    powerpoint_path = @powerpoint,
                    offline_zip_url = @zip,
                    updated_at = @t;";
            cmd.Parameters.AddWithValue("@id", lectureId);
            cmd.Parameters.AddWithValue("@pdf", (object?)pdfPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@video", (object?)videoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@elearn", (object?)elearningPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@powerpoint", (object?)powerpointPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@zip", (object?)offlineZipUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));
            cmd.ExecuteNonQuery();

            System.Diagnostics.Debug.WriteLine($"[Cache] ✓ Saved cache for lecture: {lectureId}");
        }

        // =========================
        // HELPER: Validate Path Safely
        // =========================
        private static string? ValidatePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            try
            {
                // Check if path is too long
                if (path.Length > 260)
                {
                    System.Diagnostics.Debug.WriteLine($"[Cache] Path too long (>260): {path}");
                    return null;
                }

                // Try to access file - if throws, path is invalid
                if (File.Exists(path))
                    return path;
                else
                    return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Invalid path '{path}': {ex.Message}");
                return null;
            }
        }

        // =========================
        // LOAD CACHE
        // =========================
        public static LectureOfflineCache? Load(string lectureId)
        {
            EnsureInitialized();

            using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            conn.Open();

            // Ensure UTF-8 encoding
            using var pragmaCmd = conn.CreateCommand();
            pragmaCmd.CommandText = "PRAGMA encoding;";
            var encoding = pragmaCmd.ExecuteScalar();
            System.Diagnostics.Debug.WriteLine($"[Cache] Database encoding: {encoding}");

            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"SELECT lecture_id, pdf_path, video_path, elearning_path, powerpoint_path, offline_zip_url
                  FROM offline_lecture_cache
                  WHERE lecture_id = @id
                  LIMIT 1;";
            cmd.Parameters.AddWithValue("@id", lectureId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return null;

            var cache = new LectureOfflineCache
            {
                LectureId = SafeGetString(reader, 0),
                PdfPath = SafeGetString(reader, 1),
                VideoPath = SafeGetString(reader, 2),
                ElearningPath = SafeGetString(reader, 3),
                PowerPointPath = SafeGetString(reader, 4),
                OfflineZipUrl = SafeGetString(reader, 5)
            };

            // Log paths for debugging
            if (!string.IsNullOrEmpty(cache.PdfPath))
                System.Diagnostics.Debug.WriteLine($"[Cache] Loaded PDF: {cache.PdfPath}");
            if (!string.IsNullOrEmpty(cache.VideoPath))
                System.Diagnostics.Debug.WriteLine($"[Cache] Loaded Video: {cache.VideoPath}");
            if (!string.IsNullOrEmpty(cache.ElearningPath))
                System.Diagnostics.Debug.WriteLine($"[Cache] Loaded Elearning: {cache.ElearningPath}");
            if (!string.IsNullOrEmpty(cache.PowerPointPath))
                System.Diagnostics.Debug.WriteLine($"[Cache] Loaded PowerPoint: {cache.PowerPointPath}");

            // Validate file existence and clean up dead entries
            bool hasDeadFiles = ValidateAndCleanDeadFiles(cache);
            if (hasDeadFiles)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Cleaned up dead file references for lecture: {lectureId}");
            }

            return cache;
        }

        // =========================
        // HELPER: Safe Get String
        // =========================
        private static string? SafeGetString(System.Data.Common.DbDataReader reader, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return null;

            try
            {
                var value = reader.GetString(ordinal);
                // Trim any accidental whitespace
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Error reading string at ordinal {ordinal}: {ex.Message}");
                return null;
            }
        }

        // =========================
        // VALIDATE FILE EXISTENCE
        // =========================
        private static bool ValidateAndCleanDeadFiles(LectureOfflineCache cache)
        {
            bool hasDeadFiles = false;
            var validatedCache = new LectureOfflineCache { LectureId = cache.LectureId };

            // Use ValidatePath helper for safe checking
            string? pdfPath = ValidatePath(cache.PdfPath);
            string? videoPath = ValidatePath(cache.VideoPath);
            string? elearningPath = ValidatePath(cache.ElearningPath);
            string? powerpointPath = ValidatePath(cache.PowerPointPath);

            // Track if any files were lost
            if (!string.IsNullOrEmpty(cache.PdfPath) && pdfPath == null)
                hasDeadFiles = true;
            if (!string.IsNullOrEmpty(cache.VideoPath) && videoPath == null)
                hasDeadFiles = true;
            if (!string.IsNullOrEmpty(cache.ElearningPath) && elearningPath == null)
                hasDeadFiles = true;
            if (!string.IsNullOrEmpty(cache.PowerPointPath) && powerpointPath == null)
                hasDeadFiles = true;

            validatedCache.PdfPath = pdfPath;
            validatedCache.VideoPath = videoPath;
            validatedCache.ElearningPath = elearningPath;
            validatedCache.PowerPointPath = powerpointPath;
            validatedCache.OfflineZipUrl = cache.OfflineZipUrl;

            // If there were dead files, update the database
            if (hasDeadFiles)
            {
                Save(validatedCache.LectureId, validatedCache.PdfPath, validatedCache.VideoPath,
                    validatedCache.ElearningPath, validatedCache.PowerPointPath, validatedCache.OfflineZipUrl);

                System.Diagnostics.Debug.WriteLine($"[Cache] Removed dead file references for lecture: {cache.LectureId}");
            }

            // Update cache object with validated paths
            cache.PdfPath = pdfPath;
            cache.VideoPath = videoPath;
            cache.ElearningPath = elearningPath;
            cache.PowerPointPath = powerpointPath;

            return hasDeadFiles;
        }

        // =========================
        // DELETE CACHE
        // =========================
        public static void Delete(string lectureId)
        {
            var cache = Load(lectureId);
            if (cache == null) return;

            // ===== XOA CAC FILE VAT LY =====
            DeleteFileIfExists(cache.PdfPath);
            DeleteFileIfExists(cache.VideoPath);
            DeleteFileIfExists(cache.ElearningPath);
            DeleteFileIfExists(cache.PowerPointPath);

            // ===== XOA THU MUC EXTRACT =====
            try
            {
                string lectureFolder = Path.Combine(AppConfig.LectureExtractFolder, lectureId);
                if (Directory.Exists(lectureFolder))
                {
                    Directory.Delete(lectureFolder, true);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted folder: {lectureFolder}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Error deleting folder: {ex.Message}");
            }

            // ===== XOA ENTRY TRONG DB =====
            EnsureInitialized();
            using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM offline_lecture_cache WHERE lecture_id = @id;";
            cmd.Parameters.AddWithValue("@id", lectureId);
            cmd.ExecuteNonQuery();

            System.Diagnostics.Debug.WriteLine($"[Cache] Deleted offline cache for lecture: {lectureId}");
        }

        // =========================
        // CLEAN CORRUPTED CACHE ENTRIES
        // =========================
        public static void CleanCorruptedEntries()
        {
            EnsureInitialized();

            System.Diagnostics.Debug.WriteLine("[Cache] Scanning for corrupted cache entries...");

            using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT lecture_id, pdf_path, video_path, elearning_path, powerpoint_path 
                               FROM offline_lecture_cache;";

            var corruptedIds = new List<string>();

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    try
                    {
                        var lectureId = reader.GetString(0);
                        var pdfPath = reader.IsDBNull(1) ? null : reader.GetString(1);
                        var videoPath = reader.IsDBNull(2) ? null : reader.GetString(2);
                        var elearningPath = reader.IsDBNull(3) ? null : reader.GetString(3);
                        var powerpointPath = reader.IsDBNull(4) ? null : reader.GetString(4);

                        // Check for encoding corruption (invalid UTF-8 patterns)
                        bool isCorrupted = false;
                        if (HasEncodingIssue(pdfPath) || HasEncodingIssue(videoPath) || 
                            HasEncodingIssue(elearningPath) || HasEncodingIssue(powerpointPath))
                        {
                            isCorrupted = true;
                            System.Diagnostics.Debug.WriteLine($"[Cache] Found corrupted entry: {lectureId}");
                        }

                        // Check if any path is invalid (contains null chars, etc)
                        if (ContainsInvalidChars(pdfPath) || ContainsInvalidChars(videoPath) || 
                            ContainsInvalidChars(elearningPath) || ContainsInvalidChars(powerpointPath))
                        {
                            isCorrupted = true;
                            System.Diagnostics.Debug.WriteLine($"[Cache] Found invalid chars in: {lectureId}");
                        }

                        if (isCorrupted)
                        {
                            corruptedIds.Add(lectureId);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Cache] Error reading record: {ex.Message}");
                    }
                }
            }

            // Delete corrupted entries
            foreach (var id in corruptedIds)
            {
                Delete(id);
                System.Diagnostics.Debug.WriteLine($"[Cache] Deleted corrupted entry: {id}");
            }

            if (corruptedIds.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Cleaned {corruptedIds.Count} corrupted cache entries");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[Cache] No corrupted entries found");
            }
        }

        // =========================
        // HELPER: Check Encoding Issue
        // =========================
        private static bool HasEncodingIssue(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Check for common UTF-8 encoding corruption patterns
            return path.Contains("Ã") || path.Contains("â") || path.Contains("ï") || 
                   path.Contains("Â") || path.Contains("Äº") || path.Contains("Â") ||
                   path.Contains("\u00A0") || path.Contains("\u00AD");
        }

        // =========================
        // HELPER: Check Invalid Chars
        // =========================
        private static bool ContainsInvalidChars(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            // Check for null chars and other invalid filesystem chars
            return path.Contains('\0') || path.Contains('\x1A') || 
                   (path.Length > 260) || path.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t');
        }

        // =========================
        // HELPER: XOA FILE NEU TON TAI
        // =========================
        private static void DeleteFileIfExists(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Error deleting file {filePath}: {ex.Message}");
            }
        }

        // =========================
        // CLEAR ALL CACHE (FOR TESTING)
        // =========================
        public static void ClearAll()
        {
            try
            {
                EnsureInitialized();

                using (var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}"))
                {
                    conn.Open();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = @"DELETE FROM offline_lecture_cache;";
                    cmd.ExecuteNonQuery();
                }

                // Xoa thu muc Lectures
                if (Directory.Exists(AppConfig.LectureExtractFolder))
                {
                    Directory.Delete(AppConfig.LectureExtractFolder, true);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted lectures folder: {AppConfig.LectureExtractFolder}");
                }

                // Xoa thu muc Downloads (file ZIP)
                if (Directory.Exists(AppConfig.DownloadFolder))
                {
                    Directory.Delete(AppConfig.DownloadFolder, true);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted downloads folder: {AppConfig.DownloadFolder}");
                }

                System.Diagnostics.Debug.WriteLine("[Cache] All offline data cleared!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Error clearing data: {ex.Message}");
            }
        }

        // =========================
        // SCHEMA MIGRATION
        // =========================
        private static void EnsureSchemaUpToDate(SqliteConnection conn)
        {
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"PRAGMA table_info(offline_lecture_cache);";
                using var reader = cmd.ExecuteReader();
                bool hasOfflineZipUrl = false;
                bool hasPowerPointPath = false;

                while (reader.Read())
                {
                    var col = reader.GetString(1);
                    if (string.Equals(col, "offline_zip_url", StringComparison.OrdinalIgnoreCase))
                    {
                        hasOfflineZipUrl = true;
                    }

                    if (string.Equals(col, "powerpoint_path", StringComparison.OrdinalIgnoreCase))
                    {
                        hasPowerPointPath = true;
                    }
                }

                if (!hasPowerPointPath)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = @"ALTER TABLE offline_lecture_cache ADD COLUMN powerpoint_path TEXT;";
                    alter.ExecuteNonQuery();
                }

                if (!hasOfflineZipUrl)
                {
                    using var alter = conn.CreateCommand();
                    alter.CommandText = @"ALTER TABLE offline_lecture_cache ADD COLUMN offline_zip_url TEXT;";
                    alter.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Cache] Schema migration error: {ex.Message}");
            }
        }
    }
}
