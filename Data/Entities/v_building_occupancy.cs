using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_building_occupancy
{
    public long building_id { get; set; }

    public string building_code { get; set; } = null!;

    public string building_name { get; set; } = null!;

    public int? room_count { get; set; }

    public int? nominal_capacity { get; set; }

    public int? configured_beds { get; set; }

    public int? current_occupants { get; set; }

    public int? available_slots { get; set; }
}
