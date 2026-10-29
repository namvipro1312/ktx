using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class document
{
    public long id { get; set; }

    public string entity_type { get; set; } = null!;

    public long entity_id { get; set; }

    public long? uploaded_by_user_id { get; set; }

    public string original_name { get; set; } = null!;

    public string storage_key { get; set; } = null!;

    public string? mime_type { get; set; }

    public long? size_bytes { get; set; }

    public string? checksum_sha256 { get; set; }

    public DateTime created_at { get; set; }

    public virtual user? uploaded_by_user { get; set; }
}
