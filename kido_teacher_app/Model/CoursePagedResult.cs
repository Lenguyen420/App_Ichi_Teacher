using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kido_teacher_app.Model
{
    internal class CoursePagedResult
    {
        public int page { get; set; }
        public int size { get; set; }
        public int total { get; set; }
        public List<CourseDto> data { get; set; }
    }
}
