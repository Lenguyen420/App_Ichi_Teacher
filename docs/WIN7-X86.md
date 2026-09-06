# Ichi Teacher: kiểm thử Windows 7 SP1 32-bit

## Tạo bộ cài thử

Chạy trên máy build có Visual Studio MSBuild và .NET Framework 4.8 targeting pack:

```powershell
powershell -NoProfile -File .\scripts\Prepare-Win7Bundle.ps1
```

Kết quả: `out/win7-x86/`, gồm bộ cài ClickOnce trong thư mục có thời gian build,
runtime WebView2 109 x86 trong `prerequisites/`, và `build-info.json`.
File mang sang máy đích: `out/IchiTeacher-Win7-x86-test.zip`.
Script kiểm tra SHA-256 và chữ ký Microsoft của runtime; không cài runtime trên máy build,
không tăng version production, không upload. Profile thử cài từ ổ đĩa và tắt tự cập nhật
để không tự tải lại bản đang ở server. Chỉ dùng trên máy kiểm thử sạch.

## Cài trên máy đích

1. Xác nhận Windows 7 **SP1**, 32-bit, đã có các cập nhật hệ thống cần thiết để cài .NET 4.8.
2. Cài .NET Framework **4.8** và khởi động lại nếu được yêu cầu. `setup.exe` có khai báo
   prerequisite này nhưng tải từ Microsoft nếu thiếu; bộ này **không chứa bộ cài .NET offline**.
3. Copy toàn bộ thư mục bộ cài sang máy đích. Trong `prerequisites`, chạy
   `Install-WebView2-Win7.cmd` bằng **Run as administrator**.
   Đây là gói Microsoft Update Catalog, cần tham số app GUID trong script;
   không chỉ bấm đúp file EXE. Xác nhận Runtime **109.0.1518.140** trong Programs and Features.
   Cách cài này vẫn cần xác minh trên Win7 sạch.
4. Chạy `setup.exe` trong thư mục `ClickOnce-...` mới nhất.

App build x86 chạy được dưới WOW64 trên Windows x64, nhưng DLL native được app nạp
phải là x86. Trên Windows mới dùng WebView2 Evergreen phù hợp với hệ điều hành;
không cài runtime 109 cũ lên máy Windows mới để sử dụng thường xuyên.

## Các bước nghiệm thu (chưa chạy trên Win7)

| Luồng | Cách kiểm tra | Kết quả cần có |
| --- | --- | --- |
| Cài mới | Chạy setup với tài khoản Windows thông thường sau khi cài prerequisite | App khởi động, không thiếu DLL |
| Đăng nhập | Dùng tài khoản kiểm thử đúng, rồi thử mật khẩu sai | Đúng thì vào trang chính; sai thì báo lỗi phù hợp |
| Tải bài | Tải một bài có ZIP e-learning, ảnh và tài liệu | Tải/giải nén hoàn tất, tài nguyên mở được |
| Cache SQLite | Đóng/mở app sau khi tải bài; ngắt mạng và mở lại nội dung đã lưu | Dữ liệu và bài tải trước vẫn còn; không lỗi native SQLite |
| E-learning | Mở bài online và bài Storyline local, thử âm thanh/video/tương tác | Hiển thị trong app; log xác nhận runtime 109 và processBits=32 |
| Mở lại | Đóng/mở e-learning nhiều lần và khởi động lại app | Không lỗi khóa browser profile hay mất tài nguyên |

Log WebView2: `%LOCALAPPDATA%\KidoTeacherApp\Cache\log_webview.txt`.
Browser profile: `%LOCALAPPDATA%\KidoTeacherApp\WebView2`.
Cache SQLite: `%LOCALAPPDATA%\KidoTeacherApp\Db\app_cache.db`.
Manifest ClickOnce đã được bổ sung DLL native `e_sqlite3.dll` x86;
script build kiểm tra file, kiến trúc và hash của cả SQLite và WebView2Loader.
Không xóa cache đang dùng để kiểm thử. Nếu báo thiếu `api-ms-win-crt-...`, kiểm tra Universal CRT
và bản Visual C++ Redistributable x86 hỗ trợ Win7; không tải DLL rời từ website bên ngoài.

## Giới hạn và nguồn

SDK được ghim ở `1.0.1518.46`; runtime 109 là nhánh cuối cho Win7 và đã hết hỗ trợ.
Build/publish trên Win11 không chứng minh app chạy đầy đủ trên Win7.
Kiểm tra ngày 2026-09-06 trên Windows 11 x64: payload ClickOnce chạy test SQLite
ghi/đóng/mở lại/đọc trong tiến trình x86 thành công; WebView2 SDK nạp được runtime
Evergreen 152.0.4191.66 đang cài. Đây không phải kiểm thử runtime 109 hay giao diện e-learning.
Build còn các cảnh báo C# hiện có; NuGet báo NU1903 cho SQLitePCLRaw.lib.e_sqlite3 2.1.2
(phiên bản native đã dùng trước thay đổi này, nay khai báo trực tiếp để đóng gói ClickOnce).
Chưa nâng cấp SQLite trong lần sửa tương thích này; cần đánh giá riêng trước khi phát hành production.
Đăng nhập và tải bài cần backend, tài khoản kiểm thử và kết nối TLS hoạt động trên máy đích.

- [Microsoft: giới hạn SDK và runtime Win7](https://blogs.windows.com/msedgedev/2022/12/09/microsoft-edge-and-webview2-ending-support-for-windows-7-and-windows-8-8-1/)
- [Microsoft Update Catalog: Runtime 109.0.1518.140](https://www.catalog.update.microsoft.com/Search.aspx?q=Microsoft%20Edge-WebView2%20Runtime%20109.0.1518.140)
- [Tham khảo tham số cài gói Catalog từ Microsoft Q&A, cần kiểm thử trên máy đích](https://learn.microsoft.com/en-us/answers/questions/1661814/latest-version-of-microsoft-edge-webview2-runtime)
- [Yêu cầu .NET Framework](https://learn.microsoft.com/en-us/dotnet/framework/get-started/system-requirements)
