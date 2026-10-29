using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class repair_request_update
{
    public long id { get; set; }

    public long repair_request_id { get; set; }

    public long updated_by_user_id { get; set; }

    public string? old_status { get; set; }

    public string? new_status { get; set; }

    public string? comment { get; set; }

    public DateTime created_at { get; set; }

    public virtual repair_request repair_request { get; set; } = null!;

    public virtual user updated_by_user { get; set; } = null!;
}
