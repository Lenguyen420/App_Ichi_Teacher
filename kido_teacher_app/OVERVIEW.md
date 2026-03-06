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
- SQLite cache: `DbFolder`, `DbPath` (app_cache.db).
- Token nhớ đăng nhập: `AppDataRoaming\token.txt`.
- OfflineResourceService dùng đường dẫn riêng: `ApplicationData\KIDO\OfflineLessons\{lectureId}`.

## UI chính
- `Main_Form`: menu điều hướng → `UC_GioiThieu`, `UC_TaiKhoan`, `UC_GiaoAn`.
- `Form_Login`: đăng nhập giáo viên, lưu token nếu chọn “remember”.
- `UC_GiaoAn`: hiển thị danh sách lớp (card), load ảnh qua cache.
- Giao án theo tháng: `UC_GiaoAnTheoThang`, chi tiết `UC_GiaoAn_TheoThangChiTiet`.
- Viewer: `Form_PdfViewer`, `Form_ElearningViewer`.
- Giới thiệu: có form chỉnh sửa thông tin (`Form_ChinhSuaThongTinCongTy`).

## Services (API/logic)
- `AuthService`: login admin/teacher, lưu token, kiểm tra token, parse JWT claims.
- `ClassService`, `CourseService`, `LectureService`: lấy danh sách/chi tiết theo lớp/khoá.
- `LectureDownloadService`: tải zip bài giảng, giải nén.
- `LectureResourceService`: map file pdf/video/elearning trong thư mục giải nén.
- `VersionCheckService`: check version server, mở URL update nếu bắt buộc.
- `ApiDownloadService`: download bytes với Bearer token.
- `DbCacheService`: cache JSON vào SQLite (api_cache).
- `OfflinePrefetchService`: prefetch lớp/khoá/bài giảng + ảnh vào cache khi online.
- `UserService`: lấy user, lọc theo nhóm, tìm kiếm, cache theo id.
- `ResourceHelper`: parse `LectureResourceDto` thành online/offline links.
- `ImageCacheService` + `Class/Course/LectureImageCacheService`: cache ảnh về local.
- `OfflineResourceService`: quản lý file offline theo lecture.
- Shared:
- `LectureOfflineCacheService`: lưu đường dẫn offline của bài giảng trong SQLite (migrate từ resource-map.json).
- `CacheImagePathNormalizer`: chuẩn hoá field ảnh trước khi cache.
- `OfflineState`: detect offline (ping host API, cache 5s).
- `FileLog`: log vào `CacheFolder\app.log`.
- `GetMaxCodeService`: lấy mã lớn nhất theo loại.

## Models
Các DTO phản ánh dữ liệu API: `ClassDto`, `CourseDto`, `LectureDto`, `LessonDto`, `UserDto`, `GroupDto`, `LectureResourceDto`, `LectureFiles`, `LectureOfflineCache`, `PagedResult`, `Wrapper`, `ApiResponse`, `Upload*`, v.v.

## Điểm cần lưu ý
- Có 2 lớp `AuthSession` ở 2 namespace:
- `kido_teacher_app.Config.AuthSession` (chỉ `AccessToken`).
- `kido_teacher_app.Services.AuthSession` (có `AccessToken`, `UserId`, `Role`).
- Nhiều nơi dùng `Services.AuthSession`, nhưng `ImageCacheService` đang gọi `Config.AuthSession`.
- Dễ gây nhầm lẫn hoặc lỗi nếu token không sync.
- Offline/cache dùng SQLite (`DbCacheService`, `LectureOfflineCacheService`), có migrate từ `resource-map.json`.
- `OfflineResourceService` lưu file offline ở thư mục khác `AppConfig.LectureExtractFolder`.

## File/Folder quan trọng
- `kido_teacher_app\Program.cs`: entrypoint + auto-login.
- `kido_teacher_app\Forms\Auth\Login_Form.cs`: login flow.
- `kido_teacher_app\Forms\Main\Main_Form.cs`: điều hướng UI.
- `kido_teacher_app\Services\*.cs`: API + business logic.
- `kido_teacher_app\Config\*.cs`: cấu hình chung.
- `kido_teacher_app\Shared\Caching\*.cs`: cache ảnh + offline.
- `kido_teacher_app\Shared\Network\*.cs`: trạng thái offline.
- `kido_teacher_app\Shared\Logging\*.cs`: logging.

## Gợi ý hướng đọc tiếp
- Nếu sửa auth: xem `AuthService`, `Login_Form`, `Program`.
- Nếu sửa tải giáo án/offline: xem `LectureService`, `LectureDownloadService`, `OfflineResourceService`.
- Nếu sửa UI menu/luồng chính: xem `Main_Form` và các `UC_*` trong `Forms\Main\Page`.
