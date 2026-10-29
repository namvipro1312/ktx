using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class violation_invoice_item
{
    public long violation_id { get; set; }

    public long invoice_item_id { get; set; }

    public DateTime linked_at { get; set; }

    public virtual invoice_item invoice_item { get; set; } = null!;

    public virtual student_violation violation { get; set; } = null!;
}
