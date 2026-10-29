using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class bed
{
    public long id { get; set; }

    public long room_id { get; set; }

    public string bed_number { get; set; } = null!;

    public string status { get; set; } = null!;

    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    public virtual ICollection<registration_bed_offer> registration_bed_offers { get; set; } = new List<registration_bed_offer>();

    public virtual room room { get; set; } = null!;
}
