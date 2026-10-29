using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class rule_acknowledgement
{
    public long rule_id { get; set; }

    public long student_id { get; set; }

    public DateTime acknowledged_at { get; set; }

    public virtual dormitory_rule rule { get; set; } = null!;

    public virtual student student { get; set; } = null!;
}
