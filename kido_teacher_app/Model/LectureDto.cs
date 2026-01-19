using System;
using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class LectureDto
    {
        public string id { get; set; }
        public string code { get; set; }
        public string title { get; set; }
        public string note { get; set; }
        public int orderColumn { get; set; }
        public string avatar { get; set; }
        public string classId { get; set; }
        public string courseId { get; set; }

        public List<LectureResourceDto> resources { get; set; }

        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
