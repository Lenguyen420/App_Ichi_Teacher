namespace kido_teacher_app.Model
{
    public class AddUsersToGroupRequest
    {
        public List<UserInGroup> users { get; set; } = new();
    }

    public class UserInGroup
    {
        public string userId { get; set; }
        public string role { get; set; } = "MEMBER";
    }
}
