namespace kido_teacher_app.Model
{
    public class ClassCreateDto
    {
        public string code { get; set; }
        public string name { get; set; }
        public int orderNumber { get; set; }
        public string displayType { get; set; }

        public string currentImage { get; set; }

        public string note { get; set; }
    }
}
