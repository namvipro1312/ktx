using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class room_asset
{
    public long id { get; set; }

    public long room_id { get; set; }

    public long asset_type_id { get; set; }

    public string? asset_tag { get; set; }

    public string name { get; set; } = null!;

    public short quantity { get; set; }

    public string condition_status { get; set; } = null!;

    public DateOnly? acquired_at { get; set; }

    public DateTime? last_checked_at { get; set; }

    public string? note { get; set; }

    public virtual asset_type asset_type { get; set; } = null!;

    public virtual room room { get; set; } = null!;
}
