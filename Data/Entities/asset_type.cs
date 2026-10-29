using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class asset_type
{
    public long id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public virtual ICollection<room_asset> room_assets { get; set; } = new List<room_asset>();
}
