# Cache Diagnostic Tool - User Guide

## 📋 Lý do cần dùng

Nếu bạn gặp lỗi:
- ❌ File không mở được
- ❌ "File không tồn tại"
- ❌ Ký tự lạ trong tên file (Báº®T, Ã"C, vv.)
- ❌ Cache bị lỗi encoding

=> Database cache có thể bị corrupted.

## 🔧 Cách sửa

### **Cách 1: Dùng PowerShell Script (Dễ nhất)**

1. **Mở PowerShell**: 
   - Nhấn `Windows + R`
   - Gõ `powershell`
   - Nhấn Enter

2. **Chạy script**:
   ```powershell
   cd "C:\Users\[YourUsername]\code\work\App_Ichi_Teacher"
   .\cache_diagnostic.ps1
   ```

3. **Script sẽ**:
   - Quét database
   - Show corrupted entries
   - Hỏi có muốn fix không
   - Backup + Reset database

### **Cách 2: Dùng App Menu**

1. **Mở Kido Teacher App**
2. **Vào Menu** → **Help** → **Cache Diagnostics** (nếu có)
3. **Click "Scan Database"**
4. **Nếu có lỗi, click "Backup & Reset"**
5. **Restart app**

### **Cách 3: Manual Reset**

1. **Tắt Kido Teacher App**
2. **Mở File Explorer**
3. **Đi đến**: `C:\Users\YourUsername\AppData\Local\KidoTeacherApp\`
4. **Tìm file**: `app_cache.db`
5. **Xóa file** → Xong!
6. **Mở app lại** → Database sẽ tự tạo lại

## ✅ Cách kiểm tra

- Chạy `cache_diagnostic.ps1`
- Nếu output show `✓ Database is healthy!` → OK
- Nếu show `✗ Corrupted entries` → Cần fix

## 📝 Output Example

```
========== CACHE DIAGNOSTIC ==========
Time: 2026-05-07 03:53:37

Database path: C:\Users\Admin\AppData\Local\KidoTeacherApp\app_cache.db
Database size: 5 MB
Lectures path: C:\Users\Admin\AppData\Local\KidoTeacherApp\Lectures

Scanning database...

✓ ce5ac06a-9ef3-4162-83e3-34db8732a72d
✗ 12345678-1234-1234-1234-123456789012
  - PDF has encoding issue
  - PDF file missing

Summary:
  Valid entries: 1
  Corrupted entries: 1

⚠ Found corrupted cache entries!
Do you want to backup and reset the database? (Y/N): Y

✓ Database backed up to: ...app_cache.db.backup.20260507_035337
✓ Corrupted database deleted

Done! Please restart the Kido Teacher app.

========== END DIAGNOSTIC ==========
```

## 🆘 Troubleshooting

### PowerShell không chạy script
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
Rồi chạy lại script.

### SQLite not found error
- Cài đặt SQLite: https://www.sqlite.org/download.html
- Hoặc dùng Cách 3 (Manual Reset)

### Vẫn lỗi sau khi reset
- Database bị full: Xóa folder `Lectures` cũ
- Path quá dài: Đổi tên file
- Không có quyền ghi: Chạy PowerShell as Administrator

## 📞 Need Help?

1. **Chạy diagnostic lấy report**
2. **Lưu output ra file**: 
   ```powershell
   .\cache_diagnostic.ps1 | Out-File diagnostic_report.txt
   ```
3. **Gửi file report cho dev**

---

**Remember**: Chạy diagnostic thường xuyên (khoảng 1 tháng/lần) để phát hiện sớm vấn đề! ✓
