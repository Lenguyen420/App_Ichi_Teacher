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
    }

}
