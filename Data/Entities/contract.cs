using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class contract
{
    public long id { get; set; }

    public string contract_code { get; set; } = null!;

    public long? registration_id { get; set; }

    public long student_id { get; set; }

    public long bed_id { get; set; }

    public long? created_by_user_id { get; set; }

    public DateOnly start_date { get; set; }

    public DateOnly end_date { get; set; }

    public decimal monthly_price { get; set; }

    public decimal deposit_amount { get; set; }

    public string status { get; set; } = null!;

    public DateTime? signed_at { get; set; }

    public string? terms { get; set; }

    public DateTime created_at { get; set; }

    public virtual bed bed { get; set; } = null!;

    public virtual ICollection<contract_amendment> contract_amendments { get; set; } = new List<contract_amendment>();

    public virtual user? created_by_user { get; set; }

    public virtual ICollection<deposit_transaction> deposit_transactions { get; set; } = new List<deposit_transaction>();

    public virtual ICollection<invoice> invoices { get; set; } = new List<invoice>();

    public virtual occupancy_record? occupancy_record { get; set; }

    public virtual room_registration? registration { get; set; }

    public virtual ICollection<service_subscription> service_subscriptions { get; set; } = new List<service_subscription>();

    public virtual ICollection<service_usage> service_usages { get; set; } = new List<service_usage>();

    public virtual student student { get; set; } = null!;

    public virtual ICollection<student_violation> student_violations { get; set; } = new List<student_violation>();
}
