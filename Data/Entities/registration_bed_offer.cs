using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class registration_bed_offer
{
    public long id { get; set; }

    public long registration_id { get; set; }

    public long bed_id { get; set; }

    public long offered_by_user_id { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public string status { get; set; } = null!;

    public DateTime? expires_at { get; set; }

    public DateTime? responded_at { get; set; }

    public string? note { get; set; }

    public DateTime created_at { get; set; }

    public virtual bed bed { get; set; } = null!;

    public virtual user offered_by_user { get; set; } = null!;

    public virtual room_registration registration { get; set; } = null!;
}
