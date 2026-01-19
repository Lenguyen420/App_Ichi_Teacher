namespace kido_teacher_app.Model
{
    public class GroupUserItem
    {

        // POST /user-groups/group/{groupId}/members-with-role
        // Đại diện 1 user + role trong group
        public string userId { get; set; }
        public string role { get; set; }
    }
}