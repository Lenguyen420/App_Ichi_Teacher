using System.Collections.Generic;

namespace kido_teacher_app.Model
{
    public class QuestionAnswerDto
    {
        // ================== DÙNG CHUNG ==================
        public int QuestionIndex { get; set; }      // Số thứ tự câu hỏi (1,2,3...)
        public string QuestionId { get; set; }      // Id backend (nếu có)

        // ================== NỘI DUNG ==================
        public string QuestionText { get; set; }    // Nội dung câu hỏi

        // Key: A/B/C/D – Value: nội dung đáp án
        public Dictionary<string, string> Answers { get; set; }
            = new Dictionary<string, string>();

        // ================== USER TRẢ LỜI ==================
        public string SelectedAnswer { get; set; }  // A/B/C/D hoặc null

        public bool IsAnswered =>
            !string.IsNullOrEmpty(SelectedAnswer);

        // ================== CHẤM ĐIỂM ==================
        public string CorrectAnswer { get; set; }   // A/B/C/D

        public bool IsCorrect =>
            IsAnswered && SelectedAnswer == CorrectAnswer;

        public int Score { get; set; }               // Điểm câu này (0 / 1 / tuỳ cấu hình)

        // ================== TỰ LUẬN (MỞ RỘNG) ==================
        public bool IsEssay { get; set; }            // true = tự luận
        public string EssayAnswer { get; set; }      // Nội dung tự luận HS nhập
        public int EssayScore { get; set; }          // Điểm GV chấm

        // ================== TRẠNG THÁI UI ==================
        public bool IsMarked { get; set; }           // Đánh dấu sao ⭐
        public bool IsSkipped { get; set; }           // Bỏ qua (chưa chọn)

        // ================== THỜI GIAN (OPTIONAL) ==================
        public int TimeSpentSeconds { get; set; }    // Thời gian làm câu này

         
    }
}
