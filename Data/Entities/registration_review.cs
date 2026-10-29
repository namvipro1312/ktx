using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class registration_review
{
    public long id { get; set; }

    public long registration_id { get; set; }

    public long reviewer_user_id { get; set; }

    public string decision { get; set; } = null!;

    public string? comment { get; set; }

    public DateTime reviewed_at { get; set; }

    public virtual room_registration registration { get; set; } = null!;

    public virtual user reviewer_user { get; set; } = null!;
}
