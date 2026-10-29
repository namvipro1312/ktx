using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class payment
{
    public long id { get; set; }

    public string payment_code { get; set; } = null!;

    public long invoice_id { get; set; }

    public decimal amount { get; set; }

    public string method { get; set; } = null!;

    public string status { get; set; } = null!;

    public string? transaction_reference { get; set; }

    public long? received_by_user_id { get; set; }

    public DateTime paid_at { get; set; }

    public string? note { get; set; }

    public virtual invoice invoice { get; set; } = null!;

    public virtual ICollection<payment_refund> payment_refunds { get; set; } = new List<payment_refund>();

    public virtual user? received_by_user { get; set; }
}
