using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class permission
{
    public long id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string module { get; set; } = null!;

    public virtual ICollection<role> roles { get; set; } = new List<role>();
}
