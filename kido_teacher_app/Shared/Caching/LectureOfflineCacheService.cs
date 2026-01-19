using kido_teacher_app.Config;
using kido_teacher_app.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace kido_teacher_app.Shared.Caching
{
    public static class LectureOfflineCacheService
    {
        private static readonly string ResourceMapPath =
            Path.Combine(AppConfig.CacheFolder, "resource-map.json");

        // =========================
        // LOAD TOÀN BỘ MAP
        // =========================
        private static Dictionary<string, LectureOfflineCache> LoadMap()
        {
            if (!File.Exists(ResourceMapPath))
                return new Dictionary<string, LectureOfflineCache>();

            try
            {
                var json = File.ReadAllText(ResourceMapPath);
                return JsonSerializer.Deserialize<Dictionary<string, LectureOfflineCache>>(json)
                    ?? new Dictionary<string, LectureOfflineCache>();
            }
            catch
            {
                return new Dictionary<string, LectureOfflineCache>();
            }
        }

        // =========================
        // LƯU TOÀN BỘ MAP
        // =========================
        private static void SaveMap(Dictionary<string, LectureOfflineCache> map)
        {
            Directory.CreateDirectory(AppConfig.CacheFolder);

            File.WriteAllText(
                ResourceMapPath,
                JsonSerializer.Serialize(map, new JsonSerializerOptions
                {
                    WriteIndented = true
                })
            );
        }

        // =========================
        // SAVE CACHE
        // =========================
        public static void Save(
            string lectureId,
            string? pdfPath,
            string? videoPath,
            string? elearningPath
        )
        {
            var map = LoadMap();

            map[lectureId] = new LectureOfflineCache
            {
                LectureId = lectureId,
                PdfPath = pdfPath,
                VideoPath = videoPath,
                ElearningPath = elearningPath
            };

            SaveMap(map);
        }

        // =========================
        // LOAD CACHE
        // =========================
        public static LectureOfflineCache? Load(string lectureId)
        {
            var map = LoadMap();
            return map.ContainsKey(lectureId) ? map[lectureId] : null;
        }

        // =========================
        // DELETE CACHE
        // =========================
        public static void Delete(string lectureId)
        {
            var map = LoadMap();

            if (map.ContainsKey(lectureId))
            {
                var cache = map[lectureId];

                // ===== XÓA CÁC FILE VẬT LÝ =====
                DeleteFileIfExists(cache.PdfPath);
                DeleteFileIfExists(cache.VideoPath);
                DeleteFileIfExists(cache.ElearningPath);

                // ===== XÓA THƯ MỤC EXTRACT (NẾU RỖNG) =====
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

                // ===== XÓA ENTRY TRONG MAP =====
                map.Remove(lectureId);
                SaveMap(map);

                System.Diagnostics.Debug.WriteLine($"[Cache] Deleted offline cache for lecture: {lectureId}");
            }
        }

        // =========================
        // HELPER: XÓA FILE NẾU TỒN TẠI
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
                // Xóa file JSON cache
                if (File.Exists(ResourceMapPath))
                {
                    File.Delete(ResourceMapPath);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted cache file: {ResourceMapPath}");
                }

                // Xóa thư mục Lectures
                if (Directory.Exists(AppConfig.LectureExtractFolder))
                {
                    Directory.Delete(AppConfig.LectureExtractFolder, true);
                    System.Diagnostics.Debug.WriteLine($"[Cache] Deleted lectures folder: {AppConfig.LectureExtractFolder}");
                }

                // Xóa thư mục Downloads (file ZIP)
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
    }
}
