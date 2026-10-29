using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class audit_log
{
    public long id { get; set; }

    public long? user_id { get; set; }

    public string action { get; set; } = null!;

    public string entity_type { get; set; } = null!;

    public long? entity_id { get; set; }

    public string? old_values { get; set; }

    public string? new_values { get; set; }

    public string? ip_address { get; set; }

    public string? user_agent { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? user { get; set; }
}
