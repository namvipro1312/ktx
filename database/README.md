# CSDL quản lý ký túc xá – SQL Server

CSDL `qlktx` đã được tạo trên SQL Server cục bộ `Admin-PC` (SQL Server 2022 Developer Edition). Script T-SQL chính nằm ở `schema.sql`.

## Kết nối và chạy lại

Máy này dùng Windows Authentication; có thể kiểm tra bằng:

```powershell
sqlcmd -S localhost -E -d qlktx -Q "SELECT DB_NAME()"
```

`schema.sql` là script cài đặt cho CSDL mới. Không chạy lại nguyên trạng trên CSDL đã có bảng; thay đổi tiếp theo nên được tạo thành migration riêng để giữ dữ liệu.

`schema_mysql.sql` là bản MySQL cũ, không dùng cho SQL Server.

## Phạm vi

| Nhóm | Nội dung | Bảng chính |
| --- | --- | --- |
| sp1 | Đăng nhập, phân quyền, phiên đăng nhập, khôi phục mật khẩu, nhật ký | `users`, `roles`, `permissions`, `user_roles`, `role_permissions`, `user_sessions`, `password_reset_tokens`, `login_attempts`, `audit_logs` |
| sp1 | Sinh viên, tòa, phòng, giường, tài sản | `students`, `buildings`, `rooms`, `beds`, `asset_types`, `room_assets` |
| sp2 | Đăng ký phòng, lịch sử duyệt, đề xuất giường | `room_registrations`, `registration_reviews`, `registration_bed_offers` |
| sp3 | Hợp đồng, phụ lục/gia hạn, cọc, nhận/trả phòng | `contracts`, `contract_amendments`, `deposit_transactions`, `occupancy_records` |
| sp4 | Dịch vụ, điện nước, sửa chữa, nội quy, vi phạm, khách thăm | `services`, `service_subscriptions`, `service_usages`, `utility_meters`, `utility_readings`, `repair_requests`, `dormitory_rules`, `student_violations`, `guest_visits` |
| sp5 | Hóa đơn, thanh toán, hoàn tiền và thống kê | `invoices`, `invoice_items`, `payments`, `payment_refunds` |

Các view báo cáo: `v_invoice_balances`, `v_room_occupancy`, `v_building_occupancy`, `v_current_residents`, `v_monthly_invoice_summary`, `v_registration_summary`, `v_repair_summary`, `v_student_outstanding_balances`.

Script đã khởi tạo 5 vai trò, 23 quyền và ma trận quyền khởi đầu. Rà soát chính sách quyền của trường trước khi triển khai cho người dùng thật.

## Quy tắc cần xử lý trong ứng dụng

- Hash mật khẩu bằng Argon2id/bcrypt; chỉ lưu hash token phiên và token đặt lại mật khẩu. Giới hạn lần thử đăng nhập và thu hồi phiên khi cần.
- Tạo đề nghị giường/hợp đồng trong transaction, khóa giường và kiểm tra ngày hợp đồng chồng lấn. SQL Server không có exclusion constraint cho khoảng ngày.
- Ghi nhận nhận/trả phòng, cọc, thanh toán và hoàn tiền theo sổ giao dịch, không xóa dữ liệu tài chính đã phát sinh.
- Khi ghi điện nước, kiểm tra chỉ số mới không thấp hơn chỉ số trước, tính tiêu thụ/chi phí, rồi tạo khoản sử dụng và dòng hóa đơn nhất quán.
- Cập nhật trạng thái hóa đơn theo khoản mục và thanh toán hoàn tất; kiểm tra tổng hoàn tiền không vượt tiền đã thu và tổng hoàn/khấu trừ cọc không vượt số dư cọc.
- `documents` lưu khóa tệp trong kho lưu trữ riêng; ứng dụng phải kiểm tra đối tượng được tham chiếu và quyền truy cập.

CSDL đã sẵn sàng cho lớp dữ liệu của các luồng phổ biến. Giao diện, API, tích hợp cổng thanh toán/email/SMS, sao lưu và các quy trình vận hành vẫn cần được triển khai trong ứng dụng/hạ tầng.
