using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class room
{
    public long id { get; set; }

    public long building_id { get; set; }

    public string room_number { get; set; } = null!;

    public short? floor_number { get; set; }

    public string? room_type { get; set; }

    public short capacity { get; set; }

    public decimal monthly_price { get; set; }

    public string status { get; set; } = null!;

    public string? description { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<bed> beds { get; set; } = new List<bed>();

    public virtual building building { get; set; } = null!;

    public virtual ICollection<repair_request> repair_requests { get; set; } = new List<repair_request>();

    public virtual ICollection<room_asset> room_assets { get; set; } = new List<room_asset>();

    public virtual ICollection<utility_meter> utility_meters { get; set; } = new List<utility_meter>();
}
