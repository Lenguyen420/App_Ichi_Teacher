namespace kido_teacher_app.Model
{
    // =====================================================
    // INIT UPLOAD
    // =====================================================
    public class InitUploadRequest
    {
        public string fileName { get; set; }
        public long fileSize { get; set; }
        public int totalChunks { get; set; }
        public string? mimeType { get; set; }
        public string? fileType { get; set; }
    }

    public class InitUploadResponse
    {
        public string uploadId { get; set; }
        public string fileName { get; set; }
        public int totalChunks { get; set; }
    }

    // =====================================================
    // UPLOAD CHUNK
    // =====================================================
    public class UploadChunkResponse
    {
        public int received { get; set; }
        public int total { get; set; }
    }

    // =====================================================
    // COMPLETE UPLOAD
    // =====================================================
    public class CompleteUploadRequest
    {
        public string uploadId { get; set; }
    }

    public class CompleteUploadResponse
    {
        public bool success { get; set; }
        public string path { get; set; }
        public string url { get; set; }
        public string fileName { get; set; }
        public long fileSize { get; set; }
        public string mimeType { get; set; }
    }
}
