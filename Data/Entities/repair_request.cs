using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class repair_request
{
    public long id { get; set; }

    public string request_code { get; set; } = null!;

    public long? student_id { get; set; }

    public long room_id { get; set; }

    public long reported_by_user_id { get; set; }

    public long? assigned_to_user_id { get; set; }

    public string title { get; set; } = null!;

    public string description { get; set; } = null!;

    public string priority { get; set; } = null!;

    public string status { get; set; } = null!;

    public DateTime reported_at { get; set; }

    public DateTime? completed_at { get; set; }

    public string? resolution_note { get; set; }

    public virtual user? assigned_to_user { get; set; }

    public virtual ICollection<repair_request_update> repair_request_updates { get; set; } = new List<repair_request_update>();

    public virtual user reported_by_user { get; set; } = null!;

    public virtual room room { get; set; } = null!;

    public virtual student? student { get; set; }
}
