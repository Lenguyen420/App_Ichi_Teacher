using System;
using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class AttemptReportGroupDto
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }

    public class AttemptReportStudentDto
    {
        public string id { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string userName { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public string studentGroupId { get; set; } = string.Empty;
        public string studentGroupName { get; set; } = string.Empty;
    }

    public class StudentAttemptReportDto
    {
        public string groupId { get; set; } = string.Empty;
        public AttemptReportStudentDto? student { get; set; }
        public DateTime? fromDate { get; set; }
        public DateTime? toDate { get; set; }
        public AttemptReportSummaryDto? summary { get; set; }
        public List<AttemptReportTrendDto> trend { get; set; } = new List<AttemptReportTrendDto>();
        public List<AttemptHistoryDto> attempts { get; set; } = new List<AttemptHistoryDto>();
        public int page { get; set; }
        public int limit { get; set; }
        public int total { get; set; }
    }

    public class AttemptReportSummaryDto
    {
        public int totalAttempts { get; set; }
        public double? averageScore { get; set; }
        public double? highestScore { get; set; }
        public DateTime? latestAttemptAt { get; set; }
    }

    public class AttemptReportTrendDto
    {
        public DateTime? date { get; set; }
        public int attemptCount { get; set; }
        public double? averageScore { get; set; }
        public double? highestScore { get; set; }
    }

    public class AttemptHistoryDto
    {
        public string attemptId { get; set; } = string.Empty;
        public string questionBankId { get; set; } = string.Empty;
        public string questionBankName { get; set; } = string.Empty;
        public string examSetId { get; set; } = string.Empty;
        public string examSetName { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public DateTime? startedAt { get; set; }
        public DateTime? submittedAt { get; set; }
        public double? score { get; set; }
    }
}
