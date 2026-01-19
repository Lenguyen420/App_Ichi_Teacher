namespace kido_teacher_app.Models
{
    public class LectureOfflineCache
    {
        public string LectureId { get; set; } = "";

        public string? PdfPath { get; set; }
        public string? VideoPath { get; set; }
        public string? ElearningPath { get; set; }
    }
}
