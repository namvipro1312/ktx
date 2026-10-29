using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class guest_visit
{
    public long id { get; set; }

    public long student_id { get; set; }

    public string guest_name { get; set; } = null!;

    public string? guest_identity_no { get; set; }

    public string? guest_phone { get; set; }

    public string? visit_purpose { get; set; }

    public DateTime expected_arrival { get; set; }

    public DateTime? expected_departure { get; set; }

    public DateTime? actual_arrival { get; set; }

    public DateTime? actual_departure { get; set; }

    public long? approved_by_user_id { get; set; }

    public string status { get; set; } = null!;

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? approved_by_user { get; set; }

    public virtual student student { get; set; } = null!;
}
