using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    /// <summary>
    /// DTO for Zone Detail API response wrapper
    /// </summary>
    public sealed class ZoneDetailPayload
    {
        public List<ZoneDetailItem> data { get; set; } = new();
        public int page { get; set; }
        public int size { get; set; }
        public int total { get; set; }
    }

    /// <summary>
    /// Zone with its schools
    /// </summary>
    public sealed class ZoneDetailItem
    {
        public IdNameCode zone { get; set; }
        public List<SchoolNode> schools { get; set; } = new();
    }

    /// <summary>
    /// School with its student groups
    /// </summary>
    public class SchoolNode : IdNameCode
    {
        public List<StudentGroupNode> studentGroups { get; set; } = new();
    }

    /// <summary>
    /// Student group node
    /// </summary>
    public class StudentGroupNode : IdNameCode
    {
    }

    /// <summary>
    /// Base class for id, name, and code
    /// </summary>
    public class IdNameCode
    {
        public string id { get; set; }
        public string name { get; set; }
        public object code { get; set; }
    }
}
