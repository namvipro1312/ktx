using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class utility_reading
{
    public long id { get; set; }

    public long meter_id { get; set; }

    public DateOnly reading_date { get; set; }

    public decimal previous_value { get; set; }

    public decimal current_value { get; set; }

    public decimal consumption { get; set; }

    public decimal unit_price { get; set; }

    public decimal total_amount { get; set; }

    public long? recorded_by_user_id { get; set; }

    public long? service_usage_id { get; set; }

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual utility_meter meter { get; set; } = null!;

    public virtual user? recorded_by_user { get; set; }

    public virtual service_usage? service_usage { get; set; }
}
