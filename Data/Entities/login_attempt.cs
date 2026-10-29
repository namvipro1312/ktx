using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class login_attempt
{
    public long id { get; set; }

    public string username { get; set; } = null!;

    public string? ip_address { get; set; }

    public bool succeeded { get; set; }

    public DateTime attempted_at { get; set; }
}
