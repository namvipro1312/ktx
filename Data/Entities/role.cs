using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class role
{
    public long id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();

    public virtual ICollection<permission> permissions { get; set; } = new List<permission>();
}
