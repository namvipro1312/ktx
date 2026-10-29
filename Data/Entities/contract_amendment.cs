using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class contract_amendment
{
    public long id { get; set; }

    public long contract_id { get; set; }

    public string amendment_code { get; set; } = null!;

    public string amendment_type { get; set; } = null!;

    public string? old_values { get; set; }

    public string? new_values { get; set; }

    public string? reason { get; set; }

    public DateOnly effective_date { get; set; }

    public long? created_by_user_id { get; set; }

    public DateTime? signed_at { get; set; }

    public string? document_path { get; set; }

    public DateTime created_at { get; set; }

    public virtual contract contract { get; set; } = null!;

    public virtual user? created_by_user { get; set; }
}
