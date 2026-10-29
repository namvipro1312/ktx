using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class student_violation
{
    public long id { get; set; }

    public long student_id { get; set; }

    public long? contract_id { get; set; }

    public long? rule_id { get; set; }

    public long recorded_by_user_id { get; set; }

    public string violation_code { get; set; } = null!;

    public string title { get; set; } = null!;

    public string? description { get; set; }

    public DateTime occurred_at { get; set; }

    public decimal fine_amount { get; set; }

    public string status { get; set; } = null!;

    public string? resolution_note { get; set; }

    public DateTime? resolved_at { get; set; }

    public DateTime created_at { get; set; }

    public virtual contract? contract { get; set; }

    public virtual user recorded_by_user { get; set; } = null!;

    public virtual dormitory_rule? rule { get; set; }

    public virtual student student { get; set; } = null!;

    public virtual violation_invoice_item? violation_invoice_item { get; set; }
}
