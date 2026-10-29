using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_repair_summary
{
    public string status { get; set; } = null!;

    public string priority { get; set; } = null!;

    public int? request_count { get; set; }
}
