using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class user_role
{
    public long user_id { get; set; }

    public long role_id { get; set; }

    public DateTime assigned_at { get; set; }

    public virtual role role { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
