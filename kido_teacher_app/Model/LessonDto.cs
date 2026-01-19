
namespace kido_teacher_app.Model;
public class LessonDto
{
    public string id { get; set; }
    public string code { get; set; }
    public string title { get; set; }
    public string note { get; set; }
    public int orderColumn { get; set; }
    public string avatar { get; set; }

    public string classId { get; set; }
    public string courseId { get; set; }
    public List<ResourceDto> resources { get; set; }
}

public class ResourceDto
{
    public string type { get; set; }     // PDF / VIDEO / LESSON_PLAN
    public string source { get; set; }   // ONLINE / OFFLINE
    public string url { get; set; }
}
