using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_invoice_balance
{
    public long invoice_id { get; set; }

    public string invoice_code { get; set; } = null!;

    public long contract_id { get; set; }

    public DateOnly period_start { get; set; }

    public DateOnly period_end { get; set; }

    public DateOnly due_date { get; set; }

    public string status { get; set; } = null!;

    public decimal? total_amount { get; set; }

    public decimal? paid_amount { get; set; }

    public decimal? balance_amount { get; set; }
}
