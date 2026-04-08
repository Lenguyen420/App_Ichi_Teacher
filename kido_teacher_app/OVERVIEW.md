# Kido Teacher App - Overview

## Mục tiêu
Ứng dụng WinForms cho giáo viên: đăng nhập, xem thông tin tài khoản, duyệt giáo án theo lớp và khóa học, tải tài nguyên để dùng offline.

## Luồng khởi động
1. `Program.Main` khởi tạo global exception handler.
2. Tạo các thư mục local cache/download trong `LocalApplicationData\KidoTeacherApp`.
3. Migrate cache ảnh cũ từ roaming sang local và xóa file ảnh 0 byte.
4. Gọi `AuthService.TryLoginWithSavedTokenAsync()`.
5. Nếu auto-login thành công:
   chạy `OfflinePrefetchService.PrefetchTeacherOfflineAsync(prefetchImages: true)` ở background rồi mở `Main_Form`.
6. Nếu chưa có token hợp lệ:
   mở `Form_Login`, login xong mới vào `Main_Form`, đồng thời cũng prefetch nền.

## Cấu hình và thư mục
- `App.config`: `ApiBaseUrl`.
- `AppConfig` cung cấp:
  - `AppDataRoot`, `AppDataRoaming`
  - `DownloadFolder`
  - `LectureExtractFolder`
  - `CacheFolder`
  - `ClassImageCacheFolder`
  - `CourseImageCacheFolder`
  - `LectureImageCacheFolder`
  - `DbFolder`, `DbPath`
- SQLite cache:
  - `DbCacheService` lưu JSON API vào bảng cache
  - `LectureOfflineCacheService` lưu mapping file offline theo `lectureId`
- Token nhớ đăng nhập nằm ở vùng roaming do `AuthService` quản lý.

## UI chính
- `Main_Form`
  - menu trái điều hướng giữa `UC_GioiThieu`, `UC_TaiKhoan`, `UC_GiaoAn`
  - giữ lại một số màn persistent (`gioiThieuControl`, `taiKhoanControl`, `giaoAnControl`)
  - thay màn qua `ReplaceMainControl(...)`
  - có chống flicker bằng `DoubleBuffered` và `WM_SETREDRAW` trên `panelMain`
- `Form_Login`
  - login giáo viên
  - lưu token nếu login thành công
- `UC_TaiKhoan`
  - hiện chỉ còn block thông tin tài khoản
  - phần “Thông Tin Bản Quyền” đã bị gỡ khỏi UI và khỏi designer

## Luồng giáo án

### 1. Màn lớp
- `UC_GiaoAn`
  - load danh sách lớp từ `ClassService.GetAllAsync()`
  - mỗi class là một card
  - ảnh lớp tải qua `ClassImageCacheService`
  - click class sẽ vào `UC_GiaoAnTheoThang`

### 2. Màn khóa học theo lớp
- `UC_GiaoAnTheoThang`
  - load danh sách course theo `classId` bằng `CourseService.GetByClassIdAsync(_classId)`
  - hiển thị ảnh course, thực tế đang dùng card/icon kiểu “tháng”
  - text header dòng xanh dùng `course.name`, không còn dựa vào `course.code`
  - header đã được chuẩn hóa cùng kích cỡ với màn chi tiết:
    - thanh xám cao `50`, font `Segoe UI 16 Bold`
    - thanh xanh cao `40`, font `Segoe UI 14 Bold`
    - nút back `30x30`, neo phải

### 3. Màn chi tiết giáo án theo khóa học
- `UC_GiaoAn_TheoThangChiTiet`
  - nhận `classId`, `courseId`, `className`, `courseName`
  - header chỉ đổi text `Giáo Án / {className} / {courseName}`
  - load danh sách bằng chiến lược `cache-first`
    - đọc cache key `lectures_class_{classId}_course_{courseId}` từ `DbCacheService`
    - nếu có cache thì render ngay
    - nếu online thì gọi tiếp `LectureService.GetByClassCourseAsync(...)`
    - nếu dữ liệu API giống hệt cache vừa render thì không render lại lần 2
  - không còn gọi `LectureService.GetByIdAsync()` theo từng lecture trong màn này
  - list API dùng endpoint:
    `/lecture?page=1&size=1000&courseId=...&classId=...&isGetResource=true`
  - có bật double buffering cho control và `flowList` để giảm flicker

