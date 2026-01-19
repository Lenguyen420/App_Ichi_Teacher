using kido_teacher_app.Model;

public class LectureUpdateDto
{
    public string code { get; set; }
    public string title { get; set; }
    public string note { get; set; }
    public int orderColumn { get; set; }
    public string avatar { get; set; }
    public string classId { get; set; }
    public string courseId { get; set; }
    public string groupId { get; set; }
    public List<ResourceDto> resources { get; set; }
}
public class ResourceDto
{
    public string type { get; set; }
    public string source { get; set; }
    public string url { get; set; }
}