using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_current_resident
{
    public long contract_id { get; set; }

    public string contract_code { get; set; } = null!;

    public long student_id { get; set; }

    public string student_code { get; set; } = null!;

    public string full_name { get; set; } = null!;

    public string? phone { get; set; }

    public string building_name { get; set; } = null!;

    public string room_number { get; set; } = null!;

    public string bed_number { get; set; } = null!;

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public DateTime? checked_in_at { get; set; }
}
