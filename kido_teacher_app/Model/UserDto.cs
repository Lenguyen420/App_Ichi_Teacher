//using Newtonsoft.Json;

//namespace kido_teacher_app.Model
//{
//    public class UserDto
//    {
//        [JsonProperty("id")]
//        public string id { get; set; }



//        public string userName { get; set; }
//        public string fullName { get; set; }
//        public string email { get; set; }
//        public string role { get; set; }

//        public string phoneNumber { get; set; }
//        public string birthday { get; set; }
//        public string gender { get; set; }
//        public string citizenId { get; set; }
//        public string address { get; set; }
//        public string note { get; set; }
//        public string userType { get; set; }
//        public string activatedDate { get; set; }
//        public string expiredDate { get; set; }
//        public bool isLinkedAccount { get; set; }

//        public string status { get; set; }
//        public List<string> groupIds { get; set; }

//    }
//}
using Newtonsoft.Json;
using System.Collections.Generic;

namespace kido_teacher_app.Model   // ⭐ BẮT BUỘC PHẢI CÓ
{
    public class UserDto
    {
        [JsonProperty("id")]
        public string id { get; set; } = string.Empty;

        public string userName { get; set; } = string.Empty;
        public string fullName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string role { get; set; } = string.Empty;

        public string phoneNumber { get; set; } = string.Empty;
        public string birthday { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string citizenId { get; set; } = string.Empty;
        public string address { get; set; } = string.Empty;
        public string note { get; set; } = string.Empty;
        public string userType { get; set; } = string.Empty;
        public string activatedDate { get; set; } = string.Empty;
        public string expiredDate { get; set; } = string.Empty;

        public bool isLinkedAccount { get; set; }
        public bool isDisabled { get; set; }
        public string status { get; set; } = string.Empty;

        // ===== QUYỀN =====
        public bool canCreateTeacherCode { get; set; }
        public bool canCreateAdminCode { get; set; }
        public bool canAddLesson { get; set; }
        public bool canUpdateLesson { get; set; }
        public bool canManageLesson { get; set; }
        public bool canManageAccount { get; set; }

        // ===== NHÓM =====
        public List<string> groupIds { get; set; } = new();
    }
}
