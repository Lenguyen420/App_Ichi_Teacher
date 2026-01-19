using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class LectureCreateDto
    {
        public string code { get; set; }
        public string title { get; set; }
        public string note { get; set; }
        public int orderColumn { get; set; }
        public string avatar { get; set; }
        public string courseId { get; set; }

        public List<LectureResourceDto> resources { get; set; } = new();
    }

    public class LectureResourceDto
    {
        public string type { get; set; }     // PDF | VIDEO | LESSON
        public string source { get; set; }   // ONLINE | OFFLINE
        public string url { get; set; }
    }
}




