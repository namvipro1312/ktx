using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class dormitory_rule
{
    public long id { get; set; }

    public string title { get; set; } = null!;

    public string content { get; set; } = null!;

    public string version { get; set; } = null!;

    public DateOnly effective_from { get; set; }

    public DateOnly? effective_to { get; set; }

    public bool is_active { get; set; }

    public long? created_by_user_id { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? created_by_user { get; set; }

    public virtual ICollection<rule_acknowledgement> rule_acknowledgements { get; set; } = new List<rule_acknowledgement>();

    public virtual ICollection<student_violation> student_violations { get; set; } = new List<student_violation>();
}
