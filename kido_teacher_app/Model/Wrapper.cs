namespace kido_teacher_app.Model
{
    public class Wrapper<T>
    {
        public int page { get; set; }
        public int size { get; set; }
        public int total { get; set; }
        public T data { get; set; }
    }
}
