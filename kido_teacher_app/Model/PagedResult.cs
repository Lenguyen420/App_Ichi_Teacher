//using System.Collections.Generic;

//namespace kido_teacher_app.Model
//{
//    public class PagedResult<T>
//    {
//        public List<T> items { get; set; }   // 🔴 PHẢI CÓ
//        public int total { get; set; }
//        public int page { get; set; }
//        public int limit { get; set; }
//    }
//}

namespace kido_teacher_app.Model
{
    public class PagedResult<T>
    {
        public List<T> items { get; set; }
        public int total { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }

        public List<T> data { get; set; }
        public int limit { get; set; }
    }
}