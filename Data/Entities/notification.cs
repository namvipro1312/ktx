using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class notification
{
    public long id { get; set; }

    public long user_id { get; set; }

    public string title { get; set; } = null!;

    public string message { get; set; } = null!;

    public string notification_type { get; set; } = null!;

    public string? entity_type { get; set; }

    public long? entity_id { get; set; }

    public DateTime? read_at { get; set; }

    public DateTime created_at { get; set; }

    public virtual user user { get; set; } = null!;
}
