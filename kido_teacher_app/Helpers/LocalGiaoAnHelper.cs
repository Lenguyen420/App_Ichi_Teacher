using System;
using System.IO;

namespace kido_teacher_app.Helpers
{
    public static class LocalGiaoAnHelper
    {
        /// <summary>
        /// Trả về thư mục lưu giáo án offline trong Resources/GiaoAn
        /// </summary>
        public static string GetLectureFolder(string lectureId)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources",
                "GiaoAn",
                lectureId
            );
        }
    }
}
