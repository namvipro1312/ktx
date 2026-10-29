using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class student
{
    public long id { get; set; }

    public long user_id { get; set; }

    public string student_code { get; set; } = null!;

    public DateOnly? date_of_birth { get; set; }

    public string gender { get; set; } = null!;

    public string? faculty { get; set; }

    public short? course_year { get; set; }

    public string? emergency_contact_name { get; set; }

    public string? emergency_contact_phone { get; set; }

    public DateTime created_at { get; set; }

    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    public virtual ICollection<guest_visit> guest_visits { get; set; } = new List<guest_visit>();

    public virtual ICollection<repair_request> repair_requests { get; set; } = new List<repair_request>();

    public virtual ICollection<room_registration> room_registrations { get; set; } = new List<room_registration>();

    public virtual ICollection<rule_acknowledgement> rule_acknowledgements { get; set; } = new List<rule_acknowledgement>();

    public virtual ICollection<student_violation> student_violations { get; set; } = new List<student_violation>();

    public virtual user user { get; set; } = null!;
}
