using System;
using System.Collections.Generic;

namespace QLKTX.Data.Entities;

public partial class user
{
    public long id { get; set; }

    public string username { get; set; } = null!;

    public string? email { get; set; }

    public string password_hash { get; set; } = null!;

    public string full_name { get; set; } = null!;

    public string? phone { get; set; }

    public bool is_active { get; set; }

    public DateTime? last_login_at { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<audit_log> audit_logs { get; set; } = new List<audit_log>();

    public virtual ICollection<contract_amendment> contract_amendments { get; set; } = new List<contract_amendment>();

    public virtual ICollection<contract> contracts { get; set; } = new List<contract>();

    public virtual ICollection<deposit_transaction> deposit_transactions { get; set; } = new List<deposit_transaction>();

    public virtual ICollection<document> documents { get; set; } = new List<document>();

    public virtual ICollection<dormitory_rule> dormitory_rules { get; set; } = new List<dormitory_rule>();

    public virtual ICollection<guest_visit> guest_visits { get; set; } = new List<guest_visit>();

    public virtual ICollection<notification> notifications { get; set; } = new List<notification>();

    public virtual ICollection<occupancy_record> occupancy_recordchecked_in_by_users { get; set; } = new List<occupancy_record>();

    public virtual ICollection<occupancy_record> occupancy_recordchecked_out_by_users { get; set; } = new List<occupancy_record>();

    public virtual ICollection<password_reset_token> password_reset_tokens { get; set; } = new List<password_reset_token>();

    public virtual ICollection<payment_refund> payment_refunds { get; set; } = new List<payment_refund>();

    public virtual ICollection<payment> payments { get; set; } = new List<payment>();

    public virtual ICollection<registration_bed_offer> registration_bed_offers { get; set; } = new List<registration_bed_offer>();

    public virtual ICollection<registration_review> registration_reviews { get; set; } = new List<registration_review>();

    public virtual ICollection<repair_request_update> repair_request_updates { get; set; } = new List<repair_request_update>();

    public virtual ICollection<repair_request> repair_requestassigned_to_users { get; set; } = new List<repair_request>();

    public virtual ICollection<repair_request> repair_requestreported_by_users { get; set; } = new List<repair_request>();

    public virtual ICollection<service_usage> service_usages { get; set; } = new List<service_usage>();

    public virtual student? student { get; set; }

    public virtual ICollection<student_violation> student_violations { get; set; } = new List<student_violation>();

    public virtual ICollection<user_role> user_roles { get; set; } = new List<user_role>();

    public virtual ICollection<user_session> user_sessions { get; set; } = new List<user_session>();

    public virtual ICollection<utility_reading> utility_readings { get; set; } = new List<utility_reading>();
}
