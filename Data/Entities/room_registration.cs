using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class room_registration
{
    public long id { get; set; }

    public string registration_code { get; set; } = null!;

    public long student_id { get; set; }

    public long? preferred_building_id { get; set; }

    public string? preferred_room_type { get; set; }

    public DateOnly requested_start_date { get; set; }

    public DateOnly requested_end_date { get; set; }

    public DateTime submitted_at { get; set; }

    public string status { get; set; } = null!;

    public string? note { get; set; }

    public virtual contract? contract { get; set; }

    public virtual building? preferred_building { get; set; }

    public virtual ICollection<registration_bed_offer> registration_bed_offers { get; set; } = new List<registration_bed_offer>();

    public virtual ICollection<registration_review> registration_reviews { get; set; } = new List<registration_review>();

    public virtual student student { get; set; } = null!;
}
