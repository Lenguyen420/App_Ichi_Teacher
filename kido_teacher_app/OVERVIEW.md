# Kido Teacher App - Overview (sơ lược)

## Mục tiêu
Ứng dụng WinForms cho giáo viên: đăng nhập, xem thông tin tài khoản, truy cập giáo án/bài giảng, tải tài nguyên và dùng offline.

## Luồng chính
1. `Program.Main` khởi tạo cấu hình, bắt lỗi global, gọi `AuthService.TryLoginWithSavedTokenAsync()`.
2. Nếu có token hợp lệ → vào `Main_Form`.
3. Nếu chưa login → mở `Form_Login`.
4. Sau khi login thành công → vào `Main_Form`.

## Cấu hình & đường dẫn
- `App.config`: `ApiBaseUrl`, `VersionJsonUrl`, `UpdateVersionApiUrl`, `UpdateVersionApiToken`.
- `AppConfig`:
- `AppDataRoot` (LocalApplicationData) và `AppDataRoaming` (ApplicationData).
- Thư mục: `DownloadFolder`, `LectureExtractFolder`, `CacheFolder`, `CourseImageCacheFolder`, `LectureImageCacheFolder`, `ClassImageCacheFolder`.
- Token nhớ đăng nhập: `AppDataRoaming\token.txt`.

## UI chính
- `Main_Form`: menu điều hướng → `UC_GioiThieu`, `UC_TaiKhoan`, `UC_GiaoAn`.
- `Form_Login`: đăng nhập giáo viên, lưu token nếu chọn “remember”.
- `UC_GiaoAn`: hiển thị danh sách lớp (card), load ảnh qua cache.

## Services (API/logic)
- `AuthService`: login admin/teacher, lưu token, kiểm tra token, parse JWT claims.
- `ClassService`, `CourseService`, `LectureService`: lấy danh sách/chi tiết theo lớp/khoá.
- `LectureDownloadService`: tải zip bài giảng, giải nén.
- `VersionCheckService`: check version server, mở URL update nếu bắt buộc.
- `ImageCacheService` + `Class/Course/LectureImageCacheService`: cache ảnh về local.
- `OfflineResourceService`: quản lý file offline theo lecture.

## Models
Các DTO phản ánh dữ liệu API: `ClassDto`, `CourseDto`, `LectureDto`, `UserDto`, `PagedResult`, `ApiResponse`, v.v.

## Điểm cần lưu ý
- Có 2 lớp `AuthSession` ở 2 namespace:
- `kido_teacher_app.Config.AuthSession` (chỉ `AccessToken`).
- `kido_teacher_app.Services.AuthSession` (có `AccessToken`, `UserId`, `Role`).
- Nhiều nơi dùng `Services.AuthSession`, nhưng `ImageCacheService` đang gọi `Config.AuthSession`.
- Dễ gây nhầm lẫn hoặc lỗi nếu token không sync.

## File/Folder quan trọng
- `kido_teacher_app\Program.cs`: entrypoint + auto-login.
- `kido_teacher_app\Forms\Auth\Login_Form.cs`: login flow.
- `kido_teacher_app\Forms\Main\Main_Form.cs`: điều hướng UI.
- `kido_teacher_app\Services\*.cs`: API + business logic.
- `kido_teacher_app\Config\*.cs`: cấu hình chung.

## Gợi ý hướng đọc tiếp
- Nếu sửa auth: xem `AuthService`, `Login_Form`, `Program`.
- Nếu sửa tải giáo án/offline: xem `LectureService`, `LectureDownloadService`, `OfflineResourceService`.
- Nếu sửa UI menu/luồng chính: xem `Main_Form` và các `UC_*` trong `Forms\Main\Page`.
