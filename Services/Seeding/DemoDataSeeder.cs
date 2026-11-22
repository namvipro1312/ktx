using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data;
using QLKTX.Data.Entities;

namespace QLKTX.Services;

public sealed class DemoDataSeeder(QlKtxContext db, IPasswordHasher<user> hasher)
{
    public async Task SeedAsync()
    {
        // 1. Roles
        if (!await db.roles.AnyAsync())
        {
            foreach (var (code, name) in new[]
            {
                ("system_admin", "Quản trị hệ thống"),
                ("student", "Sinh viên")
            }) db.roles.Add(new role { code = code, name = name });
            await db.SaveChangesAsync();
        }

        // 2. Users & Demo Accounts
        var admin = await EnsureUser("admin", "admin@qlktx.local", "Quản trị viên KTX", "Demo@123", "system_admin");
        var studentUser1 = await EnsureUser("sv001", "sv001@qlktx.local", "Nguyễn Minh Anh", "Demo@123", "student");
        var studentUser2 = await EnsureUser("sv002", "sv002@qlktx.local", "Trần Văn Bình", "Demo@123", "student");
        var studentUser3 = await EnsureUser("sv003", "sv003@qlktx.local", "Lê Thị Thu Thảo", "Demo@123", "student");

        // 3. Asset Types
        if (!await db.asset_types.AnyAsync())
        {
            db.asset_types.AddRange(
                new asset_type { code = "BED", name = "Giường và nệm", description = "Tài sản phục vụ lưu trú" },
                new asset_type { code = "FURN", name = "Nội thất", description = "Bàn ghế và tủ" },
                new asset_type { code = "ELEC", name = "Thiết bị điện", description = "Thiết bị dùng chung" }
            );
            await db.SaveChangesAsync();
        }

        // 4. Buildings
        if (!await db.buildings.AnyAsync())
        {
            db.buildings.AddRange(
                new building { code = "A", name = "Nhà A (Khu Nam)", address = "Khu nội trú · Cơ sở chính", description = "Dành cho sinh viên nam", is_active = true, created_at = DateTime.Now },
                new building { code = "B", name = "Nhà B (Khu Nữ)", address = "Khu nội trú · Cơ sở chính", description = "Dành cho sinh viên nữ", is_active = true, created_at = DateTime.Now }
            );
            await db.SaveChangesAsync();
        }

        // 5. Services
        if (!await db.services.AnyAsync())
        {
            db.services.AddRange(
                new service { code = "WIFI", name = "Internet tốc độ cao", unit = "tháng", unit_price = 50000, is_metered = false, is_active = true, description = "Gói internet băng thông rộng KTX" },
                new service { code = "PARK", name = "Gửi xe máy", unit = "tháng", unit_price = 80000, is_metered = false, is_active = true, description = "Dịch vụ trông giữ xe gắn máy tháng" },
                new service { code = "LAUNDRY", name = "Giặt sấy tự động", unit = "tháng", unit_price = 100000, is_metered = false, is_active = true, description = "Dịch vụ máy giặt thông minh KTX" },
                new service { code = "ELEC", name = "Điện năng tiêu thụ", unit = "kWh", unit_price = 3500, is_metered = true, is_active = true, description = "Điện theo chỉ số sử dụng phòng" },
                new service { code = "WATER", name = "Nước sinh hoạt", unit = "m³", unit_price = 18000, is_metered = true, is_active = true, description = "Nước sinh hoạt theo công tơ" }
            );
            await db.SaveChangesAsync();
        }

        // 6. Dormitory Rules
        if (!await db.dormitory_rules.AnyAsync())
        {
            db.dormitory_rules.AddRange(
                new dormitory_rule { title = "Nội quy khu nội trú KTX", version = "2.0", content = "1. Giữ gìn trật tự và vệ sinh chung; 2. Giờ giới nghiêm từ 23:00 đến 05:00 sáng; 3. Không sử dụng thiết bị sinh nhiệt công suất lớn trong phòng; 4. Đăng ký khách thăm theo đúng quy trình; 5. Bảo vệ tài sản được bàn giao khi nhận phòng.", effective_from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)), is_active = true, created_by_user_id = admin.id, created_at = DateTime.Now },
                new dormitory_rule { title = "Quy định về an toàn phòng cháy chữa cháy (PCCC)", version = "1.5", content = "Nghiêm cấm nấu ăn bằng bếp gas, bếp củi trong phòng; không che chắn công tắc báo cháy; bảo đảm lối thoát hiểm luôn thông thoáng.", effective_from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-6)), is_active = true, created_by_user_id = admin.id, created_at = DateTime.Now }
            );
            await db.SaveChangesAsync();
        }

        // 7. Ensure Comprehensive Rooms (33 phòng, 150+ giường khắp 2 tòa A & B)
        await EnsureComprehensiveRoomsAsync();

        // 8. Ensure Comprehensive Sprints Data (Contracts, Registrations, Invoices, Payments, Repairs...)
        await EnsureComprehensiveSprintDataAsync(admin, studentUser1, studentUser2, studentUser3);
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
            ("A", "A-103", 1, "6 người", 6, 520000, "full", 6),
            ("A", "A-104", 1, "6 người", 6, 520000, "maintenance", 0),
            ("A", "A-201", 2, "4 người", 4, 650000, "available", 1),
            ("A", "A-202", 2, "4 người", 4, 650000, "available", 0),
            ("A", "A-203", 2, "4 người", 4, 650000, "available", 1), // SV001 đang ở
            ("A", "A-204", 2, "6 người", 6, 520000, "available", 0),
            ("A", "A-301", 3, "4 người", 4, 650000, "available", 0),
            ("A", "A-302", 3, "6 người", 6, 520000, "available", 3),
            ("A", "A-303", 3, "6 người", 6, 520000, "full", 6),
            ("A", "A-304", 3, "6 người", 6, 520000, "available", 0),
            ("A", "A-401", 4, "4 người", 4, 650000, "available", 0),
            ("A", "A-402", 4, "6 người", 6, 520000, "available", 2),
            ("A", "A-403", 4, "4 người", 4, 650000, "available", 1),
            ("A", "A-404", 4, "6 người", 6, 520000, "full", 6),

            // TÒA B - KHU NỮ (Tầng 1 -> Tầng 4)
            ("B", "B-101", 1, "4 người", 4, 700000, "available", 0),
            ("B", "B-102", 1, "4 người", 4, 700000, "available", 1), // SV003 đang ở
            ("B", "B-103", 1, "4 người", 4, 700000, "full", 4),
            ("B", "B-104", 1, "6 người", 6, 550000, "available", 0),
            ("B", "B-105", 1, "4 người", 4, 700000, "available", 0),
            ("B", "B-201", 2, "4 người", 4, 700000, "available", 0),
            ("B", "B-202", 2, "4 người", 4, 700000, "available", 2),
            ("B", "B-203", 2, "6 người", 6, 550000, "available", 0),
            ("B", "B-204", 2, "6 người", 6, 550000, "maintenance", 0),
            ("B", "B-301", 3, "4 người", 4, 700000, "available", 0),
            ("B", "B-302", 3, "6 người", 6, 550000, "available", 2),
            ("B", "B-303", 3, "6 người", 6, 550000, "full", 6),
            ("B", "B-304", 3, "4 người", 4, 700000, "available", 0),
            ("B", "B-401", 4, "6 người", 6, 550000, "available", 0),
            ("B", "B-402", 4, "6 người", 6, 550000, "available", 3),
            ("B", "B-403", 4, "4 người", 4, 700000, "available", 0),
            ("B", "B-404", 4, "6 người", 6, 550000, "available", 2)
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
            else
            {
                r.capacity = (short)def.cap;
                r.room_type = def.type;
                r.monthly_price = def.price;
                if (def.status == "maintenance") r.status = "maintenance";
            }

            for (var i = 1; i <= def.cap; i++)
            {
                var bedNum = $"G{i:D2}";
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
            }

            if (r.status != "maintenance")
            {
                var availBeds = def.cap - def.occupiedCount;
                r.status = availBeds <= 0 ? "full" : "available";
            }
        }
        await db.SaveChangesAsync();
    }

    private async Task EnsureComprehensiveSprintDataAsync(user admin, user sv1User, user sv2User, user sv3User)
    {
        var student1 = await db.students.FirstOrDefaultAsync(s => s.user_id == sv1User.id);
        var student2 = await db.students.FirstOrDefaultAsync(s => s.user_id == sv2User.id);
        var student3 = await db.students.FirstOrDefaultAsync(s => s.user_id == sv3User.id);
        if (student1 is null || student2 is null || student3 is null) return;

        var bldgA = await db.buildings.FirstOrDefaultAsync(b => b.code == "A");
        var bldgB = await db.buildings.FirstOrDefaultAsync(b => b.code == "B");
        var roomA203 = await db.rooms.Include(x => x.beds).FirstOrDefaultAsync(x => x.room_number == "A-203");
        var roomB102 = await db.rooms.Include(x => x.beds).FirstOrDefaultAsync(x => x.room_number == "B-102");
        var bedA203_1 = roomA203?.beds.FirstOrDefault(b => b.bed_number == "G01");
        var bedB102_1 = roomB102?.beds.FirstOrDefault(b => b.bed_number == "G01");

        // --- SPRINT 2: Hồ sơ đăng ký phòng (Registrations) ---
        if (!await db.room_registrations.AnyAsync(r => r.registration_code == "DK-202611-DEMO01"))
        {
            db.room_registrations.Add(new room_registration
            {
                registration_code = "DK-202611-DEMO01",
                student_id = student2.id,
                preferred_building_id = bldgA?.id,
                preferred_room_type = "4 người",
                requested_start_date = DateOnly.FromDateTime(DateTime.Today),
                requested_end_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(10)),
                submitted_at = DateTime.Now.AddDays(-2),
                status = "pending",
                note = "[Ưu tiên: Con thương binh, liệt sĩ] · [Tầng: 2] · [Gửi xe: 29B1-9988] · Nguyện vọng ở cùng bạn cùng lớp."
            });
            await db.SaveChangesAsync();
        }

        // --- SPRINT 3: Hợp đồng lưu trú (Contracts) ---
        contract? contract1 = await db.contracts.FirstOrDefaultAsync(c => c.student_id == student1.id && c.status == "active");
        if (contract1 is null && bedA203_1 is not null && roomA203 is not null)
        {
            bedA203_1.status = "occupied";
            contract1 = new contract
            {
                contract_code = "HD-2026-A203-01",
                student_id = student1.id,
                bed_id = bedA203_1.id,
                start_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                end_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(8)),
                monthly_price = roomA203.monthly_price,
                deposit_amount = roomA203.monthly_price,
                status = "active",
                signed_at = DateTime.Now.AddMonths(-2),
                created_at = DateTime.Now.AddMonths(-2),
                terms = "Hợp đồng lưu trú tiêu chuẩn phòng A-203"
            };
            db.contracts.Add(contract1);
            await db.SaveChangesAsync();
        }

        contract? contract3 = await db.contracts.FirstOrDefaultAsync(c => c.student_id == student3.id);
        if (contract3 is null && bedB102_1 is not null && roomB102 is not null)
        {
            bedB102_1.status = "occupied";
            contract3 = new contract
            {
                contract_code = "HD-2026-B102-01",
                student_id = student3.id,
                bed_id = bedB102_1.id,
                start_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1)),
                end_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(9)),
                monthly_price = roomB102.monthly_price,
                deposit_amount = roomB102.monthly_price,
                status = "active",
                signed_at = DateTime.Now.AddMonths(-1),
                created_at = DateTime.Now.AddMonths(-1),
                terms = "Hợp đồng lưu trú tiêu chuẩn phòng B-102"
            };
            db.contracts.Add(contract3);
            await db.SaveChangesAsync();
        }

        // --- SPRINT 4: Dịch vụ & Đăng ký gói (Services & Subscriptions) ---
        var wifiService = await db.services.FirstOrDefaultAsync(s => s.code == "WIFI");
        var parkService = await db.services.FirstOrDefaultAsync(s => s.code == "PARK");
        if (contract1 is not null && wifiService is not null && !await db.service_subscriptions.AnyAsync(ss => ss.contract_id == contract1.id))
        {
            db.service_subscriptions.AddRange(
                new service_subscription
                {
                    contract_id = contract1.id,
                    service_id = wifiService.id,
                    start_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                    quantity = 1,
                    agreed_unit_price = wifiService.unit_price,
                    status = "active",
                    note = "Gói Wifi cá nhân sinh viên"
                },
                new service_subscription
                {
                    contract_id = contract1.id,
                    service_id = parkService!.id,
                    start_date = DateOnly.FromDateTime(DateTime.Today.AddMonths(-2)),
                    quantity = 1,
                    agreed_unit_price = parkService.unit_price,
                    status = "active",
                    note = "Gửi xe máy biển 37B2-12345"
                }
            );
            await db.SaveChangesAsync();
        }

        // --- SPRINT 4: Báo hỏng sửa chữa & Nội quy (Repairs & Rules) ---
        if (roomA203 is not null && !await db.repair_requests.AnyAsync())
        {
            db.repair_requests.AddRange(
                new repair_request
                {
                    request_code = "SC-202611-001",
                    room_id = roomA203.id,
                    student_id = student1.id,
                    reported_by_user_id = sv1User.id,
                    title = "Bóng đèn tuýp LED bàn học bị nhấp nháy",
                    description = "Đèn tuýp LED phía giường G01 chớp tắt liên tục, không đủ ánh sáng học bài buổi tối.",
                    priority = "normal",
                    status = "in_progress",
                    reported_at = DateTime.Now.AddDays(-3),
                    completed_at = null,
                    resolution_note = "Đang giao kỹ thuật viên kiểm tra bóng đèn và dây nối"
                },
                new repair_request
                {
                    request_code = "SC-202611-002",
                    room_id = roomA203.id,
                    student_id = student1.id,
                    reported_by_user_id = sv1User.id,
                    title = "Vòi xịt vệ sinh bị rò rỉ nước",
                    description = "Vòi xịt áp lực trong nhà vệ sinh bị rò nước ở khớp nối van.",
                    priority = "high",
                    status = "submitted",
                    reported_at = DateTime.Now.AddHours(-12),
                    completed_at = null,
                    resolution_note = null
                }
            );
            await db.SaveChangesAsync();
        }

        // Cam kết nội quy
        var mainRule = await db.dormitory_rules.FirstOrDefaultAsync(r => r.is_active);
        if (mainRule is not null && !await db.rule_acknowledgements.AnyAsync(ra => ra.student_id == student1.id))
        {
            db.rule_acknowledgements.Add(new rule_acknowledgement
            {
                rule_id = mainRule.id,
                student_id = student1.id,
                acknowledged_at = DateTime.Now.AddMonths(-2)
            });
            await db.SaveChangesAsync();
        }

        // --- SPRINT 5: Hóa đơn, Tiền cọc & Thanh toán VietQR (Invoices & Payments & Deposits) ---
        if (contract1 is not null && !await db.invoices.AnyAsync(i => i.contract_id == contract1.id))
        {
            // Hóa đơn Tháng trước: ĐÃ THANH TOÁN
            var prevStart = DateOnly.FromDateTime(DateTime.Today.AddMonths(-1));
            var prevEnd = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
            var inv1 = new invoice
            {
                invoice_code = "HD-202610-A203",
                contract_id = contract1.id,
                period_start = prevStart,
                period_end = prevEnd,
                issued_at = DateTime.Now.AddMonths(-1),
                due_date = prevStart.AddDays(10),
                status = "paid",
                note = "Tiền phòng và dịch vụ tháng trước"
            };
            db.invoices.Add(inv1);
            await db.SaveChangesAsync();

            db.invoice_items.AddRange(
                new invoice_item { invoice_id = inv1.id, item_type = "room_rent", description = "Tiền phòng A-203 (Tháng 10/2026)", quantity = 1, unit_price = contract1.monthly_price, amount = contract1.monthly_price },
                new invoice_item { invoice_id = inv1.id, item_type = "service", description = "Internet Wifi tốc độ cao", quantity = 1, unit_price = 50000, amount = 50000 },
                new invoice_item { invoice_id = inv1.id, item_type = "service", description = "Phí gửi xe máy", quantity = 1, unit_price = 80000, amount = 80000 }
            );
            await db.SaveChangesAsync();

            var total1 = contract1.monthly_price + 50000 + 80000;
            db.payments.Add(new payment
            {
                invoice_id = inv1.id,
                payment_code = "TT-202610-001",
                amount = total1,
                method = "bank_transfer",
                status = "completed",
                paid_at = DateTime.Now.AddMonths(-1).AddDays(5),
                note = "Thanh toán qua chuyển khoản VietQR tự động"
            });
            await db.SaveChangesAsync();

            // Hóa đơn Tháng này: CHƯA THANH TOÁN (để demo quét VietQR)
            var curStart = DateOnly.FromDateTime(DateTime.Today);
            var curEnd = DateOnly.FromDateTime(DateTime.Today.AddMonths(1).AddDays(-1));
            var inv2 = new invoice
            {
                invoice_code = "HD-202611-A203",
                contract_id = contract1.id,
                period_start = curStart,
                period_end = curEnd,
                issued_at = DateTime.Now,
                due_date = curStart.AddDays(10),
                status = "issued",
                note = "Tiền phòng và điện nước tháng 11/2026"
            };
            db.invoices.Add(inv2);
            await db.SaveChangesAsync();

            db.invoice_items.AddRange(
                new invoice_item { invoice_id = inv2.id, item_type = "room_rent", description = "Tiền phòng A-203 (Tháng 11/2026)", quantity = 1, unit_price = contract1.monthly_price, amount = contract1.monthly_price },
                new invoice_item { invoice_id = inv2.id, item_type = "service", description = "Internet Wifi tốc độ cao", quantity = 1, unit_price = 50000, amount = 50000 },
                new invoice_item { invoice_id = inv2.id, item_type = "service", description = "Phí gửi xe máy", quantity = 1, unit_price = 80000, amount = 80000 }
            );
            await db.SaveChangesAsync();

            // Giao dịch tiền đặt cọc
            db.deposit_transactions.Add(new deposit_transaction
            {
                contract_id = contract1.id,
                transaction_code = "DC-2026-A203-01",
                transaction_type = "collected",
                amount = contract1.deposit_amount,
                method = "bank_transfer",
                status = "completed",
                transaction_date = DateTime.Now.AddMonths(-2),
                reason = "Tiền đặt cọc phòng khi ký hợp đồng lưu trú (1 tháng tiền phòng)"
            });
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
                    gender = username == "sv003" ? "female" : "male",
                    faculty = "Công nghệ thông tin",
                    course_year = 2,
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
