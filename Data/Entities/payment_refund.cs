using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class payment_refund
{
    public long id { get; set; }

    public long payment_id { get; set; }

    public string refund_code { get; set; } = null!;

    public decimal amount { get; set; }

    public string? reason { get; set; }

    public string status { get; set; } = null!;

    public string method { get; set; } = null!;

    public long? processed_by_user_id { get; set; }

    public DateTime? processed_at { get; set; }

    public DateTime created_at { get; set; }

    public virtual payment payment { get; set; } = null!;

    public virtual user? processed_by_user { get; set; }
}
