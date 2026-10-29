using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class occupancy_record
{
    public long id { get; set; }

    public long contract_id { get; set; }

    public DateTime? checked_in_at { get; set; }

    public long? checked_in_by_user_id { get; set; }

    public string? checkin_condition { get; set; }

    public string? checkin_note { get; set; }

    public DateOnly expected_checkout_date { get; set; }

    public DateTime? checked_out_at { get; set; }

    public long? checked_out_by_user_id { get; set; }

    public string? checkout_condition { get; set; }

    public string? checkout_note { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? checked_in_by_user { get; set; }

    public virtual user? checked_out_by_user { get; set; }

    public virtual contract contract { get; set; } = null!;
}
