using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class v_room_occupancy
{
    public long building_id { get; set; }

    public string building_code { get; set; } = null!;

    public string building_name { get; set; } = null!;

    public long room_id { get; set; }

    public string room_number { get; set; } = null!;

    public short capacity { get; set; }

    public int? configured_beds { get; set; }

    public int? active_contracts { get; set; }

    public int? available_slots { get; set; }
}
