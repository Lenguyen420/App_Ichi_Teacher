using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class CreateUserRequest
    {
        public string userName { get; set; }
        public string password { get; set; }   // ❗ chỉ dùng khi tạo

        public string email { get; set; }
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public string birthday { get; set; }
        public string gender { get; set; }
        public string citizenId { get; set; }
        public string address { get; set; }
        public string note { get; set; }

        public string userType { get; set; }
        public string activatedDate { get; set; }
        public string expiredDate { get; set; }

        public bool canCreateTeacherCode { get; set; }
        public bool canCreateAdminCode { get; set; }
        public bool canAddLesson { get; set; }
        public bool canUpdateLesson { get; set; }
        public bool canManageLesson { get; set; }
        public bool canManageAccount { get; set; }
        public bool isDisabled { get; set; }
        public bool isLinkedAccount { get; set; }
        public string status { get; set; }

        public List<string> groupIds { get; set; }
    }
}
