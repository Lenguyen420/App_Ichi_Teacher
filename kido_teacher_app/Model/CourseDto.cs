using Newtonsoft.Json;

namespace kido_teacher_app.Model
{
    public class CourseDto
    {
        public string id { get; set; }
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int credits { get; set; }
        public string status { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string thumbnailImage { get; set; }
        public string note { get; set; }

        [JsonProperty("image")]
        public string image { get; set; }

        // ⭐ DÙNG CHUNG GET + POST
        [JsonProperty("classId")]
        public string ClassId { get; set; }

        // ⭐ DÙNG CHO HIỂN THỊ
        [JsonProperty("classCode")]
        public string ClassCode { get; set; }
    }

    public class CourseCreateDto
    {
        public string code { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int credits { get; set; }
        public string status { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string thumbnailImage { get; set; }
        public string note { get; set; }
        public string image { get; set; }

        [JsonProperty("classId")]      // ⭐ ép serialize đúng key backend cần
        public string ClassId { get; set; }
    }
}
