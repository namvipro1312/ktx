using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class service_usage
{
    public long id { get; set; }

    public long service_id { get; set; }

    public long contract_id { get; set; }

    public DateOnly period_start { get; set; }

    public DateOnly period_end { get; set; }

    public decimal quantity { get; set; }

    public decimal unit_price { get; set; }

    public long? recorded_by_user_id { get; set; }

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual contract contract { get; set; } = null!;

    public virtual ICollection<invoice_item> invoice_items { get; set; } = new List<invoice_item>();

    public virtual user? recorded_by_user { get; set; }

    public virtual service service { get; set; } = null!;

    public virtual utility_reading? utility_reading { get; set; }
}
