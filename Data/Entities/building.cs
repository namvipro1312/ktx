using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class building
{
    public long id { get; set; }

    public string code { get; set; } = null!;

    public string name { get; set; } = null!;

    public string? address { get; set; }

    public string? description { get; set; }

    public bool is_active { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<room_registration> room_registrations { get; set; } = new List<room_registration>();

    public virtual ICollection<room> rooms { get; set; } = new List<room>();
}
