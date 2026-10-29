using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class utility_meter
{
    public long id { get; set; }

    public long room_id { get; set; }

    public long service_id { get; set; }

    public string meter_code { get; set; } = null!;

    public DateOnly? installed_at { get; set; }

    public bool is_active { get; set; }

    public string? note { get; set; }

    public virtual room room { get; set; } = null!;

    public virtual service service { get; set; } = null!;

    public virtual ICollection<utility_reading> utility_readings { get; set; } = new List<utility_reading>();
}
