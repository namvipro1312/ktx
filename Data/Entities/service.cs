using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class service
{
    public long id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string unit { get; set; } = null!;

    public decimal unit_price { get; set; }

    public bool is_metered { get; set; }

    public bool is_active { get; set; }

    public string? description { get; set; }

    public virtual ICollection<service_subscription> service_subscriptions { get; set; } = new List<service_subscription>();

    public virtual ICollection<service_usage> service_usages { get; set; } = new List<service_usage>();

    public virtual ICollection<utility_meter> utility_meters { get; set; } = new List<utility_meter>();
}
