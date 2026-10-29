using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data;
using QLKTX.Data.Entities;
using QLKTX.ViewModels;

namespace QLKTX.Controllers;

[Authorize]
public sealed class DormitoryController(QlKtxContext db, IPasswordHasher<user> hasher) : Controller
{
    private long UserId => long.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;
    private bool IsStudent => User.IsInRole("student") && !User.IsInRole("system_admin") && !User.IsInRole("dorm_manager");
    private bool IsAdmin => User.IsInRole("system_admin") || User.IsInRole("dorm_manager");
    private bool IsSystemAdmin => User.IsInRole("system_admin");
    private bool IsAccountant => User.IsInRole("accountant");
    private bool IsMaintenance => User.IsInRole("maintenance_staff");

    public async Task<IActionResult> Index()
    {
        long? studentId = IsStudent ? await StudentId() : null;
        var registrations = db.room_registrations.AsQueryable();
        var contracts = db.contracts.AsQueryable();
        var repairs = db.repair_requests.AsQueryable();
        if (studentId is not null)
        {
            registrations = registrations.Where(x => x.student_id == studentId);
            contracts = contracts.Where(x => x.student_id == studentId);
            repairs = repairs.Where(x => x.student_id == studentId);
        }
        else if (!IsAdmin)
        {
            registrations = registrations.Where(_ => false);
            contracts = contracts.Where(_ => false);
            if (!IsMaintenance) repairs = repairs.Where(_ => false);
        }
        var pendingRegistrations = await registrations.CountAsync(x => x.status == "pending");
        var activeContracts = await contracts.CountAsync(x => x.status == "active");
        var openRepairs = await repairs.CountAsync(x => x.status == "submitted" || x.status == "accepted" || x.status == "in_progress");
        var balances = db.v_invoice_balances.AsQueryable();
        if (studentId is not null)
            balances = balances.Where(x => db.contracts.Any(c => c.id == x.contract_id && c.student_id == studentId));
        var outstanding = IsMaintenance ? 0m : await balances.SumAsync(x => x.balance_amount ?? 0m);
        var stations = IsStudent
            ? new List<StationViewModel>
            {
                new("registrations", "Đăng ký", pendingRegistrations, "red"),
                new("contracts", "Hợp đồng", activeContracts, "blue"),
                new("services", "Dịch vụ", await db.service_subscriptions.CountAsync(s => s.contract.student_id == studentId && s.status == "active"), "amber"),
                new("payments", "Thanh toán", await balances.CountAsync(x => (x.balance_amount ?? 0m) > 0m), "green")
            }
            : IsAccountant && !IsAdmin
            ? new List<StationViewModel> { new("payments", "Cần thu", await balances.CountAsync(x => (x.balance_amount ?? 0m) > 0m), "green"), new("payments", "Đã phát hành", await db.invoices.CountAsync(x => x.status == "issued"), "blue") }
            : IsMaintenance && !IsAdmin
            ? new List<StationViewModel> { new("repairs", "Mới gửi", await repairs.CountAsync(x => x.status == "submitted"), "red"), new("repairs", "Đang xử lý", await repairs.CountAsync(x => x.status == "accepted" || x.status == "in_progress"), "amber") }
            : new List<StationViewModel>
            {
                new("registrations", "Chờ duyệt", pendingRegistrations, "red"),
                new("contracts", "Hợp đồng", activeContracts, "blue"),
                new("repairs", "Sửa chữa", openRepairs, "amber"),
                new("payments", "Công nợ", await balances.CountAsync(x => (x.balance_amount ?? 0m) > 0m), "green")
            };

        var tasks = new List<DashboardTaskViewModel>();
        if (IsStudent)
        {
            var latest = await registrations.OrderByDescending(x => x.submitted_at).FirstOrDefaultAsync();
            if (latest is not null) tasks.Add(new(latest.registration_code, $"Yêu cầu phòng · {StatusText(latest.status)}", StatusText(latest.status), "/Dormitory/Workspace?section=registrations"));
            var ownRepairs = await repairs.OrderByDescending(x => x.reported_at).Take(3).ToListAsync();
            tasks.AddRange(ownRepairs.Select(x => new DashboardTaskViewModel(x.title, $"{x.request_code} · {x.room_id}", StatusText(x.status), "/Dormitory/Workspace?section=repairs")));
            tasks.Add(new("Khoản cần thanh toán", $"Số dư hiện tại {outstanding:N0} ₫", outstanding > 0 ? "Cần xử lý" : "Đã cân đối", "/Dormitory/Workspace?section=payments"));
        }
        else if (IsAccountant && !IsAdmin)
        {
            var due = await db.v_invoice_balances.Where(x => (x.balance_amount ?? 0m) > 0m).OrderBy(x => x.due_date).Take(4).ToListAsync();
            tasks.AddRange(due.Select(x => new DashboardTaskViewModel(x.invoice_code, $"Hạn {x.due_date:dd/MM/yyyy} · còn {x.balance_amount ?? 0m:N0} ₫", StatusText(x.status), "/Dormitory/Workspace?section=payments")));
            if (!tasks.Any()) tasks.Add(new("Không còn số dư cần theo dõi", "Các hóa đơn hiện đã cân đối.", "Đã cân đối", "/Dormitory/Workspace?section=reports"));
        }
        else if (IsMaintenance && !IsAdmin)
        {
            var repairList = await repairs.OrderByDescending(x => x.priority == "urgent").ThenBy(x => x.reported_at).Take(5).ToListAsync();
            tasks.AddRange(repairList.Select(x => new DashboardTaskViewModel(x.title, $"{x.request_code} · {StatusText(x.priority)}", StatusText(x.status), "/Dormitory/Workspace?section=repairs")));
            if (!tasks.Any()) tasks.Add(new("Chưa có yêu cầu sửa chữa", "Yêu cầu mới sẽ hiện tại đây.", "Đã cân đối", "/Dormitory/Workspace?section=repairs"));
        }
        else
        {
            var pending = await db.room_registrations.Include(x => x.student).ThenInclude(x => x.user).Where(x => x.status == "pending").OrderBy(x => x.submitted_at).Take(4).ToListAsync();
            tasks.AddRange(pending.Select(x => new DashboardTaskViewModel(x.student.user.full_name, $"{x.registration_code} · {x.preferred_room_type ?? "Chưa chọn loại phòng"}", "Chờ duyệt", "/Dormitory/Workspace?section=registrations")));
            var repairList = await db.repair_requests.Where(x => x.status == "submitted" || x.status == "accepted").OrderByDescending(x => x.priority == "urgent").ThenBy(x => x.reported_at).Take(2).ToListAsync();
            tasks.AddRange(repairList.Select(x => new DashboardTaskViewModel(x.title, $"{x.request_code} · {StatusText(x.priority)}", StatusText(x.status), "/Dormitory/Workspace?section=repairs")));
            tasks.Add(new("Đối soát hóa đơn", $"Số dư chưa thu {outstanding:N0} ₫", outstanding > 0 ? "Cần theo dõi" : "Đã cân đối", "/Dormitory/Workspace?section=payments"));
        }
        return View(new DashboardViewModel { IsStudent = IsStudent, DisplayName = User.Identity?.Name ?? "", PendingRegistrations = pendingRegistrations, AvailableBeds = await db.beds.CountAsync(x => x.status == "available"), ActiveContracts = activeContracts, Outstanding = outstanding, Stations = stations, Tasks = tasks });
    }

