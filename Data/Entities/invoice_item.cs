using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class invoice_item
{
    public long id { get; set; }

    public long invoice_id { get; set; }

    public string item_type { get; set; } = null!;

    public long? service_usage_id { get; set; }

    public string description { get; set; } = null!;

    public decimal quantity { get; set; }

    public decimal unit_price { get; set; }

    public decimal amount { get; set; }

    public virtual invoice invoice { get; set; } = null!;

    public virtual service_usage? service_usage { get; set; }

    public virtual violation_invoice_item? violation_invoice_item { get; set; }
}