### 4. Card bài giảng
- mỗi lecture card hiển thị:
  - ảnh/avatar
  - title
  - mã số
  - nút mở/tải PDF, video, e-learning, PowerPoint offline
- title dài đã được chỉnh để xuống dòng thay vì bị nuốt chữ
- nếu cache offline cũ khác `OFFLINE` zip URL từ server thì hiện label `Có cập nhật`
- click label cập nhật dùng cùng luồng update với nút chuẩn, không còn truyền control `null`
- render theo từng lecture có `try/catch`, một bài lỗi không làm trống toàn bộ danh sách

### 5. Offline
- file zip tải và giải nén qua `LectureService.DownloadAndExtractZipAsync(...)`
- file offline được map bằng `LectureResourceService.MapLectureFiles(...)`
- cache đường dẫn offline lưu qua `LectureOfflineCacheService.Save(...)`
- các viewer hiện có:
  - `Form_PdfViewer`
  - `Form_ElearningViewer`
- PDF hiện đang mở theo luồng offline/local

## Services chính
- `AuthService`
  - login
  - lưu token
  - auto-login bằng token đã lưu
- `ClassService`
  - lấy danh sách lớp
- `CourseService`
  - lấy danh sách khóa học theo lớp
- `LectureService`
  - lấy danh sách bài giảng
  - lấy theo lớp + khóa học với `isGetResource=true`
  - tải và giải nén zip bài giảng
- `LectureResourceService`
  - map file PDF/video/e-learning/PowerPoint sau khi giải nén
- `DbCacheService`
  - cache JSON API vào SQLite
- `OfflinePrefetchService`
  - prefetch class, course, lectures và ảnh sau khi login
- `UserService`
  - lấy thông tin user để đổ vào `UC_TaiKhoan`

## Shared / Helper đáng chú ý
- `LectureOfflineCacheService`
  - quản lý cache offline theo `lectureId`
  - có migrate từ `resource-map.json`
- `CacheImagePathNormalizer`
  - chuẩn hóa path ảnh trước khi lưu cache
- `ClassImageCacheService`, `CourseImageCacheService`, `LectureImageCacheService`
  - tải và cache ảnh local
- `OfflineState`
  - phát hiện online/offline, có cache ngắn hạn
- `FileLog`
  - ghi log file vào cache folder
- `LocalGiaoAnHelper`, `ZipHelper`
  - helper cho file local/zip

## Models
Các DTO chính:
- `ClassDto`
- `CourseDto`
- `LectureDto`
- `LessonDto`
- `LectureResourceDto`
- `LectureFiles`
- `LectureOfflineCache`
- `UserDto`
- `GroupDto`
- `PagedResult`
- `Wrapper`
- `ApiResponse`

## Điểm cần lưu ý
- Có 2 `AuthSession`:
  - `kido_teacher_app.Config.AuthSession`
  - `kido_teacher_app.Services.AuthSession`
  vẫn là điểm dễ gây nhầm nếu sau này token/user state bị lệch.
- Header ở các màn giáo án đã được chỉnh về cùng chuẩn UI, nhưng hiện vẫn được định nghĩa riêng ở từng user control, chưa tách thành shared control.
- `Main_Form` có cơ chế persistent control cho một số màn, nên khi sửa điều hướng cần lưu ý state `currentControl` và `persistentControls`.
- `UC_GiaoAn_TheoThangChiTiet` đang ưu tiên tốc độ mở màn bằng cache-first; nếu sửa tiếp, cần tránh chặn UI thread khi tạo quá nhiều lecture card một lúc.

## File nên đọc khi sửa từng mảng
- Auth / startup:
  - `Program.cs`
  - `Services/AuthService.cs`
  - `Forms/Auth/Login_Form.cs`
- Điều hướng chính:
  - `Forms/Main/Main_Form.cs`
- Giáo án:
  - `Forms/Main/Page/GiaoAn/UC_GiaoAn.cs`
  - `Forms/Main/Page/GiaoAn/UC_GiaoAnTheoThang.cs`
  - `Forms/Main/Page/GiaoAn/UC_GiaoAn_TheoThangChiTiet.cs`
  - `Services/LectureService.cs`
  - `Shared/Caching/LectureOfflineCacheService.cs`
- Tài khoản:
  - `Forms/Main/Page/UC_TaiKhoan.cs`
  - `Services/UserService.cs`
