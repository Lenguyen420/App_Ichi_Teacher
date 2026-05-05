using kido_teacher_app.Config;
using kido_teacher_app.Model;
using kido_teacher_app.Shared.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace kido_teacher_app.Services
{
    public sealed class AttemptReportApiException : Exception
    {
        public AttemptReportApiException(HttpStatusCode statusCode, string message, string responseBody)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseBody = responseBody;
        }

        public HttpStatusCode StatusCode { get; }

        public string ResponseBody { get; }
    }

    public static class AttemptReportService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(AppConfig.ApiBaseUrl)
        };

        public static Task<List<AttemptReportGroupDto>> GetGroupsAsync()
        {
            return ExecuteAsync<List<AttemptReportGroupDto>>(ApiRoutes.REPORT_ATTEMPT_GROUPS);
        }

        public static Task<List<AttemptReportStudentDto>> GetStudentsByGroupAsync(string groupId)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId is required.", nameof(groupId));

            return ExecuteAsync<List<AttemptReportStudentDto>>(ApiRoutes.ReportAttemptStudents(groupId));
        }

        /// <summary>
        /// Get student report with advanced filtering by zone, school, group, and student
        /// Supports studentId=all for viewing all students in selected scope
        /// </summary>
        public static Task<StudentAttemptReportDto> GetStudentReportAsync(
            string? zoneId = null,
            string? schoolId = null,
            string? groupId = null,
            string? studentId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int limit = 10)
        {
            // At least one of zoneId, schoolId, or groupId is required
            if (string.IsNullOrWhiteSpace(zoneId) && string.IsNullOrWhiteSpace(schoolId) && string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("At least one of zoneId, schoolId, or groupId is required.");

            var query = new List<string>
            {
                $"page={Math.Max(1, page).ToString(CultureInfo.InvariantCulture)}",
                $"limit={Math.Max(1, limit).ToString(CultureInfo.InvariantCulture)}"
            };

            if (!string.IsNullOrWhiteSpace(zoneId))
                query.Add($"zoneId={Uri.EscapeDataString(zoneId)}");

            if (!string.IsNullOrWhiteSpace(schoolId))
                query.Add($"schoolId={Uri.EscapeDataString(schoolId)}");

            if (!string.IsNullOrWhiteSpace(groupId))
                query.Add($"groupId={Uri.EscapeDataString(groupId)}");

            // If studentId is not specified, default to "all" for group scope
            var studentParam = string.IsNullOrWhiteSpace(studentId) ? "all" : studentId;
            query.Add($"studentId={Uri.EscapeDataString(studentParam)}");

            if (fromDate.HasValue)
                query.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (toDate.HasValue)
                query.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            return ExecuteAsync<StudentAttemptReportDto>($"{ApiRoutes.REPORT_ATTEMPT_STUDENT}?{string.Join("&", query)}");
        }

        /// <summary>
        /// Legacy method - Get student report (kept for backward compatibility)
        /// </summary>
        [Obsolete("Use GetStudentReportAsync with named parameters instead")]
        public static Task<StudentAttemptReportDto> GetStudentReportAsync(
            string groupId,
            string? studentId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId is required.", nameof(groupId));

            // If no student selected, get group report; otherwise get individual student report
            if (string.IsNullOrWhiteSpace(studentId))
                return GetGroupReportAsync(groupId, fromDate, toDate, page, limit);

            return GetStudentReportAsync(zoneId: null, schoolId: null, groupId: groupId, studentId: studentId, fromDate: fromDate, toDate: toDate, page: page, limit: limit);
        }

        public static Task<StudentAttemptReportDto> GetGroupReportAsync(
            string groupId,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int limit = 10)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId is required.", nameof(groupId));

            var query = new List<string>
            {
                $"groupId={Uri.EscapeDataString(groupId)}",
                $"page={Math.Max(1, page).ToString(CultureInfo.InvariantCulture)}",
                $"limit={Math.Max(1, limit).ToString(CultureInfo.InvariantCulture)}"
            };

            if (fromDate.HasValue)
                query.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            if (toDate.HasValue)
                query.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

            return ExecuteAsync<StudentAttemptReportDto>($"{ApiRoutes.REPORT_ATTEMPT_GROUPS}?{string.Join("&", query)}");
        }

        /// <summary>
        /// Get zone details with schools and student groups hierarchy
        /// </summary>
        public static Task<ZoneDetailPayload> GetZoneDetailAsync(int page = 1, int size = 10, string? search = null)
        {
            var query = new List<string>
            {
                "isGetAllDetail=true",
                $"page={Math.Max(1, page).ToString(CultureInfo.InvariantCulture)}",
                $"size={Math.Max(1, size).ToString(CultureInfo.InvariantCulture)}"
            };

            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");

            return ExecuteAsync<ZoneDetailPayload>($"{ApiRoutes.ZONE}?{string.Join("&", query)}");
        }

        private static async Task<T> ExecuteAsync<T>(string requestUri)
        {
            EnsureAuthorized();

            try
            {
                using var response = await client.GetAsync(requestUri);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new AttemptReportApiException(
                        response.StatusCode,
                        ExtractErrorMessage(responseBody),
                        responseBody);
                }

                OfflineState.SetOffline(false);

                var payload = DeserializePayload<T>(responseBody);
                if (payload == null)
                    throw new InvalidOperationException("Response payload is empty.");

                return payload;
            }
            catch (Exception ex) when (IsNetworkException(ex))
            {
                OfflineState.SetOffline(true);
                throw;
            }
        }

        private static void EnsureAuthorized()
        {
            if (string.IsNullOrWhiteSpace(AuthSession.AccessToken))
                throw new UnauthorizedAccessException("Token khong ton tai.");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", AuthSession.AccessToken);
        }

        private static T? DeserializePayload<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return default;

            var token = JToken.Parse(json);
            if (token.Type == JTokenType.Object)
            {
                var dataToken = token["data"];
                if (dataToken != null && (token["success"] != null || token["message"] != null))
                    return dataToken.Type == JTokenType.Null ? default : dataToken.ToObject<T>();
            }

            return token.ToObject<T>();
        }

        private static string ExtractErrorMessage(string responseBody)
        {
            if (string.IsNullOrWhiteSpace(responseBody))
                return "Khong nhan duoc phan hoi hop le tu may chu.";

            try
            {
                var token = JToken.Parse(responseBody);
                var message = token["message"]?.Value<string>()
                    ?? token["error"]?.Value<string>()
                    ?? token["errors"]?.First?.ToString();

                return string.IsNullOrWhiteSpace(message) ? responseBody : message;
            }
            catch
            {
                return responseBody;
            }
        }

        private static bool IsNetworkException(Exception ex)
        {
            if (ex is HttpRequestException || ex is TaskCanceledException || ex is System.Net.Sockets.SocketException)
                return true;

            return ex.InnerException != null && IsNetworkException(ex.InnerException);
        }

        public static async Task<byte[]> ExportGroupReportAsync(
            string groupId,
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId is required.", nameof(groupId));

            EnsureAuthorized();

            try
            {
                var query = new List<string>();
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

                var url = $"/report/attempt/classes/export-excel?groupIds={Uri.EscapeDataString(groupId)}";
                if (query.Count > 0)
                    url += $"&{string.Join("&", query)}";

                using var response = await client.PostAsync(url, new StringContent("{\"groupIds\": [\"" + Uri.EscapeDataString(groupId) + "\"]}", System.Text.Encoding.UTF8, "application/json"));
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    throw new AttemptReportApiException(
                        response.StatusCode,
                        ExtractErrorMessage(responseBody),
                        responseBody);
                }

                OfflineState.SetOffline(false);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex) when (IsNetworkException(ex))
            {
                OfflineState.SetOffline(true);
                throw;
            }
        }

        /// <summary>
        /// Xuất file Excel sheet "KET QUA LOP" cho một lớp/nhóm học sinh.
        /// </summary>
        public static async Task<byte[]> ExportClassSheetAsync(
            string groupId,
            string? examSetId = null,
            string? questionBankId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(groupId))
                throw new ArgumentException("groupId is required.", nameof(groupId));

            EnsureAuthorized();

            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(examSetId))
                    query.Add($"examSetId={Uri.EscapeDataString(examSetId)}");
                if (!string.IsNullOrWhiteSpace(questionBankId))
                    query.Add($"questionBankId={Uri.EscapeDataString(questionBankId)}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

                var url = $"/report/attempt/groups/{Uri.EscapeDataString(groupId)}/export-class-sheet";
                if (query.Count > 0)
                    url += $"?{string.Join("&", query)}";

                using var response = await client.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    throw new AttemptReportApiException(
                        response.StatusCode,
                        ExtractErrorMessage(responseBody),
                        responseBody);
                }

                OfflineState.SetOffline(false);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex) when (IsNetworkException(ex))
            {
                OfflineState.SetOffline(true);
                throw;
            }
        }

        /// <summary>
        /// Xuất file Excel sheet thống kê điểm toàn trường theo lớp/nhóm.
        /// Endpoint: GET /report/attempt/schools/{schoolId}/export-school-stat-sheet
        /// </summary>
        public static async Task<byte[]> ExportSchoolStatSheetAsync(
            string schoolId,
            string? examSetId = null,
            string? questionBankId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            if (string.IsNullOrWhiteSpace(schoolId))
                throw new ArgumentException("schoolId is required.", nameof(schoolId));

            EnsureAuthorized();

            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(examSetId))
                    query.Add($"examSetId={Uri.EscapeDataString(examSetId)}");
                if (!string.IsNullOrWhiteSpace(questionBankId))
                    query.Add($"questionBankId={Uri.EscapeDataString(questionBankId)}");
                if (fromDate.HasValue)
                    query.Add($"fromDate={fromDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");
                if (toDate.HasValue)
                    query.Add($"toDate={toDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}");

                var url = $"/report/attempt/schools/{Uri.EscapeDataString(schoolId)}/export-school-stat-sheet";
                if (query.Count > 0)
                    url += $"?{string.Join("&", query)}";

                using var response = await client.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    throw new AttemptReportApiException(
                        response.StatusCode,
                        ExtractErrorMessage(responseBody),
                        responseBody);
                }

                OfflineState.SetOffline(false);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex) when (IsNetworkException(ex))
            {
                OfflineState.SetOffline(true);
                throw;
            }
        }
    }
}
