using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_student_outstanding_balance
{
    public long student_id { get; set; }

    public string student_code { get; set; } = null!;

    public string full_name { get; set; } = null!;

    public int? unpaid_invoice_count { get; set; }

    public decimal? outstanding_amount { get; set; }
}
