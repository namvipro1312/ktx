using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class service_subscription
{
    public long id { get; set; }

    public long contract_id { get; set; }

    public long service_id { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly? end_date { get; set; }

    public decimal quantity { get; set; }

    public decimal agreed_unit_price { get; set; }

    public string status { get; set; } = null!;

    public string? note { get; set; }

    public virtual contract contract { get; set; } = null!;

    public virtual service service { get; set; } = null!;
}
