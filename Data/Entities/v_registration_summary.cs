using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_registration_summary
{
    public string status { get; set; } = null!;

    public int? registration_count { get; set; }
}
