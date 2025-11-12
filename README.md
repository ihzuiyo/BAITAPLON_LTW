# Hệ thống Quản lý Sinh viên (Student Management System)

Dự án quản lý sinh viên trung tâm tin học, xây dựng bằng **ASP.NET Core 8.0 MVC**, EF Core, và SQL Server.

## Yêu cầu Hệ thống

- .NET 8.0 SDK
- SQL Server 2019+ (hoặc SQL Server Express)
- Visual Studio 2022 (Khuyến nghị)

---

## 🚀 Hướng dẫn Cài đặt Nhanh

### 1. Thiết lập Cơ sở dữ liệu (Database)

1.  Mở **SQL Server Management Studio (SSMS)** hoặc công cụ CSDL của bạn.
2.  Tạo một database mới:
    ```sql
    CREATE DATABASE QLSV_TrungTamTinHoc
    GO
    ```
3.  Mở và chạy toàn bộ tệp script chính tại: `database/QLSV_TrungTamTinHoc.sql`.
    *Lưu ý: Script này đã bao gồm cả **cấu trúc (schema)** và **dữ liệu mẫu (mock data)**.*
4.  Cập nhật chuỗi kết nối (Connection String) trong tệp `appsettings.json`:

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Data Source=.\\SQLEXPRESS;Initial Catalog=QLSV_TrungTamTinHoc;Integrated Security=True;Encrypt=True;Trust Server Certificate=True"
      }
    }
    ```
    *(**Lưu ý:** Thay `.\\SQLEXPRESS` bằng `localhost` hoặc tên Server của bạn nếu bạn dùng SQL Server bản đầy đủ).*

### 2. Chạy Ứng dụng

1.  Mở tệp `StudentManagement.sln` bằng Visual Studio 2022.
2.  Nhấn **F5** (hoặc nút Run màu xanh lá) để chạy dự án.
3.  Ứng dụng sẽ tự động mở trình duyệt tại: `https://localhost:5001`

---

## 👤 Tài khoản Đăng nhập (Mặc định)

Bạn có thể sử dụng các tài khoản sau để kiểm tra (dữ liệu từ script `QLSV_TrungTamTinHoc.sql`):

| Vai trò | Username | Password |
|---|---|---|
| **Admin** | `admin` | `admin@123` |
| **Teacher** | `teacher01` | `teacher@123` |
| **Student** | `student01` | `student@123` |

> ⚠️ **CẢNH BÁO BẢO MẬT:**
> Các mật khẩu này hiện đang được lưu dưới dạng **văn bản thuần (plain text)** trong CSDL (theo script `database_BTLW.sql`), không được băm.
>
> Đây là một lỗ hổng bảo mật nghiêm trọng và **chỉ dùng cho mục đích demo/phát triển**. Không bao giờ sử dụng trong môi trường thực tế.