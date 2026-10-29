using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data;
using QLKTX.Data.Entities;

namespace QLKTX.Services;

public sealed class DemoDataSeeder(QlKtxContext db, IPasswordHasher<user> hasher)
{
    public async Task SeedAsync()
    {
        if (!await db.roles.AnyAsync())
        {
            foreach (var (code, name) in new[]
            {
                ("system_admin", "Quản trị hệ thống"), ("dorm_manager", "Quản lý ký túc xá"),
                ("student", "Sinh viên"), ("maintenance_staff", "Nhân viên sửa chữa"), ("accountant", "Kế toán")
            }) db.roles.Add(new role { code = code, name = name });
            await db.SaveChangesAsync();
        }

        var admin = await EnsureUser("admin", "admin@qlktx.local", "Quản trị viên Demo", "Demo@123", "system_admin");
        var studentUser = await EnsureUser("sv001", "sv001@qlktx.local", "Nguyễn Minh Anh", "Demo@123", "student");
        if (!await db.buildings.AnyAsync())
        {
            var building = new building { code = "A", name = "Nhà A", address = "Khu nội trú · Cơ sở chính", description = "Dữ liệu minh họa", is_active = true, created_at = DateTime.Now };
            var buildingB = new building { code = "B", name = "Nhà B", address = "Khu nội trú · Cơ sở chính", description = "Dữ liệu minh họa", is_active = true, created_at = DateTime.Now };
            db.buildings.AddRange(building, buildingB);
            await db.SaveChangesAsync();
            var room1 = new room { building_id = building.id, room_number = "A-203", floor_number = 2, room_type = "4 người", capacity = 4, monthly_price = 650000, status = "available", description = "Phòng 4 người", created_at = DateTime.Now, updated_at = DateTime.Now };
            var room2 = new room { building_id = building.id, room_number = "A-204", floor_number = 2, room_type = "6 người", capacity = 6, monthly_price = 520000, status = "available", description = "Phòng 6 người", created_at = DateTime.Now, updated_at = DateTime.Now };
            var room3 = new room { building_id = buildingB.id, room_number = "B-105", floor_number = 1, room_type = "4 người", capacity = 4, monthly_price = 700000, status = "available", description = "Phòng 4 người", created_at = DateTime.Now, updated_at = DateTime.Now };
            db.rooms.AddRange(room1, room2, room3);
            await db.SaveChangesAsync();
            foreach (var room in new[] { room1, room2, room3 })
                for (var i = 1; i <= room.capacity; i++) db.beds.Add(new bed { room_id = room.id, bed_number = $"G{i:00}", status = "available" });
            db.services.AddRange(new service { code = "WIFI", name = "Internet", unit = "tháng", unit_price = 50000, is_metered = false, is_active = true, description = "Gói internet khu nội trú" }, new service { code = "PARK", name = "Gửi xe", unit = "tháng", unit_price = 80000, is_metered = false, is_active = true, description = "Dịch vụ gửi xe tháng" }, new service { code = "ELEC", name = "Điện", unit = "kWh", unit_price = 3500, is_metered = true, is_active = true, description = "Điện theo chỉ số sử dụng" });
            db.dormitory_rules.Add(new dormitory_rule { title = "Nội quy khu nội trú", version = "1.0", content = "Giữ gìn vệ sinh chung; bảo quản tài sản; tuân thủ giờ yên tĩnh; đăng ký khách theo quy định; báo ngay sự cố an toàn cho ban quản lý.", effective_from = DateOnly.FromDateTime(DateTime.Today), is_active = true, created_by_user_id = admin.id, created_at = DateTime.Now });
            await db.SaveChangesAsync();

            var student = new student { user_id = studentUser.id, student_code = "20260001", gender = "female", faculty = "Công nghệ thông tin", course_year = 2, emergency_contact_name = "Người liên hệ Demo", emergency_contact_phone = "0900000000", created_at = DateTime.Now };
            db.students.Add(student);
            await db.SaveChangesAsync();
            db.room_registrations.Add(new room_registration { registration_code = "DK-2026-0001", student_id = student.id, preferred_building_id = building.id, preferred_room_type = "4 người", requested_start_date = DateOnly.FromDateTime(DateTime.Today.AddDays(14)), requested_end_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(10)), submitted_at = DateTime.Now, status = "pending", note = "Dữ liệu demo — có thể duyệt trên màn hình Đăng ký phòng." });
            await db.SaveChangesAsync();
        }
    }

    private async Task<user> EnsureUser(string username, string email, string name, string password, string roleCode)
    {
        var found = await db.users.FirstOrDefaultAsync(x => x.username == username);
        if (found is not null) return found;
        var item = new user { username = username, email = email, full_name = name, is_active = true, created_at = DateTime.Now, updated_at = DateTime.Now };
        item.password_hash = hasher.HashPassword(item, password);
        db.users.Add(item);
        await db.SaveChangesAsync();
        var role = await db.roles.SingleAsync(x => x.code == roleCode);
        db.user_roles.Add(new user_role { user_id = item.id, role_id = role.id, assigned_at = DateTime.Now });
        await db.SaveChangesAsync();
        return item;
    }
}
