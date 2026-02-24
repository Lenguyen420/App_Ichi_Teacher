using kido_teacher_app.Config;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public static class DbCacheService
    {
        private static readonly object InitLock = new object();
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized) return;

            lock (InitLock)
            {
                if (_initialized) return;

                Directory.CreateDirectory(AppConfig.DbFolder);

                using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS api_cache (
                        cache_key TEXT PRIMARY KEY,
                        json TEXT NOT NULL,
                        updated_at TEXT NOT NULL
                      );";
                cmd.ExecuteNonQuery();

                _initialized = true;
            }
        }

        public static async Task SaveAsync(string key, string json)
        {
            EnsureInitialized();

            await using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText =
                @"INSERT INTO api_cache (cache_key, json, updated_at)
                  VALUES (@k, @j, @t)
                  ON CONFLICT(cache_key) DO UPDATE SET
                    json = @j,
                    updated_at = @t;";

            cmd.Parameters.AddWithValue("@k", key);
            cmd.Parameters.AddWithValue("@j", json);
            cmd.Parameters.AddWithValue("@t", DateTime.UtcNow.ToString("o"));

            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<string?> GetJsonAsync(string key)
        {
            EnsureInitialized();

            await using var conn = new SqliteConnection($"Data Source={AppConfig.DbPath}");
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT json FROM api_cache WHERE cache_key = @k LIMIT 1;";
            cmd.Parameters.AddWithValue("@k", key);

            var result = await cmd.ExecuteScalarAsync();
            return result as string;
        }

        public static async Task<T?> GetAsync<T>(string key)
        {
            var json = await GetJsonAsync(key);
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
