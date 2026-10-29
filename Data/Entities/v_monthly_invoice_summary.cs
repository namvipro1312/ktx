using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_monthly_invoice_summary
{
    public DateOnly? billing_month { get; set; }

    public int? invoice_count { get; set; }

    public decimal? billed_amount { get; set; }

    public decimal? collected_amount { get; set; }

    public decimal? outstanding_amount { get; set; }
}
