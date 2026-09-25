using kido_teacher_app.Shared.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace kido_teacher_app.Shared.Web
{
    public static class WindowsVersionHelper
    {
        public static bool IsWindows7
        {
            get
            {
                var version = Environment.OSVersion.Version;
                return Environment.OSVersion.Platform == PlatformID.Win32NT
                    && version.Major == 6
                    && version.Minor == 1;
            }
        }
    }

    public static class LocalElearningHttpServer
    {
        private sealed class LessonMapping
        {
            public string RootFolder { get; set; } = string.Empty;
            public string EntryPath { get; set; } = string.Empty;
        }

        private static readonly object SyncRoot = new object();
        private static readonly ConcurrentDictionary<string, LessonMapping> Lessons =
            new ConcurrentDictionary<string, LessonMapping>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, string> TokensByEntryPath =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly SemaphoreSlim RequestGate = new SemaphoreSlim(12, 12);

        private static TcpListener? _listener;
        private static int _port;

        public static bool TryCreateLessonUrl(string entryPath, out string url, out string error)
        {
            url = string.Empty;
            error = string.Empty;

            try
            {
                if (string.IsNullOrWhiteSpace(entryPath))
                    throw new ArgumentException("Đường dẫn bài giảng đang trống.", nameof(entryPath));

                var fullEntryPath = Path.GetFullPath(entryPath);
                if (!File.Exists(fullEntryPath))
                    throw new FileNotFoundException("Không tìm thấy file bài giảng.", fullEntryPath);

                var rootFolder = Path.GetDirectoryName(fullEntryPath);
                if (string.IsNullOrWhiteSpace(rootFolder))
                    throw new InvalidOperationException("Không xác định được thư mục bài giảng.");

                lock (SyncRoot)
                {
                    EnsureStarted();

                    if (!TokensByEntryPath.TryGetValue(fullEntryPath, out var token))
                    {
                        token = Guid.NewGuid().ToString("N");
                        Lessons[token] = new LessonMapping
                        {
                            RootFolder = Path.GetFullPath(rootFolder),
                            EntryPath = fullEntryPath
                        };
                        TokensByEntryPath[fullEntryPath] = token;
                    }

                    var encodedFileName = Uri.EscapeDataString(Path.GetFileName(fullEntryPath));
                    url = $"http://127.0.0.1:{_port}/lesson/{token}/{encodedFileName}";
                    WebViewLog.Info($"E-LEARNING local server url='{url}' entry='{fullEntryPath}'");
                    return true;
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                WebViewLog.Error($"E-LEARNING local server start failed entry='{entryPath}' error='{ex}'");
                return false;
            }
        }

        public static void Stop()
        {
            lock (SyncRoot)
            {
                var listener = _listener;
                _listener = null;
                _port = 0;
                Lessons.Clear();
                TokensByEntryPath.Clear();

                try
                {
                    listener?.Stop();
                }
                catch
                {
                    // Shutdown must not prevent the application from closing.
                }
            }
        }

        private static void EnsureStarted()
        {
            if (_listener != null)
                return;

            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            _listener = listener;
            _port = ((IPEndPoint)listener.LocalEndpoint).Port;
            _ = Task.Run(() => AcceptLoopAsync(listener));
            WebViewLog.Info($"E-LEARNING local server started address='127.0.0.1' port='{_port}'");
        }

        private static async Task AcceptLoopAsync(TcpListener listener)
        {
            while (ReferenceEquals(_listener, listener))
            {
                TcpClient client;
                try
                {
                    client = await listener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    if (!ReferenceEquals(_listener, listener))
                        break;
                    continue;
                }
                catch (Exception ex)
                {
                    WebViewLog.Error($"E-LEARNING local server accept failed error='{ex}'");
                    break;
                }

                _ = Task.Run(async () =>
                {
                    await RequestGate.WaitAsync();
                    try
                    {
                        await HandleClientAsync(client);
                    }
                    finally
                    {
                        RequestGate.Release();
                        client.Close();
                    }
                });
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                client.NoDelay = true;
                using (var stream = client.GetStream())
                using (var reader = new StreamReader(stream, Encoding.ASCII, false, 4096, true))
                {
                    var requestLine = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(requestLine))
                        return;

                    var requestParts = requestLine.Split(' ');
                    if (requestParts.Length < 2)
                    {
                        await WriteSimpleResponseAsync(stream, 400, "Bad Request");
                        return;
                    }

                    var method = requestParts[0].ToUpperInvariant();
                    if (method != "GET" && method != "HEAD")
                    {
                        await WriteSimpleResponseAsync(stream, 405, "Method Not Allowed");
                        return;
                    }

                    var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    string? line;
                    while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                    {
                        var colon = line.IndexOf(':');
                        if (colon > 0)
                            headers[line.Substring(0, colon).Trim()] = line.Substring(colon + 1).Trim();
                    }

                    var requestTarget = requestParts[1];
                    if (Uri.TryCreate(requestTarget, UriKind.Absolute, out var absoluteUri))
                        requestTarget = absoluteUri.PathAndQuery;

                    var queryIndex = requestTarget.IndexOf('?');
                    var rawPath = queryIndex >= 0 ? requestTarget.Substring(0, queryIndex) : requestTarget;
                    var segments = rawPath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length < 3 || !string.Equals(segments[0], "lesson", StringComparison.OrdinalIgnoreCase))
                    {
                        await WriteSimpleResponseAsync(stream, 404, "Not Found");
                        return;
                    }

                    if (!Lessons.TryGetValue(segments[1], out var lesson))
                    {
                        await WriteSimpleResponseAsync(stream, 404, "Not Found");
                        return;
                    }

                    var encodedRelativePath = string.Join("/", segments.Skip(2));
                    var relativePath = Uri.UnescapeDataString(encodedRelativePath)
                        .Replace('/', Path.DirectorySeparatorChar);

                    if (Path.IsPathRooted(relativePath))
                    {
                        await WriteSimpleResponseAsync(stream, 403, "Forbidden");
                        return;
                    }

                    var rootWithSeparator = lesson.RootFolder.TrimEnd(
                        Path.DirectorySeparatorChar,
                        Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
                    var requestedPath = Path.GetFullPath(Path.Combine(lesson.RootFolder, relativePath));
                    if (!requestedPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
                        || !File.Exists(requestedPath))
                    {
                        await WriteSimpleResponseAsync(stream, 404, "Not Found");
                        return;
                    }

                    await WriteFileResponseAsync(stream, method, requestedPath, headers);
                }
            }
            catch (IOException)
            {
                // Browsers may cancel speculative or superseded requests.
            }
            catch (ObjectDisposedException)
            {
                // The app or browser closed while a response was being streamed.
            }
            catch (Exception ex)
            {
                WebViewLog.Error($"E-LEARNING local server request failed error='{ex}'");
            }
        }

        private static async Task WriteFileResponseAsync(
            NetworkStream stream,
            string method,
            string filePath,
            IDictionary<string, string> requestHeaders)
        {
            var fileInfo = new FileInfo(filePath);
            long start = 0;
            long end = fileInfo.Length - 1;
            var isPartial = false;

            if (requestHeaders.TryGetValue("Range", out var rangeHeader))
            {
                if (!TryParseRange(rangeHeader, fileInfo.Length, out start, out end))
                {
                    var invalidHeaders =
                        $"Content-Range: bytes */{fileInfo.Length}\r\n" +
                        "Accept-Ranges: bytes\r\n";
                    await WriteHeadersAsync(stream, 416, "Range Not Satisfiable", invalidHeaders, 0, "text/plain; charset=utf-8");
                    return;
                }

                isPartial = true;
            }

            var contentLength = fileInfo.Length == 0 ? 0 : end - start + 1;
            var extraHeaders = "Accept-Ranges: bytes\r\nCache-Control: private, max-age=3600\r\n";
            if (isPartial)
                extraHeaders += $"Content-Range: bytes {start}-{end}/{fileInfo.Length}\r\n";

            await WriteHeadersAsync(
                stream,
                isPartial ? 206 : 200,
                isPartial ? "Partial Content" : "OK",
                extraHeaders,
                contentLength,
                GetContentType(filePath));

            if (method == "HEAD" || contentLength == 0)
                return;

            using (var file = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                64 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                file.Position = start;
                var buffer = new byte[64 * 1024];
                long remaining = contentLength;
                while (remaining > 0)
                {
                    var count = await file.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                    if (count <= 0)
                        break;

                    await stream.WriteAsync(buffer, 0, count);
                    remaining -= count;
                }
            }
        }

        private static bool TryParseRange(string value, long fileLength, out long start, out long end)
        {
            start = 0;
            end = Math.Max(0, fileLength - 1);

            if (fileLength <= 0 || string.IsNullOrWhiteSpace(value)
                || !value.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase)
                || value.IndexOf(',') >= 0)
                return false;

            var range = value.Substring("bytes=".Length).Trim();
            var dash = range.IndexOf('-');
            if (dash < 0)
                return false;

            var startText = range.Substring(0, dash).Trim();
            var endText = range.Substring(dash + 1).Trim();

            if (startText.Length == 0)
            {
                if (!long.TryParse(endText, NumberStyles.None, CultureInfo.InvariantCulture, out var suffixLength)
                    || suffixLength <= 0)
                    return false;

                suffixLength = Math.Min(suffixLength, fileLength);
                start = fileLength - suffixLength;
                end = fileLength - 1;
                return true;
            }

            if (!long.TryParse(startText, NumberStyles.None, CultureInfo.InvariantCulture, out start)
                || start < 0
                || start >= fileLength)
                return false;

            if (endText.Length == 0)
            {
                end = fileLength - 1;
                return true;
            }

            if (!long.TryParse(endText, NumberStyles.None, CultureInfo.InvariantCulture, out end)
                || end < start)
                return false;

            end = Math.Min(end, fileLength - 1);
            return true;
        }

        private static async Task WriteSimpleResponseAsync(NetworkStream stream, int statusCode, string reason)
        {
            var body = Encoding.UTF8.GetBytes(reason);
            await WriteHeadersAsync(stream, statusCode, reason, "Cache-Control: no-store\r\n", body.Length, "text/plain; charset=utf-8");
            await stream.WriteAsync(body, 0, body.Length);
        }

        private static async Task WriteHeadersAsync(
            NetworkStream stream,
            int statusCode,
            string reason,
            string extraHeaders,
            long contentLength,
            string contentType)
        {
            var headers = Encoding.ASCII.GetBytes(
                $"HTTP/1.1 {statusCode} {reason}\r\n" +
                $"Content-Type: {contentType}\r\n" +
                $"Content-Length: {contentLength}\r\n" +
                extraHeaders +
                "Connection: close\r\n\r\n");
            await stream.WriteAsync(headers, 0, headers.Length);
        }

        private static string GetContentType(string path)
        {
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".html":
                case ".htm": return "text/html; charset=utf-8";
                case ".js": return "application/javascript; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".json": return "application/json; charset=utf-8";
                case ".xml": return "application/xml; charset=utf-8";
                case ".svg": return "image/svg+xml";
                case ".png": return "image/png";
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".webp": return "image/webp";
                case ".ico": return "image/x-icon";
                case ".mp4": return "video/mp4";
                case ".webm": return "video/webm";
                case ".ogv": return "video/ogg";
                case ".mp3": return "audio/mpeg";
                case ".m4a": return "audio/mp4";
                case ".wav": return "audio/wav";
                case ".ogg": return "audio/ogg";
                case ".woff": return "font/woff";
                case ".woff2": return "font/woff2";
                case ".ttf": return "font/ttf";
                case ".eot": return "application/vnd.ms-fontobject";
                case ".vtt": return "text/vtt; charset=utf-8";
                case ".pdf": return "application/pdf";
                default: return "application/octet-stream";
            }
        }
    }
}