    public async Task<IActionResult> Workspace(string section = "rooms", string? station = null)
    {
        section = section.ToLowerInvariant();
        if (IsStudent && section is not ("rooms" or "registrations" or "contracts" or "services" or "repairs" or "rules" or "payments" or "deposits" or "guests" or "violations" or "refunds" or "notifications")) return Forbid();
        if (!IsStudent && !IsAdmin && !((IsMaintenance && section is ("repairs" or "rooms" or "assets" or "meters" or "utilities" or "notifications")) || (IsAccountant && section is ("payments" or "deposits" or "refunds" or "reports" or "notifications")))) return Forbid();
        if (section == "users" && !IsSystemAdmin) return Forbid();
        long? studentId = IsStudent ? await StudentId() : null;
        var model = new ModuleViewModel { Section = section };
        switch (section)
        {
            case "rooms":
                if (IsStudent && studentId is not null)
                {
                    var rooms = await db.rooms.Include(x => x.building).Include(x => x.beds).Where(x => x.status == "available").OrderBy(x => x.building.code).ThenBy(x => x.room_number).ToListAsync();
                    model = Build("rooms", "Phòng và giường", "Chỗ ở đang mở đăng ký tại các tòa. Số giường trống được tính từ danh sách giường thực tế.", ["Tòa / phòng", "Loại phòng", "Còn trống", "Giá tháng", "Tình trạng"], rooms.Select(x => Row(x.id, [$"{x.building.name} · {x.room_number}", x.room_type ?? "—", x.beds.Count(b => b.status == "available").ToString(), $"{x.monthly_price:N0} ₫", StatusText(x.status)], x.status)).ToList());
                }
                else
                {
                    var rooms = await db.rooms.Include(x => x.building).Include(x => x.beds).OrderBy(x => x.building.code).ThenBy(x => x.room_number).ToListAsync();
                    model = Build("rooms", "Tòa, phòng và giường", "Theo dõi năng lực, giá và tình trạng từng chỗ ở.", ["Tòa / phòng", "Tầng", "Loại phòng", "Giường trống / sức chứa", "Giá tháng", "Trạng thái"], rooms.Select(x => Row(x.id, [$"{x.building.name} · {x.room_number}", x.floor_number?.ToString() ?? "—", x.room_type ?? "—", $"{x.beds.Count(b => b.status == "available")} / {x.capacity}", $"{x.monthly_price:N0} ₫", StatusText(x.status)], x.status, !IsAdmin || x.status == "maintenance" ? [] : [new RowActionViewModel("Bảo trì", "room-maintenance"), new RowActionViewModel("Mở phòng", "room-open")])).ToList(), "Thêm phòng", "Mã phòng, tòa, tầng, loại, sức chứa, giá tháng.", [
                        new("buildingId", "Tòa", "select", true, await db.buildings.Where(x => x.is_active).Select(x => new SelectOptionViewModel(x.id.ToString(), x.name)).ToListAsync()),
                        new("roomNumber", "Mã phòng"), new("floor", "Tầng", "number"), new("roomType", "Loại phòng"), new("capacity", "Sức chứa", "number"), new("price", "Giá tháng (₫)", "number")]);
                }
                break;
            case "buildings":
                var buildings = await db.buildings.Include(x => x.rooms).OrderBy(x => x.code).ToListAsync();
                model = Build("buildings", "Tòa nhà", "Danh mục khu nhà đang vận hành.", ["Mã tòa", "Tên tòa", "Địa chỉ", "Số phòng", "Trạng thái"], buildings.Select(x => Row(x.id, [x.code, x.name, x.address ?? "—", x.rooms.Count.ToString(), x.is_active ? "Đang dùng" : "Đã khóa"], x.is_active ? "active" : "inactive")).ToList(), "Thêm tòa", "Tạo một tòa nhà mới.", [new("code", "Mã tòa"), new("name", "Tên tòa"), new("address", "Địa chỉ", "text", false), new("description", "Ghi chú", "text", false)]);
                break;
            case "registrations":
                var regs = db.room_registrations.Include(x => x.student).ThenInclude(x => x.user).Include(x => x.preferred_building).AsQueryable();
                if (studentId is not null) regs = regs.Where(x => x.student_id == studentId);
                if (station == "registrations") regs = regs.Where(x => x.status == "pending");
                var regRows = await regs.OrderByDescending(x => x.submitted_at).ToListAsync();
                model = Build("registrations", "Đăng ký phòng", IsStudent ? "Theo dõi yêu cầu và thời gian dự kiến nội trú của bạn." : "Xem xét hồ sơ, phân giường còn trống và tạo hợp đồng trong một giao dịch.", ["Mã đăng ký", "Sinh viên", "Nguyện vọng", "Thời gian", "Trạng thái"], regRows.Select(x => Row(x.id, [x.registration_code, $"{x.student.user.full_name} · {x.student.student_code}", $"{x.preferred_building?.name ?? "Không chọn tòa"} · {x.preferred_room_type ?? "Mọi loại"}", $"{x.requested_start_date:dd/MM/yyyy} — {x.requested_end_date:dd/MM/yyyy}", StatusText(x.status)], x.status,
                    IsStudent && x.status == "pending" ? [new RowActionViewModel("Hủy đăng ký", "registration-cancel")] : IsStudent || x.status != "pending" ? [] : [new RowActionViewModel("Duyệt & lập hợp đồng", "approve", true), new RowActionViewModel("Từ chối", "reject")])).ToList(), IsStudent ? "Gửi đăng ký mới" : null, IsStudent ? "Gửi yêu cầu đến ban quản lý. Hồ sơ demo có thể chỉnh sửa sau khi gửi." : null, IsStudent ? [
                        new("buildingId", "Tòa mong muốn", "select", false, await db.buildings.Where(x => x.is_active).Select(x => new SelectOptionViewModel(x.id.ToString(), x.name)).ToListAsync()),
                        new("roomType", "Loại phòng", "select", true, [new("4 người", "4 người"), new("6 người", "6 người"), new("2 người", "2 người")]),
                        new("startDate", "Ngày bắt đầu", "date"), new("endDate", "Ngày kết thúc", "date"), new("note", "Ghi chú", "text", false)
                    ] : []);
                break;
            case "contracts":
                var cont = db.contracts.Include(x => x.student).ThenInclude(x => x.user).Include(x => x.bed).ThenInclude(x => x.room).ThenInclude(x => x.building).Include(x => x.deposit_transactions).AsQueryable();
                if (studentId is not null) cont = cont.Where(x => x.student_id == studentId);
                if (station == "contracts") cont = cont.Where(x => x.status == "active");
                var contracts = await cont.OrderByDescending(x => x.created_at).ToListAsync();
                model = Build("contracts", "Hợp đồng lưu trú", "Ngày hiệu lực, vị trí được phân và trạng thái nhận/trả phòng.", ["Mã hợp đồng", "Sinh viên", "Vị trí", "Thời hạn", "Tiền phòng / cọc", "Cọc đã xác nhận", "Trạng thái"], contracts.Select(x =>
                {
                    var depositNet = x.deposit_transactions.Where(d => d.status == "completed").Sum(d => d.transaction_type == "collected" ? d.amount : -d.amount);
                    List<RowActionViewModel> actions = IsStudent ? [] : x.status == "draft" ? depositNet >= x.deposit_amount ? [new RowActionViewModel("Ghi nhận ký", "sign", true), new RowActionViewModel("Hủy hợp đồng", "cancel-contract")] : [new RowActionViewModel("Theo dõi tiền cọc", "open-deposits")] : x.status == "active" ? [new RowActionViewModel("Nhận phòng", "checkin", true), new RowActionViewModel("Trả phòng", "checkout")] : [];
                    return Row(x.id, [x.contract_code, x.student.user.full_name, $"{x.bed.room.building.code} · {x.bed.room.room_number} · {x.bed.bed_number}", $"{x.start_date:dd/MM/yyyy} — {x.end_date:dd/MM/yyyy}", $"{x.monthly_price:N0} / {x.deposit_amount:N0} ₫", $"{depositNet:N0} ₫", StatusText(x.status)], x.status, actions);
                }).ToList());
                break;
            case "services":
                var services = await db.services.OrderBy(x => x.name).ToListAsync();
                var subscriptions = db.service_subscriptions.Include(x => x.service).Include(x => x.contract).ThenInclude(x => x.student).ThenInclude(x => x.user).Include(x => x.contract).ThenInclude(x => x.bed).ThenInclude(x => x.room).AsQueryable();
                if (studentId is not null) subscriptions = subscriptions.Where(x => x.contract.student_id == studentId);
                if (studentId is not null && station == "services") subscriptions = subscriptions.Where(x => x.status == "active");
                var subs = await subscriptions.OrderByDescending(x => x.start_date).Take(50).ToListAsync();
                var serviceRows = services.Select(x => Row(x.id, [x.code, x.name, x.unit, $"{x.unit_price:N0} ₫", x.is_metered ? "Theo chỉ số" : "Gói tháng", x.is_active ? "Đang cung cấp" : "Đã dừng"], x.is_active ? "active" : "inactive", IsStudent && x.is_active ? [new RowActionViewModel("Đăng ký dịch vụ", "subscribe", true)] : [])).ToList();
                serviceRows.AddRange(subs.Select(x => Row(x.id, [$"↳ {x.service.name}", x.contract.contract_code, $"{x.contract.bed.room.room_number} · {x.contract.student.user.full_name}", $"{x.agreed_unit_price:N0} ₫", $"Từ {x.start_date:dd/MM/yyyy}", StatusText(x.status)], x.status, IsStudent && x.status == "active" ? [new RowActionViewModel("Ngừng", "unsubscribe")] : [])));
                model = Build("services", "Dịch vụ nội trú", "Danh mục dịch vụ và các đăng ký gắn với hợp đồng đang ở.", ["Mã / dịch vụ", "Mô tả / hợp đồng", "Đơn vị / phòng", "Đơn giá", "Cách tính / thời gian", "Trạng thái"], serviceRows, IsStudent ? "Chọn gói dịch vụ" : "Thêm dịch vụ", "Mọi khoản dịch vụ được ghi nhận theo hợp đồng và đơn giá tại thời điểm đăng ký.", IsStudent ? [new("serviceId", "Dịch vụ", "select", true, services.Where(x => x.is_active).Select(x => new SelectOptionViewModel(x.id.ToString(), $"{x.name} · {x.unit_price:N0} ₫")).ToList())] : [new("code", "Mã dịch vụ"), new("name", "Tên dịch vụ"), new("unit", "Đơn vị"), new("price", "Đơn giá", "number"), new("metered", "Theo chỉ số?", "select", true, [new("false", "Không"), new("true", "Có")]), new("description", "Mô tả", "text", false)]);
                break;
            case "repairs":
                var repairs = db.repair_requests.Include(x => x.room).ThenInclude(x => x.building).Include(x => x.student).ThenInclude(x => x!.user).AsQueryable();
                if (studentId is not null) repairs = repairs.Where(x => x.student_id == studentId);
                if (station == "repairs") repairs = repairs.Where(x => x.status == "submitted" || x.status == "accepted" || x.status == "in_progress");
                var repairList = await repairs.OrderByDescending(x => x.reported_at).ToListAsync();
                model = Build("repairs", "Sửa chữa và sự cố", "Mỗi yêu cầu lưu phòng, người báo, mức ưu tiên và lịch sử xử lý.", ["Mã / sự cố", "Phòng", "Người báo", "Ưu tiên", "Gửi lúc", "Trạng thái"], repairList.Select(x => Row(x.id, [$"{x.request_code} · {x.title}", $"{x.room.building.code} · {x.room.room_number}", x.student?.user.full_name ?? x.reported_by_user_id.ToString(), StatusText(x.priority), x.reported_at.ToString("dd/MM/yyyy HH:mm"), StatusText(x.status)], x.status, IsStudent || x.status is "completed" or "rejected" ? [] : [new RowActionViewModel("Tiếp nhận", "repair-accept"), new RowActionViewModel("Đang xử lý", "repair-progress"), new RowActionViewModel("Hoàn tất", "repair-complete", true), new RowActionViewModel("Từ chối", "repair-reject")])).ToList(), IsStudent ? "Báo sự cố" : null, IsStudent ? "Yêu cầu sẽ gắn với phòng trong hợp đồng hiện tại." : null, IsStudent ? [new("title", "Sự cố"), new("description", "Mô tả", "text"), new("priority", "Mức độ", "select", true, [new("normal", "Thông thường"), new("high", "Cao"), new("urgent", "Khẩn cấp")])] : []);
                break;
            case "rules":
                var rules = await db.dormitory_rules.OrderByDescending(x => x.effective_from).ToListAsync();
                var ack = studentId is null ? new HashSet<long>() : (await db.rule_acknowledgements.Where(x => x.student_id == studentId).Select(x => x.rule_id).ToListAsync()).ToHashSet();
                model = Build("rules", "Nội quy khu nội trú", "Phiên bản, hiệu lực và xác nhận đã đọc của sinh viên.", ["Nội quy", "Phiên bản", "Hiệu lực từ", "Nội dung", "Tình trạng"], rules.Select(x => Row(x.id, [x.title, x.version, x.effective_from.ToString("dd/MM/yyyy"), x.content, studentId is null ? (x.is_active ? "Đang áp dụng" : "Đã lưu trữ") : (ack.Contains(x.id) ? "Đã xác nhận" : "Chưa xác nhận")], x.is_active ? "active" : "inactive", studentId is not null && x.is_active && !ack.Contains(x.id) ? [new RowActionViewModel("Tôi đã đọc", "acknowledge", true)] : [])).ToList(), IsStudent ? null : "Thêm nội quy", null, IsStudent ? [] : [new("title", "Tiêu đề"), new("version", "Phiên bản"), new("effectiveFrom", "Ngày hiệu lực", "date"), new("content", "Nội dung", "textarea")]);
                break;
            case "payments":
                var balancesQuery = db.v_invoice_balances.AsQueryable();
                if (studentId is not null) balancesQuery = balancesQuery.Where(x => db.contracts.Any(c => c.id == x.contract_id && c.student_id == studentId));
                if (station == "payments") balancesQuery = balancesQuery.Where(x => (x.balance_amount ?? 0m) > 0m);
                var balancesList = await balancesQuery.OrderByDescending(x => x.due_date).ToListAsync();
                var paymentRows = new List<ModuleRowViewModel>();
                foreach (var x in balancesList)
                {
                    var pendingPayment = await db.payments.AnyAsync(p => p.invoice_id == x.invoice_id && p.status == "pending");
                    var actions = new List<RowActionViewModel>();
                    if (IsStudent && (x.balance_amount ?? 0m) > 0m && !pendingPayment) actions.Add(new("Tạo yêu cầu thanh toán", "payment-request", true));
                    if (!IsStudent && pendingPayment) actions.Add(new("Xác nhận đã thu", "payment-confirm", true));
                    paymentRows.Add(Row(x.invoice_id, [x.invoice_code, $"{x.period_start:MM/yyyy}", x.due_date.ToString("dd/MM/yyyy"), $"{x.total_amount ?? 0m:N0} ₫", $"{x.paid_amount ?? 0m:N0} ₫", $"{x.balance_amount ?? 0m:N0} ₫", pendingPayment ? "Chờ xác nhận" : StatusText(x.status)], (x.balance_amount ?? 0m) > 0m ? "pending" : "paid", actions));
                }
                model = Build("payments", "Hóa đơn và thanh toán", "Yêu cầu thanh toán được lưu ở trạng thái chờ; nhân viên kế toán xác nhận giao dịch thực nhận.", ["Mã hóa đơn", "Kỳ", "Hạn thanh toán", "Tổng tiền", "Đã thu", "Còn lại", "Trạng thái"], paymentRows, IsStudent ? null : "Lập hóa đơn", null, IsStudent ? [] : [new("contractId", "Hợp đồng", "select", true, await db.contracts.Where(x => x.status == "active").Include(x => x.student).ThenInclude(x => x.user).Select(x => new SelectOptionViewModel(x.id.ToString(), $"{x.contract_code} · {x.student.user.full_name}")).ToListAsync()), new("periodStart", "Kỳ từ", "date"), new("periodEnd", "Kỳ đến", "date"), new("dueDate", "Hạn thanh toán", "date")]);
                break;
            case "deposits":
                var depositQuery = db.deposit_transactions.Include(x => x.contract).ThenInclude(x => x.student).ThenInclude(x => x.user).AsQueryable();
                if (studentId is not null) depositQuery = depositQuery.Where(x => x.contract.student_id == studentId);
                var depositList = await depositQuery.OrderByDescending(x => x.transaction_date).ToListAsync();
                model = Build("deposits", "Tiền đặt cọc", "Mọi khoản thu cọc chờ kế toán đối chiếu với giao dịch thực nhận trước khi kích hoạt hợp đồng.", ["Mã giao dịch", "Hợp đồng", "Sinh viên", "Nghiệp vụ", "Số tiền", "Hình thức", "Ngày ghi nhận", "Trạng thái"], depositList.Select(x => Row(x.id, [x.transaction_code, x.contract.contract_code, x.contract.student.user.full_name, StatusText(x.transaction_type), $"{x.amount:N0} ₫", StatusText(x.method), x.transaction_date.ToString("dd/MM/yyyy HH:mm"), x.status == "pending" ? "Chờ đối chiếu" : StatusText(x.status)], x.status, !IsStudent && x.status == "pending" ? [new RowActionViewModel("Xác nhận đã thu", "deposit-confirm", true)] : [])).ToList(), IsStudent ? "Yêu cầu nộp cọc" : null, IsStudent ? "Yêu cầu sẽ ở trạng thái chờ đến khi ban quản lý đối chiếu tiền thực nhận." : null, IsStudent ? [new("contractId", "Hợp đồng", "select", true, await db.contracts.Where(x => x.student_id == studentId && (x.status == "draft" || x.status == "active")).Include(x => x.student).ThenInclude(x => x.user).Select(x => new SelectOptionViewModel(x.id.ToString(), x.contract_code + " · cọc " + x.deposit_amount.ToString("N0") + " ₫")).ToListAsync())] : []);
                break;
            case "guests":
                var guestQuery = db.guest_visits.Include(x => x.student).ThenInclude(x => x.user).AsQueryable();
                if (studentId is not null) guestQuery = guestQuery.Where(x => x.student_id == studentId);
                var guestRows = await guestQuery.OrderByDescending(x => x.expected_arrival).Take(200).ToListAsync();
                model = Build("guests", "Khách thăm", "Sinh viên đăng ký trước; ban quản lý duyệt và ghi nhận thời điểm vào, ra.", ["Khách", "Sinh viên", "Mục đích", "Dự kiến", "Thực tế", "Trạng thái"], guestRows.Select(x => Row(x.id, [x.guest_name, x.student.user.full_name, x.visit_purpose ?? "—", $"{x.expected_arrival:dd/MM/yyyy HH:mm} — {x.expected_departure:dd/MM HH:mm}", $"{x.actual_arrival:dd/MM HH:mm} — {x.actual_departure:dd/MM HH:mm}", StatusText(x.status)], x.status,
                    IsStudent && x.status == "pending" ? [new RowActionViewModel("Hủy yêu cầu", "guest-cancel")] : IsStudent ? [] : x.status == "pending" ? [new RowActionViewModel("Duyệt", "guest-approve", true), new RowActionViewModel("Từ chối", "guest-reject")] : x.status == "approved" ? [new RowActionViewModel("Ghi nhận khách đến", "guest-arrive", true)] : x.status == "checked_in" ? [new RowActionViewModel("Ghi nhận khách rời", "guest-depart", true)] : [])).ToList(), IsStudent ? "Đăng ký khách" : null, IsStudent ? "Chỉ thu thập thông tin tối thiểu để kiểm soát khách ra vào khu nội trú." : null, IsStudent ? [new("guestName", "Họ tên khách"), new("guestPhone", "Số điện thoại", "tel", false), new("purpose", "Mục đích", "text", false), new("arrival", "Dự kiến đến", "datetime-local"), new("departure", "Dự kiến rời", "datetime-local", false)] : []);
                break;
            case "assets":
                var assetList = await db.room_assets.Include(x => x.room).ThenInclude(x => x.building).Include(x => x.asset_type).OrderBy(x => x.room.building.code).ThenBy(x => x.room.room_number).ThenBy(x => x.name).ToListAsync();
                model = Build("assets", "Tài sản phòng", "Theo dõi số lượng và tình trạng tài sản gắn với từng phòng.", ["Mã / tài sản", "Tòa / phòng", "Nhóm", "Số lượng", "Kiểm kê gần nhất", "Tình trạng"], assetList.Select(x => Row(x.id, [$"{x.asset_tag ?? "Chưa gắn mã"} · {x.name}", $"{x.room.building.code} · {x.room.room_number}", x.asset_type.name, x.quantity.ToString(), x.last_checked_at?.ToString("dd/MM/yyyy") ?? "Chưa kiểm kê", StatusText(x.condition_status)], x.condition_status, IsStudent ? [] : [new RowActionViewModel("Ghi nhận kiểm kê", "asset-check")])).ToList(), IsAdmin ? "Ghi nhận tài sản" : null, null, IsAdmin ? [new("roomId", "Phòng", "select", true, await db.rooms.Include(x => x.building).OrderBy(x => x.building.code).ThenBy(x => x.room_number).Select(x => new SelectOptionViewModel(x.id.ToString(), x.building.code + " · " + x.room_number)).ToListAsync()), new("assetTypeId", "Nhóm tài sản", "select", true, await db.asset_types.OrderBy(x => x.name).Select(x => new SelectOptionViewModel(x.id.ToString(), x.name)).ToListAsync()), new("assetTag", "Mã tài sản", "text", false), new("name", "Tên tài sản"), new("quantity", "Số lượng", "number"), new("condition", "Tình trạng", "select", true, [new("new", "Mới"), new("good", "Tốt"), new("worn", "Hao mòn"), new("damaged", "Hư hỏng")]), new("acquiredAt", "Ngày nhập", "date", false)] : []);
                break;
            case "meters":
                var meterList = await db.utility_meters.Include(x => x.room).ThenInclude(x => x.building).Include(x => x.service).Include(x => x.utility_readings).OrderBy(x => x.room.building.code).ThenBy(x => x.room.room_number).ThenBy(x => x.meter_code).ToListAsync();
                model = Build("meters", "Đồng hồ điện nước", "Mã hóa từng đồng hồ theo phòng và dịch vụ; mỗi đồng hồ chỉ có một bản ghi cho mỗi ngày.", ["Mã đồng hồ", "Tòa / phòng", "Dịch vụ", "Lắp đặt", "Chỉ số gần nhất", "Trạng thái"], meterList.Select(x => { var latest = x.utility_readings.OrderByDescending(r => r.reading_date).FirstOrDefault(); return Row(x.id, [x.meter_code, $"{x.room.building.code} · {x.room.room_number}", x.service.name, x.installed_at?.ToString("dd/MM/yyyy") ?? "—", latest is null ? "Chưa ghi chỉ số" : $"{latest.current_value:N3} · {latest.reading_date:dd/MM/yyyy}", x.is_active ? "Đang hoạt động" : "Đã ngừng"], x.is_active ? "active" : "inactive", IsAdmin ? [new RowActionViewModel(x.is_active ? "Ngừng đồng hồ" : "Mở lại", "meter-toggle")] : []); }).ToList(), IsAdmin ? "Lắp đồng hồ" : null, null, IsAdmin ? [new("roomId", "Phòng", "select", true, await db.rooms.Include(x => x.building).OrderBy(x => x.building.code).ThenBy(x => x.room_number).Select(x => new SelectOptionViewModel(x.id.ToString(), x.building.code + " · " + x.room_number)).ToListAsync()), new("serviceId", "Dịch vụ định mức", "select", true, await db.services.Where(x => x.is_metered && x.is_active).Select(x => new SelectOptionViewModel(x.id.ToString(), x.name + " · " + x.unit)).ToListAsync()), new("meterCode", "Mã đồng hồ"), new("installedAt", "Ngày lắp", "date", false)] : []);
                break;
            case "utilities":
                var readings = await db.utility_readings.Include(x => x.meter).ThenInclude(x => x.room).ThenInclude(x => x.building).Include(x => x.meter).ThenInclude(x => x.service).OrderByDescending(x => x.reading_date).Take(200).ToListAsync();
                model = Build("utilities", "Chỉ số và tiêu thụ", "Ghi chỉ số mới để tính sản lượng, thành tiền và phân bổ đều cho các hợp đồng đang ở trong phòng.", ["Ngày ghi", "Đồng hồ / phòng", "Dịch vụ", "Chỉ số cũ → mới", "Tiêu thụ", "Đơn giá", "Thành tiền"], readings.Select(x => Row(x.id, [x.reading_date.ToString("dd/MM/yyyy"), $"{x.meter.meter_code} · {x.meter.room.building.code} {x.meter.room.room_number}", x.meter.service.name, $"{x.previous_value:N3} → {x.current_value:N3}", x.consumption.ToString("N3"), $"{x.unit_price:N0} ₫", $"{x.total_amount:N0} ₫"], "report")).ToList(), IsAdmin || IsMaintenance ? "Ghi chỉ số mới" : null, "Sản lượng được chia đều cho hợp đồng đang có hiệu lực trong phòng; các lượt sử dụng được đưa vào hóa đơn ở mục Thanh toán.", IsAdmin || IsMaintenance ? [new("meterId", "Đồng hồ", "select", true, await db.utility_meters.Where(x => x.is_active).Include(x => x.room).ThenInclude(x => x.building).Include(x => x.service).Select(x => new SelectOptionViewModel(x.id.ToString(), x.meter_code + " · " + x.room.building.code + " " + x.room.room_number + " · " + x.service.name)).ToListAsync()), new("readingDate", "Ngày ghi", "date"), new("currentValue", "Chỉ số mới", "number"), new("note", "Ghi chú", "text", false)] : []);
                break;
            case "violations":
                var violationQuery = db.student_violations.Include(x => x.student).ThenInclude(x => x.user).Include(x => x.rule).Include(x => x.violation_invoice_item).AsQueryable();
                if (studentId is not null) violationQuery = violationQuery.Where(x => x.student_id == studentId);
                var violations = await violationQuery.OrderByDescending(x => x.occurred_at).Take(200).ToListAsync();
                model = Build("violations", "Vi phạm nội quy", "Biên bản, mức phạt, trạng thái xử lý và trạng thái lập khoản thu được lưu riêng.", ["Mã / nội dung", "Sinh viên", "Nội quy", "Thời điểm", "Mức phạt", "Khoản thu", "Trạng thái"], violations.Select(x => Row(x.id, [$"{x.violation_code} · {x.title}", x.student.user.full_name, x.rule?.title ?? "—", x.occurred_at.ToString("dd/MM/yyyy HH:mm"), $"{x.fine_amount:N0} ₫", x.violation_invoice_item is null ? "Chưa lập hóa đơn" : "Đã lập hóa đơn", StatusText(x.status)], x.status, IsStudent ? [] : x.status is "resolved" or "cancelled" ? [] : [new RowActionViewModel("Đưa vào xem xét", "violation-review"), new RowActionViewModel("Lập khoản phạt", "violation-invoice"), new RowActionViewModel("Đóng biên bản", "violation-resolve")])).ToList(), IsAdmin ? "Lập biên bản" : null, null, IsAdmin ? [new("studentId", "Sinh viên", "select", true, await db.students.Include(x => x.user).OrderBy(x => x.student_code).Select(x => new SelectOptionViewModel(x.id.ToString(), x.student_code + " · " + x.user.full_name)).ToListAsync()), new("ruleId", "Nội quy liên quan", "select", false, await db.dormitory_rules.Where(x => x.is_active).Select(x => new SelectOptionViewModel(x.id.ToString(), x.title)).ToListAsync()), new("title", "Nội dung vi phạm"), new("occurredAt", "Thời điểm", "datetime-local"), new("fineAmount", "Mức phạt (₫)", "number"), new("description", "Mô tả", "textarea", false)] : []);
                break;
            case "refunds":
                var refundQuery = db.payment_refunds.Include(x => x.payment).ThenInclude(x => x.invoice).ThenInclude(x => x.contract).ThenInclude(x => x.student).ThenInclude(x => x.user).AsQueryable();
                if (studentId is not null) refundQuery = refundQuery.Where(x => x.payment.invoice.contract.student_id == studentId);
                var refunds = await refundQuery.OrderByDescending(x => x.created_at).Take(200).ToListAsync();
                model = Build("refunds", "Yêu cầu hoàn tiền", "Sinh viên gửi yêu cầu; nhân viên chỉ ghi nhận hoàn thành sau khi đã hoàn tiền thực tế ngoài hệ thống.", ["Mã yêu cầu", "Khoản thu", "Sinh viên", "Số tiền", "Lý do", "Ngày gửi", "Trạng thái"], refunds.Select(x => Row(x.id, [x.refund_code, x.payment.payment_code, x.payment.invoice.contract.student.user.full_name, $"{x.amount:N0} ₫", x.reason ?? "—", x.created_at.ToString("dd/MM/yyyy"), StatusText(x.status)], x.status, !IsStudent && x.status == "pending" ? [new RowActionViewModel("Ghi nhận đã hoàn", "refund-complete", true), new RowActionViewModel("Từ chối", "refund-reject")] : [])).ToList(), IsStudent ? "Yêu cầu hoàn tiền" : null, "Yêu cầu không tự chuyển tiền; kế toán đối chiếu và xử lý thực tế trước khi ghi nhận hoàn tất.", IsStudent ? [new("paymentId", "Khoản đã thu", "select", true, await db.payments.Where(x => x.status == "completed" && x.invoice.contract.student_id == studentId).Include(x => x.invoice).ThenInclude(x => x.contract).Select(x => new SelectOptionViewModel(x.id.ToString(), x.payment_code + " · " + x.amount.ToString("N0") + " ₫")).ToListAsync()), new("amount", "Số tiền đề nghị (₫)", "number"), new("reason", "Lý do", "textarea")] : []);
                break;
            case "notifications":
                var notificationQuery = db.notifications.Where(x => x.user_id == UserId).OrderByDescending(x => x.created_at);
                var notices = await notificationQuery.Take(100).ToListAsync();
                model = Build("notifications", "Thông báo của tôi", "Lịch sử thông báo riêng của tài khoản đang đăng nhập.", ["Tiêu đề", "Nội dung", "Phân loại", "Ngày gửi", "Trạng thái"], notices.Select(x => Row(x.id, [x.title, x.message, x.notification_type, x.created_at.ToString("dd/MM/yyyy HH:mm"), x.read_at is null ? "Chưa đọc" : $"Đã đọc · {x.read_at:dd/MM/yyyy HH:mm}"], x.read_at is null ? "pending" : "completed", x.read_at is null ? [new RowActionViewModel("Đánh dấu đã đọc", "notification-read", true)] : [])).ToList(), IsAdmin ? "Gửi thông báo" : null, null, IsAdmin ? [new("userId", "Người nhận", "select", true, await db.users.Where(x => x.is_active).OrderBy(x => x.full_name).Select(x => new SelectOptionViewModel(x.id.ToString(), x.full_name + " · " + x.username)).ToListAsync()), new("title", "Tiêu đề"), new("message", "Nội dung", "textarea"), new("notificationType", "Phân loại", "select", true, [new("general", "Thông báo chung"), new("billing", "Tài chính"), new("operation", "Vận hành")])] : []);
                break;
            case "reports":
                if (!(IsAdmin || IsAccountant)) return Forbid();
                var buildingsReport = await db.v_building_occupancies.OrderBy(x => x.building_code).ToListAsync();
                model = Build("reports", "Thống kê vận hành", "Dữ liệu tổng hợp trực tiếp từ các view SQL Server.", ["Tòa", "Phòng", "Sức chứa", "Đang ở / còn trống", "Tình trạng"], buildingsReport.Select(x => Row(x.building_id, [x.building_code + " · " + x.building_name, x.room_count?.ToString() ?? "0", x.nominal_capacity?.ToString() ?? "0", $"{x.current_occupants ?? 0} / {x.available_slots ?? 0}", "Tổng hợp từ hợp đồng hiệu lực"], "active")).ToList());
                var monthly = await db.v_monthly_invoice_summaries.OrderByDescending(x => x.billing_month).Take(12).ToListAsync();
                model.Rows.AddRange(monthly.Select((x, i) => Row(-i - 1, ["Thu chi · " + (x.billing_month?.ToString("MM/yyyy") ?? "—"), x.invoice_count?.ToString() ?? "0", "—", "—", $"Đã lập {x.billed_amount ?? 0m:N0} ₫ · Đã thu {x.collected_amount ?? 0m:N0} ₫ · Còn {x.outstanding_amount ?? 0m:N0} ₫"], "report")));
                break;
            case "users":
                if (!IsSystemAdmin) return Forbid();
                var users = await db.users.Include(x => x.user_roles).ThenInclude(x => x.role).OrderBy(x => x.username).ToListAsync();
                model = Build("users", "Tài khoản và phân quyền", "Tài khoản được gắn vai trò từ ma trận quyền của cơ sở dữ liệu.", ["Tên đăng nhập", "Họ tên", "Email", "Vai trò", "Lần đăng nhập", "Trạng thái"], users.Select(x => Row(x.id, [x.username, x.full_name, x.email ?? "—", string.Join(", ", x.user_roles.Select(r => r.role.name)), x.last_login_at?.ToString("dd/MM/yyyy HH:mm") ?? "Chưa đăng nhập", x.is_active ? "Đang hoạt động" : "Đã khóa"], x.is_active ? "active" : "inactive", x.id == UserId ? [] : [new RowActionViewModel(x.is_active ? "Khóa" : "Mở khóa", "user-toggle")])).ToList(), "Tạo tài khoản", "Tài khoản demo đã có mật khẩu mẫu; đổi mật khẩu trước khi triển khai thật.", [new("username", "Tên đăng nhập"), new("fullName", "Họ tên"), new("email", "Email", "email", false), new("password", "Mật khẩu", "password"), new("roleId", "Vai trò", "select", true, await db.roles.Select(x => new SelectOptionViewModel(x.id.ToString(), x.name)).ToListAsync())]);
                break;
            default:
                return RedirectToAction(nameof(Workspace), new { section = "rooms" });
        }
        ViewData["ActiveSection"] = section;
        return View("Workspace", model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string section, IFormCollection form)
    {
        section = section.ToLowerInvariant();
        if (IsStudent && section is not ("registrations" or "services" or "repairs" or "guests" or "refunds")) return Forbid();
        try
        {
            if (section == "registrations")
            {
                var studentId = await StudentId();
                var start = DateOnly.Parse(form["startDate"]!);
                var end = DateOnly.Parse(form["endDate"]!);
                if (end <= start) throw new InvalidOperationException("Ngày kết thúc phải sau ngày bắt đầu.");
                if (await db.room_registrations.AnyAsync(x => x.student_id == studentId && x.status == "pending")) throw new InvalidOperationException("Bạn đang có một yêu cầu chờ duyệt.");
                var newRegistration = new room_registration { registration_code = "DK-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(), student_id = studentId, preferred_building_id = NullableLong(form, "buildingId"), preferred_room_type = form["roomType"], requested_start_date = start, requested_end_date = end, submitted_at = DateTime.Now, status = "pending", note = form["note"] };
                db.room_registrations.Add(newRegistration);
                await db.SaveChangesAsync();
                Audit("create", section, newRegistration.id, "room registration submitted");
                await NotifyRole("dorm_manager", "Có hồ sơ đăng ký phòng mới", $"{newRegistration.registration_code} đang chờ xét duyệt.", "operation", "room_registration", newRegistration.id);
                await db.SaveChangesAsync();
                return Done("Đã gửi đăng ký phòng.", section);
            }
            if (section == "deposits")
            {
                if (!IsStudent) return Forbid();
                var ownContractId = Long(form, "contractId");
                var ownContract = await db.contracts.Include(x => x.deposit_transactions).SingleAsync(x => x.id == ownContractId && x.student.user_id == UserId && (x.status == "draft" || x.status == "active"));
                var depositNet = ownContract.deposit_transactions.Where(x => x.status == "completed").Sum(x => x.transaction_type == "collected" ? x.amount : -x.amount);
                var remaining = ownContract.deposit_amount - depositNet;
                if (remaining <= 0) throw new InvalidOperationException("Khoản cọc đã được đối soát đủ.");
                if (ownContract.deposit_transactions.Any(x => x.status == "pending" && x.transaction_type == "collected")) throw new InvalidOperationException("Đã có một yêu cầu nộp cọc đang chờ đối chiếu.");
                var request = new deposit_transaction { contract_id = ownContract.id, transaction_code = "DC-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), transaction_type = "collected", amount = remaining, method = "bank_transfer", status = "pending", transaction_date = DateTime.Now, reason = "Yêu cầu nộp tiền cọc; chờ đối chiếu giao dịch thực nhận." };
                db.deposit_transactions.Add(request);
                await db.SaveChangesAsync();
                Audit("deposit-request", section, request.id, "pending deposit collection");
                await db.SaveChangesAsync();
                return Done("Đã tạo yêu cầu nộp cọc. Hợp đồng chỉ được kích hoạt sau khi kế toán xác nhận.", section);
            }
            if (section == "guests")
            {
                if (!IsStudent) return Forbid();
                var arrival = DateTime.Parse(Text(form, "arrival"), CultureInfo.InvariantCulture);
                var departure = string.IsNullOrWhiteSpace(form["departure"]) ? (DateTime?)null : DateTime.Parse(Text(form, "departure"), CultureInfo.InvariantCulture);
                if (arrival < DateTime.Now.AddMinutes(-5) || departure is not null && departure <= arrival) throw new InvalidOperationException("Thời gian khách đến cần ở tương lai; thời gian rời phải sau lúc đến.");
                var studentId = await StudentId();
                if (!await db.contracts.AnyAsync(x => x.student_id == studentId && x.status == "active" && x.start_date <= DateOnly.FromDateTime(arrival) && x.end_date >= DateOnly.FromDateTime(arrival))) throw new InvalidOperationException("Bạn cần có hợp đồng đang lưu trú vào ngày khách đến.");
                var guest = new guest_visit { student_id = studentId, guest_name = Text(form, "guestName"), guest_phone = string.IsNullOrWhiteSpace(form["guestPhone"]) ? null : Text(form, "guestPhone"), visit_purpose = string.IsNullOrWhiteSpace(form["purpose"]) ? null : Text(form, "purpose"), expected_arrival = arrival, expected_departure = departure, status = "pending", created_at = DateTime.Now };
                db.guest_visits.Add(guest);
                await db.SaveChangesAsync();
                Audit("guest-request", section, guest.id, "guest visit submitted");
                await db.SaveChangesAsync();
                return Done("Đã gửi đăng ký khách thăm.", section);
            }
            if (section == "refunds")
            {
                if (!IsStudent) return Forbid();
                var paymentId = Long(form, "paymentId");
                var paid = await db.payments.Include(x => x.payment_refunds).Include(x => x.invoice).SingleAsync(x => x.id == paymentId && x.status == "completed" && x.invoice.contract.student.user_id == UserId);
                var amount = Decimal(form, "amount");
                var alreadyRequested = paid.payment_refunds.Where(x => x.status is "pending" or "completed").Sum(x => x.amount);
                if (amount <= 0 || alreadyRequested + amount > paid.amount) throw new InvalidOperationException("Số tiền yêu cầu vượt khoản thanh toán còn có thể hoàn.");
                var refund = new payment_refund { payment_id = paid.id, refund_code = "HT-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), amount = amount, reason = Text(form, "reason"), method = "bank_transfer", status = "pending", created_at = DateTime.Now };
                db.payment_refunds.Add(refund);
                await db.SaveChangesAsync();
                Audit("refund-request", section, refund.id, refund.refund_code);
                await NotifyRole("accountant", "Có yêu cầu hoàn tiền", $"{refund.refund_code} · {refund.amount:N0} ₫ đang chờ xem xét.", "billing", "payment_refund", refund.id);
                await db.SaveChangesAsync();
                return Done("Đã gửi yêu cầu hoàn tiền để kế toán xem xét.", section);
            }
            if (section == "services" && IsStudent)
            {
                var targetContract = await ActiveStudentContract();
                var serviceId = Long(form, "serviceId");
                var selected = await db.services.SingleAsync(x => x.id == serviceId && x.is_active);
                if (await db.service_subscriptions.AnyAsync(x => x.contract_id == targetContract.id && x.service_id == serviceId && x.status == "active")) throw new InvalidOperationException("Dịch vụ này đã được đăng ký.");
                db.service_subscriptions.Add(new service_subscription { contract_id = targetContract.id, service_id = serviceId, start_date = DateOnly.FromDateTime(DateTime.Today), quantity = 1, agreed_unit_price = selected.unit_price, status = "active", note = "Đăng ký từ cổng sinh viên" });
                await db.SaveChangesAsync();
                Audit("subscribe", section, selected.id, selected.code);
                await db.SaveChangesAsync();
                return Done("Đã đăng ký dịch vụ theo hợp đồng.", section);
            }
            if (IsStudent && section is not ("services" or "repairs")) return Forbid();
            if (!IsAdmin && !IsStudent && !(IsAccountant && section is ("payments" or "deposits" or "refunds")) && !(IsMaintenance && section is ("repairs" or "meters" or "utilities"))) return Forbid();
            if (section is ("users" or "buildings" or "rooms" or "rules" or "services" or "assets" or "violations" or "notifications") && !IsAdmin) return Forbid();
            switch (section)
            {
                case "buildings":
                    db.buildings.Add(new building { code = Text(form, "code"), name = Text(form, "name"), address = form["address"], description = form["description"], is_active = true, created_at = DateTime.Now });
                    break;
                case "rooms":
                    var capacity = Short(form, "capacity");
                    if (capacity is < 1 or > 16) throw new InvalidOperationException("Sức chứa cần từ 1 đến 16 giường.");
                    var room = new room { building_id = Long(form, "buildingId"), room_number = Text(form, "roomNumber"), floor_number = Short(form, "floor"), room_type = Text(form, "roomType"), capacity = capacity, monthly_price = Decimal(form, "price"), status = "available", created_at = DateTime.Now, updated_at = DateTime.Now };
                    db.rooms.Add(room);
                    await db.SaveChangesAsync();
                    for (var i = 1; i <= capacity; i++) db.beds.Add(new bed { room_id = room.id, bed_number = $"G{i:00}", status = "available" });
                    break;
                case "services":
                    db.services.Add(new service { code = Text(form, "code"), name = Text(form, "name"), unit = Text(form, "unit"), unit_price = Decimal(form, "price"), is_metered = form["metered"] == "true", is_active = true, description = form["description"] });
                    break;
                case "rules":
                    db.dormitory_rules.Add(new dormitory_rule { title = Text(form, "title"), version = Text(form, "version"), effective_from = DateOnly.Parse(form["effectiveFrom"]!), content = Text(form, "content"), is_active = true, created_by_user_id = UserId, created_at = DateTime.Now });
                    break;
                case "payments":
                    var contractId = Long(form, "contractId");
                    var periodStart = DateOnly.Parse(form["periodStart"]!);
                    var periodEnd = DateOnly.Parse(form["periodEnd"]!);
                    if (periodEnd <= periodStart) throw new InvalidOperationException("Kỳ đến phải sau kỳ bắt đầu.");
                    var contract = await db.contracts.SingleAsync(x => x.id == contractId && x.status == "active");
                    if (periodEnd <= periodStart) throw new InvalidOperationException("Kỳ đến phải sau kỳ bắt đầu.");
                    var invoice = new invoice { invoice_code = "HD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(), contract_id = contractId, period_start = periodStart, period_end = periodEnd, issued_at = DateTime.Now, due_date = DateOnly.Parse(form["dueDate"]!), status = "issued", note = "Tiền lưu trú và dịch vụ" };
                    db.invoices.Add(invoice);
                    await db.SaveChangesAsync();
                    var months = Math.Max(1, (periodEnd.Year - periodStart.Year) * 12 + periodEnd.Month - periodStart.Month);
                    db.invoice_items.Add(new invoice_item { invoice_id = invoice.id, item_type = "room_rent", description = "Phí lưu trú", quantity = months, unit_price = contract.monthly_price, amount = months * contract.monthly_price });
                    var unbilledUsages = await db.service_usages.Where(x => x.contract_id == contractId && x.period_start >= periodStart && x.period_end <= periodEnd && !db.invoice_items.Any(i => i.service_usage_id == x.id)).Include(x => x.service).ToListAsync();
                    foreach (var usage in unbilledUsages) db.invoice_items.Add(new invoice_item { invoice_id = invoice.id, item_type = "service", service_usage_id = usage.id, description = usage.service.name, quantity = usage.quantity, unit_price = usage.unit_price, amount = usage.quantity * usage.unit_price });
                    break;
                case "users":
                    var newUser = new user { username = Text(form, "username"), full_name = Text(form, "fullName"), email = string.IsNullOrWhiteSpace(form["email"]) ? null : Text(form, "email"), is_active = true, created_at = DateTime.Now, updated_at = DateTime.Now };
                    var password = Text(form, "password");
                    if (password.Length < 12 || !password.Any(char.IsUpper) || !password.Any(char.IsDigit)) throw new InvalidOperationException("Mật khẩu cần ít nhất 12 ký tự, gồm chữ in hoa và chữ số.");
                    newUser.password_hash = hasher.HashPassword(newUser, password);
                    var roleId = Long(form, "roleId");
                    var assignedRole = await db.roles.SingleAsync(x => x.id == roleId);
                    await using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        db.users.Add(newUser);
                        await db.SaveChangesAsync();
                        db.user_roles.Add(new user_role { user_id = newUser.id, role_id = roleId, assigned_at = DateTime.Now });
                        if (assignedRole.code == "student") db.students.Add(new student { user_id = newUser.id, student_code = "SV-" + DateTime.Now.Year + "-" + newUser.id.ToString("D5"), gender = "unspecified", created_at = DateTime.Now });
                        Audit("create", section, newUser.id, "user account created");
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    break;
                case "repairs":
                    var activeContract = await ActiveStudentContract();
                    var selectedRoom = await db.beds.Where(x => x.id == activeContract.bed_id).Select(x => x.room_id).SingleAsync();
                    var student = await db.students.SingleAsync(x => x.id == activeContract.student_id);
                    db.repair_requests.Add(new repair_request { request_code = "SC-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), student_id = student.id, room_id = selectedRoom, reported_by_user_id = UserId, title = Text(form, "title"), description = Text(form, "description"), priority = form["priority"] == "urgent" ? "urgent" : form["priority"] == "high" ? "high" : "normal", status = "submitted", reported_at = DateTime.Now });
                    break;
                case "assets":
                    var quantity = Short(form, "quantity");
                    if (quantity < 1) throw new InvalidOperationException("Số lượng tài sản cần từ 1 trở lên.");
                    db.room_assets.Add(new room_asset { room_id = Long(form, "roomId"), asset_type_id = Long(form, "assetTypeId"), asset_tag = string.IsNullOrWhiteSpace(form["assetTag"]) ? null : Text(form, "assetTag"), name = Text(form, "name"), quantity = quantity, condition_status = Text(form, "condition"), acquired_at = string.IsNullOrWhiteSpace(form["acquiredAt"]) ? null : DateOnly.Parse(Text(form, "acquiredAt")), last_checked_at = DateTime.Now, note = null });
                    break;
                case "meters":
                    var roomId = Long(form, "roomId");
                    var serviceId = Long(form, "serviceId");
                    if (!await db.services.AnyAsync(x => x.id == serviceId && x.is_active && x.is_metered)) throw new InvalidOperationException("Đồng hồ chỉ gắn với dịch vụ tính theo chỉ số đang hoạt động.");
                    db.utility_meters.Add(new utility_meter { room_id = roomId, service_id = serviceId, meter_code = Text(form, "meterCode"), installed_at = string.IsNullOrWhiteSpace(form["installedAt"]) ? null : DateOnly.Parse(Text(form, "installedAt")), is_active = true });
                    break;
                case "utilities":
                    var meterId = Long(form, "meterId");
                    var meter = await db.utility_meters.Include(x => x.service).Include(x => x.utility_readings).SingleAsync(x => x.id == meterId && x.is_active);
                    var readingDate = DateOnly.Parse(Text(form, "readingDate"));
                    var previousReading = meter.utility_readings.OrderByDescending(x => x.reading_date).FirstOrDefault();
                    if (previousReading is not null && readingDate <= previousReading.reading_date) throw new InvalidOperationException("Ngày ghi chỉ số phải sau lần ghi gần nhất.");
                    var currentValue = Decimal(form, "currentValue");
                    var previousValue = previousReading?.current_value ?? 0m;
                    if (currentValue < previousValue) throw new InvalidOperationException("Chỉ số mới không được nhỏ hơn chỉ số trước.");
                    var consumption = currentValue - previousValue;
                    var contractsInRoom = await db.contracts.Include(x => x.bed).Where(x => x.bed.room_id == meter.room_id && x.status == "active" && x.start_date <= readingDate && x.end_date >= readingDate).ToListAsync();
                    await using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        var reading = new utility_reading { meter_id = meter.id, reading_date = readingDate, previous_value = previousValue, current_value = currentValue, consumption = consumption, unit_price = meter.service.unit_price, total_amount = consumption * meter.service.unit_price, recorded_by_user_id = UserId, note = string.IsNullOrWhiteSpace(form["note"]) ? null : Text(form, "note"), created_at = DateTime.Now };
                        db.utility_readings.Add(reading);
                        var usagePeriodStart = previousReading?.reading_date ?? meter.installed_at ?? readingDate.AddDays(-1);
                        if (usagePeriodStart >= readingDate) usagePeriodStart = readingDate.AddDays(-1);
                        for (var i = 0; i < contractsInRoom.Count; i++)
                        {
                            var contractInRoom = contractsInRoom[i];
                            var share = i == contractsInRoom.Count - 1 ? consumption - Math.Round(consumption / contractsInRoom.Count, 3) * (contractsInRoom.Count - 1) : Math.Round(consumption / contractsInRoom.Count, 3);
                            var usage = new service_usage { service_id = meter.service_id, contract_id = contractInRoom.id, period_start = usagePeriodStart, period_end = readingDate, quantity = share, unit_price = meter.service.unit_price, recorded_by_user_id = UserId, note = $"Đồng hồ {meter.meter_code} · chỉ số phòng {meter.room_id}" };
                            db.service_usages.Add(usage);
                            if (i == 0) reading.service_usage = usage;
                        }
                        Audit("utility-reading", section, reading.id, $"meter:{meter.meter_code}; consumption:{consumption}");
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    return Done("Đã ghi chỉ số và tạo lượt sử dụng chờ lập hóa đơn.", section);
                case "violations":
                    var targetStudentId = Long(form, "studentId");
                    var fine = Decimal(form, "fineAmount");
                    if (fine < 0) throw new InvalidOperationException("Mức phạt không được âm.");
                    var violation = new student_violation { student_id = targetStudentId, contract_id = await db.contracts.Where(x => x.student_id == targetStudentId && x.status == "active").Select(x => (long?)x.id).FirstOrDefaultAsync(), rule_id = NullableLong(form, "ruleId"), recorded_by_user_id = UserId, violation_code = "VP-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), title = Text(form, "title"), description = string.IsNullOrWhiteSpace(form["description"]) ? null : Text(form, "description"), occurred_at = DateTime.Parse(Text(form, "occurredAt"), CultureInfo.InvariantCulture), fine_amount = fine, status = "recorded", created_at = DateTime.Now };
                    db.student_violations.Add(violation);
                    break;
                case "notifications":
                    if (!IsAdmin) return Forbid();
                    var recipientId = Long(form, "userId");
                    if (!await db.users.AnyAsync(x => x.id == recipientId && x.is_active)) throw new InvalidOperationException("Tài khoản nhận không còn hoạt động.");
                    db.notifications.Add(new notification { user_id = recipientId, title = Text(form, "title"), message = Text(form, "message"), notification_type = Text(form, "notificationType"), entity_type = "announcement", created_at = DateTime.Now });
                    break;
                default:
                    throw new InvalidOperationException("Mục này chưa hỗ trợ tạo bản ghi.");
            }
            if (section != "users") Audit("create", section, null, "record created");
            await db.SaveChangesAsync();
            return Done("Đã lưu thay đổi.", section);
        }
        catch (Exception ex) when (ex is InvalidOperationException or DbUpdateException or FormatException or OverflowException)
        {
            TempData["Error"] = ex is DbUpdateException ? "Dữ liệu bị trùng hoặc chưa đáp ứng ràng buộc. Hãy kiểm tra các trường và thử lại." : ex.Message;
            return RedirectToAction(nameof(Workspace), new { section });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(string section, long id, string operation)
    {
        section = section.ToLowerInvariant();
        if (operation is ("acknowledge" or "subscribe" or "unsubscribe" or "payment-request" or "guest-cancel") && !IsStudent) return Forbid();
        if (operation == "user-toggle" && !IsSystemAdmin) return Forbid();
        if (operation == "open-deposits") return RedirectToAction(nameof(Workspace), new { section = "deposits" });
        if (IsStudent && operation is ("approve" or "reject" or "sign" or "cancel-contract" or "checkin" or "checkout" or "room-maintenance" or "room-open" or "payment-confirm" or "deposit-confirm" or "user-toggle" or "repair-accept" or "repair-progress" or "repair-complete" or "repair-reject" or "guest-approve" or "guest-reject" or "guest-arrive" or "guest-depart" or "asset-check" or "meter-toggle" or "violation-review" or "violation-resolve" or "violation-invoice" or "refund-complete" or "refund-reject")) return Forbid();
        if (operation.StartsWith("guest-") && operation != "guest-cancel" && !IsAdmin) return Forbid();
        if (operation == "asset-check" && !(IsAdmin || IsMaintenance)) return Forbid();
        if (operation == "meter-toggle" && !IsAdmin) return Forbid();
        if (operation.StartsWith("violation-") && !(IsAdmin || IsAccountant)) return Forbid();
        if (operation.StartsWith("refund-") && !(IsAdmin || IsAccountant)) return Forbid();
        if (operation == "notification-read" && IsStudent == false && !IsAdmin && !IsAccountant && !IsMaintenance) return Forbid();
        try
        {
            if (operation is "approve" or "reject" or "sign" or "cancel-contract" or "checkin" or "checkout" or "room-maintenance" or "room-open" or "user-toggle" or "repair-accept" or "repair-progress" or "repair-complete" or "repair-reject")
            {
                if (operation.StartsWith("repair-") ? !(IsAdmin || IsMaintenance) : !IsAdmin) return Forbid();
            }
            if (operation == "payment-confirm" && !(IsAdmin || IsAccountant)) return Forbid();
            if (operation == "deposit-confirm" && !(IsAdmin || IsAccountant)) return Forbid();
            switch (operation)
            {
                case "approve":
                    var registration = await db.room_registrations.Include(x => x.student).SingleAsync(x => x.id == id && x.status == "pending");
                    var freeBeds = db.beds.Include(x => x.room).Where(x => x.status == "available" && x.room.status == "available");
                    if (registration.preferred_building_id is not null) freeBeds = freeBeds.Where(x => x.room.building_id == registration.preferred_building_id);
                    if (!string.IsNullOrWhiteSpace(registration.preferred_room_type)) freeBeds = freeBeds.Where(x => x.room.room_type == registration.preferred_room_type);
                    var freeBed = await freeBeds.OrderBy(x => x.room.building.code).ThenBy(x => x.room.room_number).FirstOrDefaultAsync();
                    if (freeBed is null) throw new InvalidOperationException("Không còn giường phù hợp. Vui lòng điều chỉnh nguyện vọng trước khi duyệt.");
                    await using (var tx = await db.Database.BeginTransactionAsync())
                    {
                        freeBed.status = "occupied";
                        registration.status = "contracted";
                        var newContract = new contract { contract_code = "HD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(), registration_id = registration.id, student_id = registration.student_id, bed_id = freeBed.id, created_by_user_id = UserId, start_date = registration.requested_start_date, end_date = registration.requested_end_date, monthly_price = freeBed.room.monthly_price, deposit_amount = freeBed.room.monthly_price, status = "draft", terms = "Hợp đồng chờ xác nhận ký. Tiền đặt cọc được theo dõi bằng giao dịch riêng." };
                        db.contracts.Add(newContract);
                        db.registration_reviews.Add(new registration_review { registration_id = registration.id, reviewer_user_id = UserId, decision = "approved", comment = $"Phân giường {freeBed.room.room_number} · {freeBed.bed_number}; hợp đồng được tạo.", reviewed_at = DateTime.Now });
                        Notify(registration.student.user_id, "Đăng ký phòng đã được duyệt", $"Hồ sơ {registration.registration_code} được phân {freeBed.room.building.code} · {freeBed.room.room_number} · {freeBed.bed_number}. Hợp đồng đang chờ ký và đối chiếu cọc.", "operation", "room_registration", registration.id);
                        Audit("approve", "room_registration", registration.id, "bed assigned and draft contract created");
                        await db.SaveChangesAsync();
                        await tx.CommitAsync();
                    }
                    TempData["Notice"] = "Đã duyệt, giữ giường và tạo hợp đồng nháp. Xác nhận ký kết thực tế trước khi kích hoạt.";
                    return RedirectToAction(nameof(Workspace), new { section = "contracts" });
                case "reject":
                    var rejected = await db.room_registrations.Include(x => x.student).SingleAsync(x => x.id == id && x.status == "pending");
                    rejected.status = "rejected";
                    db.registration_reviews.Add(new registration_review { registration_id = id, reviewer_user_id = UserId, decision = "rejected", comment = "Không đáp ứng điều kiện phân phòng.", reviewed_at = DateTime.Now });
                    Notify(rejected.student.user_id, "Đăng ký phòng chưa được duyệt", $"Hồ sơ {rejected.registration_code} cần trao đổi thêm với ban quản lý.", "operation", "room_registration", id);
                    break;
                case "registration-cancel":
                    if (!IsStudent) return Forbid();
                    var ownRegistration = await db.room_registrations.SingleAsync(x => x.id == id && x.student.user_id == UserId && x.status == "pending");
                    ownRegistration.status = "cancelled";
                    break;
                case "checkin":
                    var checkin = await db.contracts.Include(x => x.occupancy_record).SingleAsync(x => x.id == id && x.status == "active");
                    if (DateOnly.FromDateTime(DateTime.Today) < checkin.start_date) throw new InvalidOperationException("Chưa đến ngày bắt đầu hợp đồng để nhận phòng.");
                    if (DateOnly.FromDateTime(DateTime.Today) >= checkin.end_date) throw new InvalidOperationException("Hợp đồng đã hết thời hạn nhận phòng.");
                    if (checkin.occupancy_record is null) db.occupancy_records.Add(new occupancy_record { contract_id = id, checked_in_at = DateTime.Now, checked_in_by_user_id = UserId, checkin_condition = "Chưa ghi nhận hư hỏng", expected_checkout_date = checkin.end_date, created_at = DateTime.Now });
                    break;
                case "sign":
                    var signedContract = await db.contracts.Include(x => x.deposit_transactions).SingleAsync(x => x.id == id && x.status == "draft");
                    var depositNet = signedContract.deposit_transactions.Where(x => x.status == "completed").Sum(x => x.transaction_type == "collected" ? x.amount : -x.amount);
                    if (depositNet < signedContract.deposit_amount) throw new InvalidOperationException("Chưa đủ tiền cọc đã được kế toán xác nhận để kích hoạt hợp đồng.");
                    signedContract.status = "active";
                    signedContract.signed_at = DateTime.Now;
                    signedContract.terms = "Đã ghi nhận xác nhận hợp đồng trên hệ thống.";
                    break;
                case "cancel-contract":
                    var cancelledContract = await db.contracts.Include(x => x.bed).SingleAsync(x => x.id == id && x.status == "draft");
                    cancelledContract.status = "cancelled";
                    cancelledContract.bed.status = "available";
                    break;
                case "checkout":
                    var checkout = await db.contracts.Include(x => x.occupancy_record).Include(x => x.bed).SingleAsync(x => x.id == id && x.status == "active");
                    if (checkout.occupancy_record is not null) { checkout.occupancy_record.checked_out_at = DateTime.Now; checkout.occupancy_record.checked_out_by_user_id = UserId; checkout.occupancy_record.checkout_condition = "Đã ghi nhận khi trả phòng"; }
                    checkout.status = "expired";
                    checkout.bed.status = "available";
                    break;
                case "room-maintenance":
                    (await db.rooms.SingleAsync(x => x.id == id)).status = "maintenance";
                    break;
                case "room-open":
                    (await db.rooms.SingleAsync(x => x.id == id)).status = "available";
                    break;
                case "repair-accept": case "repair-progress": case "repair-complete": case "repair-reject":
                    var repair = await db.repair_requests.Include(x => x.student).SingleAsync(x => x.id == id);
                    var next = operation switch { "repair-accept" => "accepted", "repair-progress" => "in_progress", "repair-complete" => "completed", _ => "rejected" };
                    db.repair_request_updates.Add(new repair_request_update { repair_request_id = id, updated_by_user_id = UserId, old_status = repair.status, new_status = next, comment = "Cập nhật từ màn hình quản lý.", created_at = DateTime.Now });
                    repair.status = next;
                    if (next == "completed") { repair.completed_at = DateTime.Now; repair.resolution_note = "Đã hoàn tất xử lý."; }
                    if (repair.student is not null) Notify(repair.student.user_id, "Yêu cầu sửa chữa được cập nhật", $"{repair.request_code} · {repair.title} hiện ở trạng thái {StatusText(next)}.", "operation", "repair_request", repair.id);
                    break;
                case "acknowledge":
                    var ownStudent = await StudentId();
                    if (!await db.rule_acknowledgements.AnyAsync(x => x.rule_id == id && x.student_id == ownStudent)) db.rule_acknowledgements.Add(new rule_acknowledgement { rule_id = id, student_id = ownStudent, acknowledged_at = DateTime.Now });
                    break;
                case "subscribe":
                    var target = await ActiveStudentContract();
                    var service = await db.services.SingleAsync(x => x.id == id && x.is_active);
                    if (!await db.service_subscriptions.AnyAsync(x => x.contract_id == target.id && x.service_id == id && x.status == "active")) db.service_subscriptions.Add(new service_subscription { contract_id = target.id, service_id = id, start_date = DateOnly.FromDateTime(DateTime.Today), quantity = 1, agreed_unit_price = service.unit_price, status = "active" });
                    break;
                case "unsubscribe":
                    var sub = await db.service_subscriptions.Include(x => x.contract).SingleAsync(x => x.id == id);
                    if (IsStudent && sub.contract.student_id != await StudentId()) return Forbid();
                    sub.status = "ended"; sub.end_date = DateOnly.FromDateTime(DateTime.Today);
                    break;
                case "payment-request":
                    var invoice = await db.invoices.Include(x => x.contract).SingleAsync(x => x.id == id);
                    if (IsStudent && invoice.contract.student_id != await StudentId()) return Forbid();
                    var balance = await db.v_invoice_balances.Where(x => x.invoice_id == id).Select(x => x.balance_amount ?? 0m).SingleAsync();
                    if (balance <= 0) throw new InvalidOperationException("Hóa đơn đã được thanh toán đủ.");
                    db.payments.Add(new payment { payment_code = "TT-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant(), invoice_id = id, amount = balance, method = "bank_transfer", status = "pending", transaction_reference = null, paid_at = DateTime.Now, note = "Yêu cầu chờ kế toán xác nhận giao dịch." });
                    break;
                case "payment-confirm":
                    var pendingPayment = await db.payments.Include(x => x.invoice).ThenInclude(x => x.contract).ThenInclude(x => x.student).SingleAsync(x => x.invoice_id == id && x.status == "pending");
                    pendingPayment.status = "completed"; pendingPayment.received_by_user_id = UserId; pendingPayment.paid_at = DateTime.Now; pendingPayment.transaction_reference = "COUNTER-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                    Notify(pendingPayment.invoice.contract.student.user_id, "Khoản thanh toán đã được đối soát", $"{pendingPayment.invoice.invoice_code} · {pendingPayment.amount:N0} ₫ đã được ghi nhận.", "billing", "invoice", id);
                    break;
                case "deposit-confirm":
                    var pendingDeposit = await db.deposit_transactions.Include(x => x.contract).ThenInclude(x => x.student).SingleAsync(x => x.id == id && x.status == "pending" && x.transaction_type == "collected");
                    pendingDeposit.status = "completed";
                    pendingDeposit.handled_by_user_id = UserId;
                    pendingDeposit.reason = "Đã đối chiếu với giao dịch thực nhận.";
                    Notify(pendingDeposit.contract.student.user_id, "Tiền đặt cọc đã được đối chiếu", $"{pendingDeposit.transaction_code} · {pendingDeposit.amount:N0} ₫.", "billing", "contract", pendingDeposit.contract_id);
                    break;
                case "user-toggle":
                    var targetUser = await db.users.SingleAsync(x => x.id == id && x.id != UserId);
                    targetUser.is_active = !targetUser.is_active; targetUser.updated_at = DateTime.Now;
                    break;
                case "guest-cancel":
                    var ownGuest = await db.guest_visits.Include(x => x.student).ThenInclude(x => x.user).SingleAsync(x => x.id == id && x.status == "pending");
                    if (ownGuest.student.user_id != UserId) return Forbid();
                    ownGuest.status = "cancelled";
                    break;
                case "guest-approve": case "guest-reject":
                    var guestReview = await db.guest_visits.Include(x => x.student).SingleAsync(x => x.id == id && x.status == "pending");
                    guestReview.status = operation == "guest-approve" ? "approved" : "rejected";
                    guestReview.approved_by_user_id = UserId;
                    Notify(guestReview.student.user_id, operation == "guest-approve" ? "Đăng ký khách đã được duyệt" : "Đăng ký khách chưa được duyệt", $"Yêu cầu khách {guestReview.guest_name} ngày {guestReview.expected_arrival:dd/MM/yyyy HH:mm} đã được xử lý.", "operation", "guest_visit", guestReview.id);
                    break;
                case "guest-arrive":
                    var arrivingGuest = await db.guest_visits.SingleAsync(x => x.id == id && x.status == "approved");
                    arrivingGuest.actual_arrival = DateTime.Now;
                    arrivingGuest.status = "checked_in";
                    break;
                case "guest-depart":
                    var departingGuest = await db.guest_visits.SingleAsync(x => x.id == id && x.status == "checked_in");
                    departingGuest.actual_departure = DateTime.Now;
                    departingGuest.status = "completed";
                    break;
                case "asset-check":
                    var checkedAsset = await db.room_assets.SingleAsync(x => x.id == id);
                    checkedAsset.last_checked_at = DateTime.Now;
                    break;
                case "meter-toggle":
                    var targetMeter = await db.utility_meters.SingleAsync(x => x.id == id);
                    targetMeter.is_active = !targetMeter.is_active;
                    break;
                case "violation-review":
                    var reviewedViolation = await db.student_violations.SingleAsync(x => x.id == id && x.status == "recorded");
                    reviewedViolation.status = "under_review";
                    break;
                case "violation-resolve":
                    var resolvedViolation = await db.student_violations.Include(x => x.student).SingleAsync(x => x.id == id && (x.status == "recorded" || x.status == "under_review" || x.status == "appealed"));
                    resolvedViolation.status = "resolved";
                    resolvedViolation.resolved_at = DateTime.Now;
                    resolvedViolation.resolution_note = "Đã xử lý bởi ban quản lý.";
                    Notify(resolvedViolation.student.user_id, "Biên bản đã được xử lý", $"Biên bản {resolvedViolation.violation_code} · {resolvedViolation.title} đã được đóng.", "operation", "student_violation", resolvedViolation.id);
                    break;
                case "violation-invoice":
                    var fineViolation = await db.student_violations.Include(x => x.contract).Include(x => x.violation_invoice_item).SingleAsync(x => x.id == id && x.fine_amount > 0);
                    if (fineViolation.violation_invoice_item is not null) throw new InvalidOperationException("Khoản phạt đã được đưa vào hóa đơn.");
                    var fineContract = fineViolation.contract ?? await db.contracts.SingleOrDefaultAsync(x => x.student_id == fineViolation.student_id && x.status == "active") ?? throw new InvalidOperationException("Sinh viên chưa có hợp đồng hoạt động để gắn hóa đơn.");
                    var finePeriodStart = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
                    var finePeriodEnd = finePeriodStart.AddMonths(1);
                    var fineInvoice = await db.invoices.SingleOrDefaultAsync(x => x.contract_id == fineContract.id && x.period_start == finePeriodStart && x.period_end == finePeriodEnd);
                    if (fineInvoice is null)
                    {
                        fineInvoice = new invoice { invoice_code = "HD-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant(), contract_id = fineContract.id, period_start = finePeriodStart, period_end = finePeriodEnd, issued_at = DateTime.Now, due_date = finePeriodEnd.AddDays(10), status = "issued", note = "Phí lưu trú và khoản phát sinh" };
                        db.invoices.Add(fineInvoice);
                        await db.SaveChangesAsync();
                    }
                    if (fineInvoice.status is "paid" or "void" or "overdue") throw new InvalidOperationException("Hóa đơn hiện tại đã khóa; lập một hóa đơn điều chỉnh riêng.");
                    var fineItem = new invoice_item { invoice_id = fineInvoice.id, item_type = "fine", description = $"Khoản phạt {fineViolation.violation_code} · {fineViolation.title}", quantity = 1, unit_price = fineViolation.fine_amount, amount = fineViolation.fine_amount };
                    db.invoice_items.Add(fineItem);
                    await db.SaveChangesAsync();
                    db.violation_invoice_items.Add(new violation_invoice_item { violation_id = id, invoice_item_id = fineItem.id, linked_at = DateTime.Now });
                    break;
                case "refund-complete": case "refund-reject":
                    var targetRefund = await db.payment_refunds.SingleAsync(x => x.id == id && x.status == "pending");
                    targetRefund.status = operation == "refund-complete" ? "completed" : "rejected";
                    targetRefund.processed_by_user_id = UserId;
                    targetRefund.processed_at = DateTime.Now;
                    if (operation == "refund-complete")
                    {
                        var originalPayment = await db.payments.SingleAsync(x => x.id == targetRefund.payment_id);
                        var refundedTotal = await db.payment_refunds.Where(x => x.payment_id == originalPayment.id && (x.status == "completed" || x.id == targetRefund.id)).SumAsync(x => x.amount);
                        if (refundedTotal >= originalPayment.amount) originalPayment.status = "refunded";
                        Notify(await db.invoices.Where(x => x.id == originalPayment.invoice_id).Select(x => x.contract.student.user_id).SingleAsync(), "Yêu cầu hoàn tiền đã được ghi nhận", $"Yêu cầu {targetRefund.refund_code} đã được kế toán đánh dấu hoàn tất.", "billing", "payment_refund", targetRefund.id);
                    }
                    break;
                case "notification-read":
                    var notice = await db.notifications.SingleAsync(x => x.id == id && x.user_id == UserId);
                    notice.read_at ??= DateTime.Now;
                    break;
                default: throw new InvalidOperationException("Thao tác không hợp lệ.");
            }
            Audit(operation, section, id, operation);
            await db.SaveChangesAsync();
            return Done("Đã cập nhật trạng thái.", section);
        }
        catch (Exception ex) when (ex is InvalidOperationException or DbUpdateException)
        {
            TempData["Error"] = ex is DbUpdateException ? "Không thể lưu do dữ liệu liên quan hoặc ràng buộc đang được sử dụng." : ex.Message;
            return RedirectToAction(nameof(Workspace), new { section });
        }
    }

    private async Task<long> StudentId() => await db.students.Where(x => x.user_id == UserId).Select(x => x.id).SingleAsync();
    private async Task<contract> ActiveStudentContract()
    {
        var studentId = await StudentId();
        return await db.contracts.Include(x => x.bed).ThenInclude(x => x.room).FirstOrDefaultAsync(x => x.student_id == studentId && x.status == "active") ?? throw new InvalidOperationException("Bạn chưa có hợp đồng lưu trú đang hoạt động.");
    }
    private IActionResult Done(string message, string section) { TempData["Notice"] = message; return RedirectToAction(nameof(Workspace), new { section }); }
    private void Audit(string action, string entityType, long? entityId, string? value)
    {
        db.audit_logs.Add(new audit_log { user_id = UserId, action = action, entity_type = entityType, entity_id = entityId, new_values = value, ip_address = HttpContext.Connection.RemoteIpAddress?.ToString(), user_agent = Request.Headers.UserAgent.ToString(), created_at = DateTime.Now });
    }
    private void Notify(long userId, string title, string message, string kind, string? entityType = null, long? entityId = null)
    {
        db.notifications.Add(new notification { user_id = userId, title = title, message = message, notification_type = kind, entity_type = entityType, entity_id = entityId, created_at = DateTime.Now });
    }
    private async Task NotifyRole(string roleCode, string title, string message, string kind, string? entityType = null, long? entityId = null)
    {
        var recipients = await db.user_roles.Where(x => x.role.code == roleCode && x.user.is_active).Select(x => x.user_id).Distinct().ToListAsync();
        foreach (var recipient in recipients) Notify(recipient, title, message, kind, entityType, entityId);
    }
    private static string Text(IFormCollection form, string key) => form[key].ToString().Trim();
    private static long Long(IFormCollection form, string key) => long.Parse(form[key]!, CultureInfo.InvariantCulture);
    private static long? NullableLong(IFormCollection form, string key) => long.TryParse(form[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : null;
    private static short Short(IFormCollection form, string key) => short.Parse(form[key]!, CultureInfo.InvariantCulture);
    private static decimal Decimal(IFormCollection form, string key) => decimal.Parse(form[key]!, CultureInfo.InvariantCulture);
    private static ModuleViewModel Build(string section, string title, string description, List<string> columns, List<ModuleRowViewModel> rows, string? formTitle = null, string? formHelp = null, List<FormFieldViewModel>? fields = null) => new() { Section = section, Title = title, Description = description, CountLabel = $"{rows.Count} mục", Columns = columns, Rows = rows, FormTitle = formTitle, FormHelp = formHelp, Fields = fields ?? [] };
    private static ModuleRowViewModel Row(long id, List<string> cells, string state, List<RowActionViewModel>? actions = null) => new() { Id = id, Cells = cells, State = state, Actions = actions ?? [] };
    public static string StatusText(string? value) => value?.ToLowerInvariant() switch
    {
        "pending" or "submitted" => "Chờ xử lý", "approved" => "Đã duyệt", "rejected" => "Từ chối", "cancelled" => "Đã hủy", "contracted" => "Đã lập hợp đồng", "active" => "Đang hiệu lực", "expired" => "Hết hạn", "terminated" => "Đã kết thúc", "draft" => "Bản nháp", "available" => "Còn chỗ", "occupied" => "Đang sử dụng", "full" => "Đã đủ", "maintenance" => "Đang bảo trì", "inactive" => "Ngưng hoạt động", "accepted" => "Đã tiếp nhận", "in_progress" => "Đang xử lý", "completed" => "Hoàn tất", "checked_in" => "Đang có khách", "recorded" => "Đã lập biên bản", "under_review" => "Đang xem xét", "resolved" => "Đã xử lý", "appealed" => "Đang khiếu nại", "new" => "Mới", "good" => "Tốt", "worn" => "Hao mòn", "damaged" => "Hư hỏng", "disposed" => "Đã thanh lý", "paid" => "Đã thanh toán", "partially_paid" => "Thanh toán một phần", "issued" => "Đã phát hành", "overdue" => "Quá hạn", "urgent" => "Khẩn cấp", "high" => "Cao", "normal" => "Thông thường", "low" => "Thấp", "paused" => "Tạm dừng", "ended" => "Đã kết thúc", "failed" => "Thất bại", "refunded" => "Đã hoàn tiền", "collected" => "Thu cọc", "deducted" => "Khấu trừ", "cash" => "Tiền mặt", "bank_transfer" => "Chuyển khoản", "online" => "Trực tuyến", "other" => "Khác", "report" => "Tổng hợp", _ => value ?? "—"
    };
}
