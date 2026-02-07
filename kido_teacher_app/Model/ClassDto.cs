using Newtonsoft.Json;

namespace kido_teacher_app.Model
{
    public class ClassDto
    {
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public int orderNumber { get; set; }
        public string displayType { get; set; }

        [JsonProperty("avatarImage")]
        // ⭐ FIELD ẢNH — PHẢI CÓ CẢ 2
        public string avatarImage { get; set; }
        public string currentImage { get; set; }

        // thêm fallback
        [JsonProperty("avatar")]
        public string avatar { get; set; }

        [JsonProperty("imageUrl")]
        public string imageUrl { get; set; }

        public string note { get; set; }
        public List<ExamDto> exams { get; set; }
    }


    public class ExamDto
    {
        public string title { get; set; }        // Tên đề
        public string subject { get; set; }      // Ví dụ: Tiếng Anh 1 / Sách Family and Friends
        public string level { get; set; }        // Dễ / Khó
        public string type { get; set; }         // Phiếu bài tập
        public int time { get; set; }             // phút
    }


}
