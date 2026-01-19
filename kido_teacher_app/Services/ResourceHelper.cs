using kido_teacher_app.Model;
using System.Collections.Generic;

namespace kido_teacher_app.Services
{
    public class ResourceGroup
    {
        public string? PdfOnline { get; set; }
        public string? VideoOnline { get; set; }
        public string? LessonOnline { get; set; }

        public string? PdfOffline { get; set; }
        public string? VideoOffline { get; set; }
        public string? LessonOffline { get; set; }
    }

    public static class ResourceHelper
    {
        // 🔥 NHẬN LectureResourceDto
        public static ResourceGroup Parse(List<LectureResourceDto> resources)
        {
            var res = new ResourceGroup();

            if (resources == null)
                return res;

            foreach (var r in resources)
            {
                var type = r.type?.ToUpper();
                var source = r.source?.ToUpper();

                if (type == "PDF" && source == "ONLINE")
                    res.PdfOnline = r.url;

                if (type == "VIDEO" && source == "ONLINE")
                    res.VideoOnline = r.url;

                if (type == "LESSON" && source == "ONLINE")
                    res.LessonOnline = r.url;

                if (type == "PDF" && source == "OFFLINE")
                    res.PdfOffline = r.url;

                if (type == "VIDEO" && source == "OFFLINE")
                    res.VideoOffline = r.url;

                if (type == "LESSON" && source == "OFFLINE")
                    res.LessonOffline = r.url;
            }

            return res;
        }
    }
}
