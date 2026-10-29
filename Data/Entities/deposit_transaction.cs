using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class deposit_transaction
{
    public long id { get; set; }

    public long contract_id { get; set; }

    public string transaction_code { get; set; } = null!;

    public string transaction_type { get; set; } = null!;

    public decimal amount { get; set; }

    public string method { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime transaction_date { get; set; }

    public long? handled_by_user_id { get; set; }

    public string? reason { get; set; }

    public virtual contract contract { get; set; } = null!;

    public virtual user? handled_by_user { get; set; }
}
