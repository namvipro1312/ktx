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
        await EnsureUser("qlktx01", "qlktx01@qlktx.local", "Nguyen Van Quan Ly", "Demo@123", "dorm_manager");
        await EnsureUser("ketoan01", "ketoan01@qlktx.local", "Tran Thi Ke Toan", "Demo@123", "accountant");
        await EnsureUser("ktv01", "ktv01@qlktx.local", "Le Van Ky Thuat", "Demo@123", "maintenance_staff");
        var studentUser = await EnsureUser("sv001", "sv001@qlktx.local", "Nguyễn Minh Anh", "Demo@123", "student");
        await EnsureUser("sv002", "sv002@qlktx.local", "Tran Van Binh", "Demo@123", "student");
        if (!await db.asset_types.AnyAsync())
        {
            db.asset_types.AddRange(new asset_type { code = "BED", name = "Giường và nệm", description = "Tài sản phục vụ lưu trú" }, new asset_type { code = "FURN", name = "Nội thất", description = "Bàn ghế và tủ" }, new asset_type { code = "ELEC", name = "Thiết bị điện", description = "Thiết bị dùng chung" });
            await db.SaveChangesAsync();
        }
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

        var demoRoom = await db.rooms.OrderBy(x => x.id).FirstOrDefaultAsync();
        var furniture = await db.asset_types.FirstOrDefaultAsync(x => x.code == "FURN");
        if (demoRoom is not null && furniture is not null && !await db.room_assets.AnyAsync())
        {
            db.room_assets.AddRange(new room_asset { room_id = demoRoom.id, asset_type_id = furniture.id, asset_tag = "DEMO-TBL-01", name = "Bàn học", quantity = 2, condition_status = "good", acquired_at = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), last_checked_at = DateTime.Now, note = "Dữ liệu minh họa" }, new room_asset { room_id = demoRoom.id, asset_type_id = furniture.id, asset_tag = "DEMO-CHAIR-01", name = "Ghế", quantity = 4, condition_status = "good", acquired_at = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), last_checked_at = DateTime.Now, note = "Dữ liệu minh họa" });
            await db.SaveChangesAsync();
        }
        var electricity = await db.services.FirstOrDefaultAsync(x => x.is_metered && x.code == "ELEC");
        if (demoRoom is not null && electricity is not null && !await db.utility_meters.AnyAsync(x => x.room_id == demoRoom.id && x.service_id == electricity.id))
        {
            db.utility_meters.Add(new utility_meter { room_id = demoRoom.id, service_id = electricity.id, meter_code = "DEMO-" + demoRoom.room_number + "-E", installed_at = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)), is_active = true, note = "Đồng hồ minh họa; thay mã khi vận hành thật." });
            await db.SaveChangesAsync();
        }

        await EnsureComprehensiveRoomsAsync();
    }

    private async Task EnsureComprehensiveRoomsAsync()
    {
        var bldgA = await db.buildings.FirstOrDefaultAsync(b => b.code == "A");
        var bldgB = await db.buildings.FirstOrDefaultAsync(b => b.code == "B");
        if (bldgA is null || bldgB is null) return;

        var roomDefinitions = new (string bldgCode, string roomNum, int floor, string type, int cap, decimal price, string status, int occupiedCount)[]
        {
            // TÒA A - KHU NAM (Tầng 1 -> Tầng 4)
            ("A", "A-101", 1, "4 người", 4, 650000, "available", 0),
            ("A", "A-102", 1, "4 người", 4, 650000, "available", 2),
            ("A", "A-103", 1, "6 người", 6, 520000, "available", 6), // Kín chỗ
            ("A", "A-104", 1, "6 người", 6, 520000, "maintenance", 0), // Bảo trì
            ("A", "A-201", 2, "4 người", 4, 650000, "available", 1),
            ("A", "A-202", 2, "4 người", 4, 650000, "available", 0),
            ("A", "A-203", 2, "4 người", 4, 650000, "available", 1),
            ("A", "A-204", 2, "6 người", 6, 520000, "available", 0),
            ("A", "A-301", 3, "4 người", 4, 650000, "available", 0),
            ("A", "A-302", 3, "6 người", 6, 520000, "available", 3),
            ("A", "A-303", 3, "6 người", 6, 520000, "available", 6), // Kín chỗ
            ("A", "A-304", 3, "8 người", 8, 420000, "available", 0),
            ("A", "A-401", 4, "4 người", 4, 650000, "available", 0),
            ("A", "A-402", 4, "6 người", 6, 520000, "available", 2),
            ("A", "A-403", 4, "8 người", 8, 420000, "available", 4),
            ("A", "A-404", 4, "8 người", 8, 420000, "available", 8), // Kín chỗ

            // TÒA B - KHU NỮ (Tầng 1 -> Tầng 4)
            ("B", "B-101", 1, "4 người", 4, 700000, "available", 0),
            ("B", "B-102", 1, "4 người", 4, 700000, "available", 1),
            ("B", "B-103", 1, "4 người", 4, 700000, "available", 4), // Kín chỗ
            ("B", "B-104", 1, "6 người", 6, 550000, "available", 0),
            ("B", "B-105", 1, "4 người", 4, 700000, "available", 0),
            ("B", "B-201", 2, "4 người", 4, 700000, "available", 0),
            ("B", "B-202", 2, "4 người", 4, 700000, "available", 2),
            ("B", "B-203", 2, "6 người", 6, 550000, "available", 0),
            ("B", "B-204", 2, "6 người", 6, 550000, "maintenance", 0), // Bảo trì
            ("B", "B-301", 3, "4 người", 4, 700000, "available", 0),
            ("B", "B-302", 3, "6 người", 6, 550000, "available", 2),
            ("B", "B-303", 3, "8 người", 8, 450000, "available", 8), // Kín chỗ
            ("B", "B-304", 3, "8 người", 8, 450000, "available", 0),
            ("B", "B-401", 4, "6 người", 6, 550000, "available", 0),
            ("B", "B-402", 4, "6 người", 6, 550000, "available", 3),
            ("B", "B-403", 4, "8 người", 8, 450000, "available", 0),
            ("B", "B-404", 4, "8 người", 8, 450000, "available", 4)
        };

        foreach (var def in roomDefinitions)
        {
            var bldg = def.bldgCode == "A" ? bldgA : bldgB;
            var r = await db.rooms.Include(x => x.beds).FirstOrDefaultAsync(x => x.building_id == bldg.id && x.room_number == def.roomNum);
            if (r is null)
            {
                r = new room
                {
                    building_id = bldg.id,
                    room_number = def.roomNum,
                    floor_number = (short)def.floor,
                    room_type = def.type,
                    capacity = (short)def.cap,
                    monthly_price = def.price,
                    status = def.status,
                    description = $"Phòng {def.roomNum} - {def.type}",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                db.rooms.Add(r);
                await db.SaveChangesAsync();
            }

            for (var i = 1; i <= r.capacity; i++)
            {
                var bedNum = $"G{i:02}";
                var bed = r.beds.FirstOrDefault(b => b.bed_number == bedNum);
                var isOccupied = i <= def.occupiedCount;
                if (bed is null)
                {
                    db.beds.Add(new bed
                    {
                        room_id = r.id,
                        bed_number = bedNum,
                        status = isOccupied ? "occupied" : "available"
                    });
                }
                else if (isOccupied && bed.status == "available")
                {
                    bed.status = "occupied";
                }
            }
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
        if (roleCode == "student")
        {
            var exists = await db.students.AnyAsync(s => s.user_id == item.id);
            if (!exists)
            {
                db.students.Add(new student
                {
                    user_id = item.id,
                    student_code = "SV" + item.id.ToString("D5"),
                    gender = "male",
                    faculty = "Công nghệ thông tin",
                    course_year = 1,
                    emergency_contact_name = "Phụ huynh " + item.full_name,
                    emergency_contact_phone = "0912345678",
                    created_at = DateTime.Now
                });
            }
        }
        await db.SaveChangesAsync();
        return item;
    }
}
