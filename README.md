# QL Ký túc xá

Ứng dụng web ASP.NET Core MVC quản lý vòng đời lưu trú: tài khoản và vai trò, tòa/phòng/giường, đăng ký và duyệt phòng, hợp đồng/nhận trả phòng, dịch vụ, sửa chữa, nội quy, hóa đơn/thanh toán và báo cáo.

## Công nghệ

- .NET 9 MVC
- Entity Framework Core 9, reverse-engineer từ SQL Server
- SQL Server 2022 trở lên
- Giao diện responsive, vai trò sinh viên và nhân viên quản trị

## Chạy trên máy phát triển

1. Cài .NET 9 SDK và SQL Server. Tạo CSDL bằng script mới trên một database trống:

   ```powershell
   sqlcmd -S localhost -E -i database/schema.sql
   ```

2. Cấu hình kết nối bằng User Secrets (không đặt mật khẩu trong Git):

   ```powershell
   dotnet user-secrets set "ConnectionStrings:Dormitory" "Server=localhost;Database=qlktx;Integrated Security=True;TrustServerCertificate=True;"
   ```

   Máy dùng SQL Authentication có thể đặt chuỗi kết nối riêng bằng User Secrets hoặc biến môi trường `ConnectionStrings__Dormitory`.

3. Chạy ứng dụng:

   ```powershell
   dotnet restore
   dotnet run
   ```

   Hồ sơ chạy `http` mở cổng `5080`; hồ sơ `https` dùng cổng `7222` và `5080`.

## Tài khoản demo

Trong `Development`, ứng dụng tự tạo dữ liệu minh họa khi các bảng dữ liệu chính còn trống. Các tài khoản chỉ dùng local:

| Vai trò | Tên đăng nhập | Mật khẩu |
| --- | --- | --- |
| Quản trị hệ thống | `admin` | `Demo@123` |
| Sinh viên | `sv001` | `Demo@123` |

Mật khẩu được lưu dưới dạng hash. Thay thông tin demo, dữ liệu sinh viên, quy tắc nội trú và bảng giá trước khi dùng thật. Không chạy seed dữ liệu demo trong Production.

## Luồng hiện có

- Đăng nhập cookie, phân quyền theo vai trò, ghi nhận lần đăng nhập và giới hạn thử sai trong cửa sổ 15 phút.
- Quản lý tòa/phòng/giường và trạng thái bảo trì.
- Sinh viên gửi đăng ký; ban quản lý duyệt hoặc từ chối. Khi duyệt, hệ thống giữ giường và tạo hợp đồng nháp; ghi nhận ký mới kích hoạt hợp đồng.
- Theo dõi nhận phòng/trả phòng, thu và đối chiếu tiền đặt cọc trước khi kích hoạt hợp đồng, dịch vụ theo hợp đồng, yêu cầu sửa chữa, nội quy và xác nhận đã đọc.
- Lập hóa đơn; sinh viên tạo yêu cầu thanh toán chờ kế toán xác nhận giao dịch thực nhận; xem báo cáo từ các SQL view.
- Tài khoản có vai trò nhân viên sửa chữa và kế toán với phạm vi màn hình/thao tác riêng.

Ứng dụng chưa tích hợp cổng thanh toán, email/SMS, ký số, kho tài liệu, sao lưu, quản lý chỉ số điện nước hoặc quy trình hoàn/khấu trừ cọc khi trả phòng. Nút xác nhận thanh toán chỉ ghi nhận nghiệp vụ nội bộ; phải đối chiếu tiền thực nhận trước khi xác nhận.

## Cấu trúc

- `Controllers/`, `Views/`, `wwwroot/`: web MVC
- `Data/`: context và entity được scaffold từ SQL Server
- `database/schema.sql`: schema chuẩn SQL Server và các view báo cáo
- `database/schema_mysql.sql`: bản tham khảo MySQL cũ
- `PRODUCT.md`, `.impeccable/surfaces/dashboard-index.md`: product truth và brief thiết kế

## Kiểm tra

```powershell
dotnet build
```

