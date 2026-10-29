# QL Ký túc xá

Ứng dụng web ASP.NET Core MVC quản lý vận hành ký túc xá cho sinh viên và nhân viên: tài khoản, chỗ ở, lưu trú, dịch vụ, vận hành phòng, tài chính và báo cáo.

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
- Khách thăm: đăng ký, duyệt/từ chối, ghi nhận giờ vào và ra; sinh viên được hủy yêu cầu chưa duyệt.
- Tài sản: kiểm kê theo phòng, nhóm tài sản, số lượng và tình trạng; đồng hồ điện nước gắn phòng/dịch vụ và ghi nhận chỉ số theo ngày.
- Điện nước: tính tiêu thụ theo chênh lệch chỉ số, tính tiền theo giá dịch vụ và phân bổ cho hợp đồng đang ở; khoản sử dụng được đưa vào kỳ lập hóa đơn.
- Vi phạm: lập biên bản, theo dõi mức phạt và xử lý, tạo khoản phạt trên hóa đơn; thông báo riêng cho sinh viên.
- Lập hóa đơn; sinh viên tạo yêu cầu thanh toán chờ kế toán xác nhận giao dịch thực nhận. Có yêu cầu hoàn tiền theo từng khoản thu, đối soát số tiền còn lại và ghi nhận sau khi hoàn thực tế.
- Hộp thư thông báo riêng, gửi thông báo trực tiếp, tìm kiếm/lọc trong danh sách và báo cáo từ SQL views.
- Tài khoản có vai trò nhân viên sửa chữa và kế toán với phạm vi màn hình/thao tác riêng.

Ứng dụng chưa tích hợp cổng thanh toán, email/SMS, ký số, lưu trữ tài liệu, sao lưu tự động hoặc kết nối thiết bị đo tự động. Chỉ số điện nước được nhập thủ công. Nút xác nhận thanh toán và hoàn tiền chỉ ghi nhận nghiệp vụ nội bộ; phải đối chiếu giao dịch thực nhận/hoàn tiền thực tế trước khi xác nhận. Hoàn/khấu trừ tiền đặt cọc khi trả phòng cần được đối soát riêng.

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

