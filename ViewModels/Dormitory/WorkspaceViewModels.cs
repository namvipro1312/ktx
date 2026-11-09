namespace QLKTX.ViewModels;

public sealed class DashboardViewModel
{
    public bool IsStudent { get; init; }
    public int PendingRegistrations { get; init; }
    public int AvailableBeds { get; init; }
    public int ActiveContracts { get; init; }
    public decimal Outstanding { get; init; }
    public string DisplayName { get; init; } = "";
    public List<StationViewModel> Stations { get; init; } = [];
    public List<DashboardTaskViewModel> Tasks { get; init; } = [];
}

public sealed class StationViewModel(string key, string label, int count, string color)
{
    public string Key { get; } = key;
    public string Label { get; } = label;
    public int Count { get; } = count;
    public string Color { get; } = color;
}

public sealed class DashboardTaskViewModel(string title, string detail, string state, string href)
{
    public string Title { get; } = title;
    public string Detail { get; } = detail;
    public string State { get; } = state;
    public string Href { get; } = href;
}

public sealed class ModuleViewModel
{
    public string Section { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string CountLabel { get; set; } = "";
    public List<string> Columns { get; set; } = [];
    public List<ModuleRowViewModel> Rows { get; set; } = [];
    public List<FormFieldViewModel> Fields { get; set; } = [];
    public string? FormTitle { get; set; }
    public string? FormHelp { get; set; }
    public List<RoomMatrixGroupViewModel> RoomGroups { get; set; } = [];
    public RegistrationStatsViewModel? RegistrationStats { get; set; }
    public List<RegistrationItemViewModel> Registrations { get; set; } = [];
    public List<RegistrationAvailableBedOption> AvailableBedsList { get; set; } = [];
}

public sealed class ModuleRowViewModel
{
    public long Id { get; init; }
    public List<string> Cells { get; init; } = [];
    public string State { get; init; } = "";
    public List<RowActionViewModel> Actions { get; init; } = [];
}

public sealed class RowActionViewModel(string label, string operation, bool primary = false)
{
    public string Label { get; } = label;
    public string Operation { get; } = operation;
    public bool Primary { get; } = primary;
}

public sealed class FormFieldViewModel(string name, string label, string type = "text", bool required = true, List<SelectOptionViewModel>? options = null)
{
    public string Name { get; } = name;
    public string Label { get; } = label;
    public string Type { get; } = type;
    public bool Required { get; } = required;
    public List<SelectOptionViewModel> Options { get; } = options ?? [];
}

public sealed class SelectOptionViewModel(string value, string label)
{
    public string Value { get; } = value;
    public string Label { get; } = label;
}

public sealed class RoomMatrixGroupViewModel
{
    public string BuildingName { get; init; } = "";
    public string BuildingCode { get; init; } = "";
    public List<FloorMatrixViewModel> Floors { get; init; } = [];
}

public sealed class FloorMatrixViewModel
{
    public int FloorNumber { get; init; }
    public List<RoomCardViewModel> Rooms { get; init; } = [];
}

public sealed class RoomCardViewModel
{
    public long Id { get; init; }
    public string RoomNumber { get; init; } = "";
    public string RoomType { get; init; } = "";
    public int Capacity { get; init; }
    public int AvailableBeds { get; init; }
    public decimal MonthlyPrice { get; init; }
    public string Status { get; init; } = "";
    public List<BedCardViewModel> Beds { get; init; } = [];
}

public sealed class BedCardViewModel
{
    public long Id { get; init; }
    public string BedNumber { get; init; } = "";
    public string Status { get; init; } = "";
}

public sealed class RegistrationStatsViewModel
{
    public int TotalCount { get; init; }
    public int PendingCount { get; init; }
    public int ContractedCount { get; init; }
    public int RejectedCount { get; init; }
}

public sealed class RegistrationItemViewModel
{
    public long Id { get; init; }
    public string RegistrationCode { get; init; } = "";
    public long StudentId { get; init; }
    public string StudentName { get; init; } = "";
    public string StudentCode { get; init; } = "";
    public string Gender { get; init; } = "Nam";
    public string Faculty { get; init; } = "";
    public string Phone { get; init; } = "";
    public string PreferredBuildingName { get; init; } = "Mọi tòa";
    public string PreferredRoomType { get; init; } = "Mọi loại";
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime SubmittedAt { get; init; }
    public string Status { get; init; } = "pending";
    public string? Note { get; init; }
    public string? PriorityType { get; init; }
    public string? AssignedBedInfo { get; init; }
    public string? ReviewDecision { get; init; }
    public string? ReviewComment { get; init; }
    public DateTime? ReviewedAt { get; init; }
    public string? ReviewerName { get; init; }
    public long? ContractId { get; init; }
    public string? ContractCode { get; init; }
}

public sealed class RegistrationAvailableBedOption
{
    public long BedId { get; init; }
    public string DisplayLabel { get; init; } = "";
    public string BuildingName { get; init; } = "";
    public string RoomNumber { get; init; } = "";
    public string BedNumber { get; init; } = "";
    public string RoomType { get; init; } = "";
    public decimal MonthlyPrice { get; init; }
}

