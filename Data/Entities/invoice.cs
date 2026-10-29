using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class invoice
{
    public long id { get; set; }

    public string invoice_code { get; set; } = null!;

    public long contract_id { get; set; }

    public DateOnly period_start { get; set; }

    public DateOnly period_end { get; set; }

    public DateTime issued_at { get; set; }

    public DateOnly due_date { get; set; }

    public string status { get; set; } = null!;

    public string? note { get; set; }

    public virtual contract contract { get; set; } = null!;

    public virtual ICollection<invoice_item> invoice_items { get; set; } = new List<invoice_item>();

    public virtual ICollection<payment> payments { get; set; } = new List<payment>();
}
