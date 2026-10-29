using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class password_reset_token
{
    public long id { get; set; }

    public long user_id { get; set; }

    public string token_hash { get; set; } = null!;

    public DateTime expires_at { get; set; }

    public DateTime? used_at { get; set; }

    public DateTime created_at { get; set; }

    public virtual user user { get; set; } = null!;
}
