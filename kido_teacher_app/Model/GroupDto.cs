using Newtonsoft.Json;
using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class GroupDto
    {
        [JsonProperty("id")]
        public string id { get; set; }        // UUID
       
        public int code { get; set; }         // Mã nhóm
        public string name { get; set; }      // Tên nhóm

        public string createdAt { get; set; }
        public string updatedAt { get; set; }

        public string createdBy { get; set; }
        public string updatedBy { get; set; }

        public List<object> users { get; set; }
        public List<GroupMemberModel> members { get; set; }
        //public List<GroupMemberModel> members { get; set; }
        // public List<GroupMemberModel> members { get; set; } = new();
    }
}
