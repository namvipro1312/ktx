using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QLKTX.Data.Entities;

namespace QLKTX.Data;

public partial class QlKtxContext : DbContext
{
    public QlKtxContext(DbContextOptions<QlKtxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<asset_type> asset_types { get; set; }

    public virtual DbSet<audit_log> audit_logs { get; set; }

    public virtual DbSet<bed> beds { get; set; }

    public virtual DbSet<building> buildings { get; set; }

    public virtual DbSet<contract> contracts { get; set; }

    public virtual DbSet<contract_amendment> contract_amendments { get; set; }

    public virtual DbSet<deposit_transaction> deposit_transactions { get; set; }

    public virtual DbSet<document> documents { get; set; }

    public virtual DbSet<dormitory_rule> dormitory_rules { get; set; }

    public virtual DbSet<guest_visit> guest_visits { get; set; }

    public virtual DbSet<invoice> invoices { get; set; }

    public virtual DbSet<invoice_item> invoice_items { get; set; }

    public virtual DbSet<login_attempt> login_attempts { get; set; }

    public virtual DbSet<notification> notifications { get; set; }

    public virtual DbSet<occupancy_record> occupancy_records { get; set; }

    public virtual DbSet<password_reset_token> password_reset_tokens { get; set; }

    public virtual DbSet<payment> payments { get; set; }

    public virtual DbSet<payment_refund> payment_refunds { get; set; }

    public virtual DbSet<permission> permissions { get; set; }

    public virtual DbSet<registration_bed_offer> registration_bed_offers { get; set; }

    public virtual DbSet<registration_review> registration_reviews { get; set; }

    public virtual DbSet<repair_request> repair_requests { get; set; }

    public virtual DbSet<repair_request_update> repair_request_updates { get; set; }

    public virtual DbSet<role> roles { get; set; }

    public virtual DbSet<room> rooms { get; set; }

    public virtual DbSet<room_asset> room_assets { get; set; }

    public virtual DbSet<room_registration> room_registrations { get; set; }

    public virtual DbSet<rule_acknowledgement> rule_acknowledgements { get; set; }

    public virtual DbSet<service> services { get; set; }

    public virtual DbSet<service_subscription> service_subscriptions { get; set; }

    public virtual DbSet<service_usage> service_usages { get; set; }

    public virtual DbSet<student> students { get; set; }

    public virtual DbSet<student_violation> student_violations { get; set; }

    public virtual DbSet<user> users { get; set; }

    public virtual DbSet<user_role> user_roles { get; set; }

    public virtual DbSet<user_session> user_sessions { get; set; }

    public virtual DbSet<utility_meter> utility_meters { get; set; }

    public virtual DbSet<utility_reading> utility_readings { get; set; }

    public virtual DbSet<v_building_occupancy> v_building_occupancies { get; set; }

    public virtual DbSet<v_current_resident> v_current_residents { get; set; }

    public virtual DbSet<v_invoice_balance> v_invoice_balances { get; set; }

    public virtual DbSet<v_monthly_invoice_summary> v_monthly_invoice_summaries { get; set; }

    public virtual DbSet<v_registration_summary> v_registration_summaries { get; set; }

    public virtual DbSet<v_repair_summary> v_repair_summaries { get; set; }

    public virtual DbSet<v_room_occupancy> v_room_occupancies { get; set; }

    public virtual DbSet<v_student_outstanding_balance> v_student_outstanding_balances { get; set; }

    public virtual DbSet<violation_invoice_item> violation_invoice_items { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<asset_type>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__asset_ty__3213E83F75C2E190");

            entity.HasIndex(e => e.code, "uq_asset_types_code").IsUnique();

            entity.Property(e => e.code).HasMaxLength(40);
            entity.Property(e => e.description).HasMaxLength(500);
            entity.Property(e => e.name).HasMaxLength(120);
        });

        modelBuilder.Entity<audit_log>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__audit_lo__3213E83FD88D8A89");

            entity.HasIndex(e => new { e.entity_type, e.entity_id, e.created_at }, "idx_audit_entity_date");

            entity.HasIndex(e => new { e.user_id, e.created_at }, "idx_audit_user_date");

            entity.Property(e => e.action).HasMaxLength(100);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.entity_type).HasMaxLength(80);
            entity.Property(e => e.ip_address).HasMaxLength(45);
            entity.Property(e => e.user_agent).HasMaxLength(500);

            entity.HasOne(d => d.user).WithMany(p => p.audit_logs)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_audit_user");
        });

        modelBuilder.Entity<bed>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__beds__3213E83F7485C821");

            entity.HasIndex(e => new { e.room_id, e.status }, "idx_beds_room_status");

            entity.HasIndex(e => new { e.room_id, e.bed_number }, "uq_beds_room_number").IsUnique();

            entity.Property(e => e.bed_number).HasMaxLength(30);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("available");

            entity.HasOne(d => d.room).WithMany(p => p.beds)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_beds_room");
        });

        modelBuilder.Entity<building>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__building__3213E83FAB16A21D");

            entity.HasIndex(e => e.code, "uq_buildings_code").IsUnique();

            entity.Property(e => e.address).HasMaxLength(255);
            entity.Property(e => e.code).HasMaxLength(30);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(150);
        });

        modelBuilder.Entity<contract>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__contract__3213E83F033FD408");

            entity.HasIndex(e => new { e.bed_id, e.start_date, e.end_date }, "idx_contracts_bed_dates");

            entity.HasIndex(e => new { e.student_id, e.status }, "idx_contracts_student_status");

            entity.HasIndex(e => e.contract_code, "uq_contracts_code").IsUnique();

            entity.HasIndex(e => e.registration_id, "uq_contracts_registration")
                .IsUnique()
                .HasFilter("([registration_id] IS NOT NULL)");

            entity.Property(e => e.contract_code).HasMaxLength(40);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.deposit_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.monthly_price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.signed_at).HasPrecision(0);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("draft");
            entity.Property(e => e.terms).HasColumnType("text");

            entity.HasOne(d => d.bed).WithMany(p => p.contracts)
                .HasForeignKey(d => d.bed_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_contracts_bed");

            entity.HasOne(d => d.created_by_user).WithMany(p => p.contracts)
                .HasForeignKey(d => d.created_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_contracts_creator");

            entity.HasOne(d => d.registration).WithOne(p => p.contract)
                .HasForeignKey<contract>(d => d.registration_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_contracts_registration");

            entity.HasOne(d => d.student).WithMany(p => p.contracts)
                .HasForeignKey(d => d.student_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_contracts_student");
        });

        modelBuilder.Entity<contract_amendment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__contract__3213E83F632038FD");

            entity.HasIndex(e => new { e.contract_id, e.effective_date }, "idx_contract_amendments_contract");

            entity.HasIndex(e => e.amendment_code, "uq_contract_amendment_code").IsUnique();

            entity.Property(e => e.amendment_code).HasMaxLength(40);
            entity.Property(e => e.amendment_type).HasMaxLength(30);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.document_path).HasMaxLength(500);
            entity.Property(e => e.reason).HasColumnType("text");
            entity.Property(e => e.signed_at).HasPrecision(0);

            entity.HasOne(d => d.contract).WithMany(p => p.contract_amendments)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_contract_amendments_contract");

            entity.HasOne(d => d.created_by_user).WithMany(p => p.contract_amendments)
                .HasForeignKey(d => d.created_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_contract_amendments_user");
        });

        modelBuilder.Entity<deposit_transaction>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__deposit___3213E83F528A76CE");

            entity.HasIndex(e => new { e.contract_id, e.transaction_date }, "idx_deposit_contract_date");

            entity.HasIndex(e => e.transaction_code, "uq_deposit_transaction_code").IsUnique();

            entity.Property(e => e.amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.method)
                .HasMaxLength(30)
                .HasDefaultValue("bank_transfer");
            entity.Property(e => e.reason).HasMaxLength(500);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("completed");
            entity.Property(e => e.transaction_code).HasMaxLength(40);
            entity.Property(e => e.transaction_date)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.transaction_type).HasMaxLength(30);

            entity.HasOne(d => d.contract).WithMany(p => p.deposit_transactions)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_deposit_contract");

            entity.HasOne(d => d.handled_by_user).WithMany(p => p.deposit_transactions)
                .HasForeignKey(d => d.handled_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_deposit_handler");
        });

        modelBuilder.Entity<document>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__document__3213E83F6EA24F1E");

            entity.HasIndex(e => new { e.entity_type, e.entity_id }, "idx_documents_entity");

            entity.Property(e => e.checksum_sha256)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.entity_type).HasMaxLength(30);
            entity.Property(e => e.mime_type).HasMaxLength(150);
            entity.Property(e => e.original_name).HasMaxLength(255);
            entity.Property(e => e.storage_key).HasMaxLength(500);

            entity.HasOne(d => d.uploaded_by_user).WithMany(p => p.documents)
                .HasForeignKey(d => d.uploaded_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_documents_uploader");
        });

        modelBuilder.Entity<dormitory_rule>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__dormitor__3213E83FA162A23C");

            entity.HasIndex(e => new { e.is_active, e.effective_from, e.effective_to }, "idx_rules_active_dates");

            entity.HasIndex(e => new { e.title, e.version }, "uq_rules_title_version").IsUnique();

            entity.Property(e => e.content).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.title).HasMaxLength(200);
            entity.Property(e => e.version).HasMaxLength(30);

            entity.HasOne(d => d.created_by_user).WithMany(p => p.dormitory_rules)
                .HasForeignKey(d => d.created_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_rules_creator");
        });

        modelBuilder.Entity<guest_visit>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__guest_vi__3213E83F693D5363");

            entity.HasIndex(e => new { e.status, e.expected_arrival }, "idx_guest_visits_status");

            entity.HasIndex(e => new { e.student_id, e.expected_arrival }, "idx_guest_visits_student_date");

            entity.Property(e => e.actual_arrival).HasPrecision(0);
            entity.Property(e => e.actual_departure).HasPrecision(0);
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.expected_arrival).HasPrecision(0);
            entity.Property(e => e.expected_departure).HasPrecision(0);
            entity.Property(e => e.guest_identity_no).HasMaxLength(80);
            entity.Property(e => e.guest_name).HasMaxLength(200);
            entity.Property(e => e.guest_phone).HasMaxLength(30);
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("pending");
            entity.Property(e => e.visit_purpose).HasMaxLength(500);

            entity.HasOne(d => d.approved_by_user).WithMany(p => p.guest_visits)
                .HasForeignKey(d => d.approved_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_guest_visits_approver");

            entity.HasOne(d => d.student).WithMany(p => p.guest_visits)
                .HasForeignKey(d => d.student_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_guest_visits_student");
        });

        modelBuilder.Entity<invoice>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__invoices__3213E83FD2195905");

            entity.HasIndex(e => new { e.status, e.due_date }, "idx_invoices_status_due");

            entity.HasIndex(e => new { e.contract_id, e.period_start, e.period_end }, "uq_invoice_contract_period").IsUnique();

            entity.HasIndex(e => e.invoice_code, "uq_invoices_code").IsUnique();

            entity.Property(e => e.invoice_code).HasMaxLength(40);
            entity.Property(e => e.issued_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("draft");

            entity.HasOne(d => d.contract).WithMany(p => p.invoices)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_invoices_contract");
        });

        modelBuilder.Entity<invoice_item>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__invoice___3213E83F8A982327");

            entity.HasIndex(e => e.invoice_id, "idx_invoice_items_invoice");

            entity.Property(e => e.amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.item_type).HasMaxLength(30);
            entity.Property(e => e.quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(12, 3)");
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.invoice).WithMany(p => p.invoice_items)
                .HasForeignKey(d => d.invoice_id)
                .HasConstraintName("fk_invoice_items_invoice");

            entity.HasOne(d => d.service_usage).WithMany(p => p.invoice_items)
                .HasForeignKey(d => d.service_usage_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_invoice_items_usage");
        });

        modelBuilder.Entity<login_attempt>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__login_at__3213E83F7C7C1E37");

            entity.HasIndex(e => new { e.ip_address, e.attempted_at }, "idx_login_attempts_ip_time");

            entity.HasIndex(e => new { e.username, e.attempted_at }, "idx_login_attempts_username_time");

            entity.Property(e => e.attempted_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ip_address).HasMaxLength(45);
            entity.Property(e => e.username).HasMaxLength(100);
        });

        modelBuilder.Entity<notification>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__notifica__3213E83FDD45E372");

            entity.HasIndex(e => new { e.user_id, e.read_at, e.created_at }, "idx_notifications_user_unread");

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.entity_type).HasMaxLength(60);
            entity.Property(e => e.message).HasColumnType("text");
            entity.Property(e => e.notification_type)
                .HasMaxLength(60)
                .HasDefaultValue("general");
            entity.Property(e => e.read_at).HasPrecision(0);
            entity.Property(e => e.title).HasMaxLength(200);

            entity.HasOne(d => d.user).WithMany(p => p.notifications)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_notifications_user");
        });

        modelBuilder.Entity<occupancy_record>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__occupanc__3213E83F458D8569");

            entity.HasIndex(e => new { e.checked_out_at, e.expected_checkout_date }, "idx_occupancy_checkout");

            entity.HasIndex(e => e.contract_id, "uq_occupancy_contract").IsUnique();

            entity.Property(e => e.checked_in_at).HasPrecision(0);
            entity.Property(e => e.checked_out_at).HasPrecision(0);
            entity.Property(e => e.checkin_note).HasColumnType("text");
            entity.Property(e => e.checkout_note).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.checked_in_by_user).WithMany(p => p.occupancy_recordchecked_in_by_users)
                .HasForeignKey(d => d.checked_in_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_occupancy_checkin_user");

            entity.HasOne(d => d.checked_out_by_user).WithMany(p => p.occupancy_recordchecked_out_by_users)
                .HasForeignKey(d => d.checked_out_by_user_id)
                .HasConstraintName("fk_occupancy_checkout_user");

            entity.HasOne(d => d.contract).WithOne(p => p.occupancy_record)
                .HasForeignKey<occupancy_record>(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_occupancy_contract");
        });

        modelBuilder.Entity<password_reset_token>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__password__3213E83F247462E9");

            entity.HasIndex(e => new { e.user_id, e.expires_at }, "idx_password_reset_user_expiry");

            entity.HasIndex(e => e.token_hash, "uq_password_reset_token_hash").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.expires_at).HasPrecision(0);
            entity.Property(e => e.token_hash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.used_at).HasPrecision(0);

            entity.HasOne(d => d.user).WithMany(p => p.password_reset_tokens)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_password_reset_user");
        });

        modelBuilder.Entity<payment>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__payments__3213E83FAC88260C");

            entity.HasIndex(e => new { e.invoice_id, e.status }, "idx_payments_invoice_status");

            entity.HasIndex(e => e.paid_at, "idx_payments_paid_at");

            entity.HasIndex(e => e.payment_code, "uq_payments_code").IsUnique();

            entity.HasIndex(e => e.transaction_reference, "uq_payments_transaction_reference")
                .IsUnique()
                .HasFilter("([transaction_reference] IS NOT NULL)");

            entity.Property(e => e.amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.method)
                .HasMaxLength(30)
                .HasDefaultValue("bank_transfer");
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.paid_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.payment_code).HasMaxLength(40);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("completed");
            entity.Property(e => e.transaction_reference).HasMaxLength(120);

            entity.HasOne(d => d.invoice).WithMany(p => p.payments)
                .HasForeignKey(d => d.invoice_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payments_invoice");

            entity.HasOne(d => d.received_by_user).WithMany(p => p.payments)
                .HasForeignKey(d => d.received_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_payments_receiver");
        });

        modelBuilder.Entity<payment_refund>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__payment___3213E83FB948D968");

            entity.HasIndex(e => new { e.payment_id, e.status }, "idx_payment_refunds_payment");

            entity.HasIndex(e => e.refund_code, "uq_payment_refunds_code").IsUnique();

            entity.Property(e => e.amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.method)
                .HasMaxLength(30)
                .HasDefaultValue("bank_transfer");
            entity.Property(e => e.processed_at).HasPrecision(0);
            entity.Property(e => e.reason).HasMaxLength(500);
            entity.Property(e => e.refund_code).HasMaxLength(40);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("pending");

            entity.HasOne(d => d.payment).WithMany(p => p.payment_refunds)
                .HasForeignKey(d => d.payment_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_payment_refunds_payment");

            entity.HasOne(d => d.processed_by_user).WithMany(p => p.payment_refunds)
                .HasForeignKey(d => d.processed_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_payment_refunds_user");
        });

        modelBuilder.Entity<permission>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__permissi__3213E83F9D5E8CD0");

            entity.HasIndex(e => e.code, "uq_permissions_code").IsUnique();

            entity.Property(e => e.code).HasMaxLength(100);
            entity.Property(e => e.module).HasMaxLength(80);
            entity.Property(e => e.name).HasMaxLength(150);
        });

        modelBuilder.Entity<registration_bed_offer>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__registra__3213E83F0409E6FD");

            entity.HasIndex(e => new { e.bed_id, e.start_date, e.end_date }, "idx_bed_offers_bed_dates");

            entity.HasIndex(e => new { e.registration_id, e.status }, "idx_bed_offers_registration");

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.expires_at).HasPrecision(0);
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.responded_at).HasPrecision(0);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("offered");

            entity.HasOne(d => d.bed).WithMany(p => p.registration_bed_offers)
                .HasForeignKey(d => d.bed_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bed_offers_bed");

            entity.HasOne(d => d.offered_by_user).WithMany(p => p.registration_bed_offers)
                .HasForeignKey(d => d.offered_by_user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_bed_offers_user");

            entity.HasOne(d => d.registration).WithMany(p => p.registration_bed_offers)
                .HasForeignKey(d => d.registration_id)
                .HasConstraintName("fk_bed_offers_registration");
        });

        modelBuilder.Entity<registration_review>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__registra__3213E83F6D250A91");

            entity.HasIndex(e => new { e.registration_id, e.reviewed_at }, "idx_registration_reviews_registration");

            entity.Property(e => e.comment).HasColumnType("text");
            entity.Property(e => e.decision).HasMaxLength(30);
            entity.Property(e => e.reviewed_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.registration).WithMany(p => p.registration_reviews)
                .HasForeignKey(d => d.registration_id)
                .HasConstraintName("fk_registration_reviews_registration");

            entity.HasOne(d => d.reviewer_user).WithMany(p => p.registration_reviews)
                .HasForeignKey(d => d.reviewer_user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_registration_reviews_user");
        });

        modelBuilder.Entity<repair_request>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__repair_r__3213E83F6F8C22D0");

            entity.HasIndex(e => new { e.room_id, e.reported_at }, "idx_repairs_room");

            entity.HasIndex(e => new { e.status, e.priority }, "idx_repairs_status_priority");

            entity.HasIndex(e => e.request_code, "uq_repairs_code").IsUnique();

            entity.Property(e => e.completed_at).HasPrecision(0);
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.priority)
                .HasMaxLength(30)
                .HasDefaultValue("normal");
            entity.Property(e => e.reported_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.request_code).HasMaxLength(40);
            entity.Property(e => e.resolution_note).HasColumnType("text");
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("submitted");
            entity.Property(e => e.title).HasMaxLength(200);

            entity.HasOne(d => d.assigned_to_user).WithMany(p => p.repair_requestassigned_to_users)
                .HasForeignKey(d => d.assigned_to_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_repairs_assignee");

            entity.HasOne(d => d.reported_by_user).WithMany(p => p.repair_requestreported_by_users)
                .HasForeignKey(d => d.reported_by_user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_repairs_reporter");

            entity.HasOne(d => d.room).WithMany(p => p.repair_requests)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_repairs_room");

            entity.HasOne(d => d.student).WithMany(p => p.repair_requests)
                .HasForeignKey(d => d.student_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_repairs_student");
        });

        modelBuilder.Entity<repair_request_update>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__repair_r__3213E83FC9A16107");

            entity.HasIndex(e => new { e.repair_request_id, e.created_at }, "idx_repair_updates_request_date");

            entity.Property(e => e.comment).HasColumnType("text");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.new_status).HasMaxLength(30);
            entity.Property(e => e.old_status).HasMaxLength(30);

            entity.HasOne(d => d.repair_request).WithMany(p => p.repair_request_updates)
                .HasForeignKey(d => d.repair_request_id)
                .HasConstraintName("fk_repair_updates_request");

            entity.HasOne(d => d.updated_by_user).WithMany(p => p.repair_request_updates)
                .HasForeignKey(d => d.updated_by_user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_repair_updates_user");
        });

        modelBuilder.Entity<role>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__roles__3213E83FD9339C2F");

            entity.HasIndex(e => e.code, "uq_roles_code").IsUnique();

            entity.Property(e => e.code).HasMaxLength(50);
            entity.Property(e => e.description).HasMaxLength(255);
            entity.Property(e => e.name).HasMaxLength(100);

            entity.HasMany(d => d.permissions).WithMany(p => p.roles)
                .UsingEntity<Dictionary<string, object>>(
                    "role_permission",
                    r => r.HasOne<permission>().WithMany()
                        .HasForeignKey("permission_id")
                        .HasConstraintName("fk_role_permissions_permission"),
                    l => l.HasOne<role>().WithMany()
                        .HasForeignKey("role_id")
                        .HasConstraintName("fk_role_permissions_role"),
                    j =>
                    {
                        j.HasKey("role_id", "permission_id").HasName("PK__role_per__C85A5463C06EE317");
                        j.ToTable("role_permissions");
                    });
        });

        modelBuilder.Entity<room>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__rooms__3213E83F8167D869");

            entity.HasIndex(e => e.status, "idx_rooms_status");

            entity.HasIndex(e => new { e.building_id, e.room_number }, "uq_rooms_building_number").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.monthly_price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.room_number).HasMaxLength(30);
            entity.Property(e => e.room_type).HasMaxLength(80);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("available");
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.building).WithMany(p => p.rooms)
                .HasForeignKey(d => d.building_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_rooms_building");
        });

        modelBuilder.Entity<room_asset>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__room_ass__3213E83F8E38EBE2");

            entity.HasIndex(e => new { e.room_id, e.asset_type_id }, "idx_room_assets_room_type");

            entity.HasIndex(e => e.asset_tag, "uq_room_assets_tag")
                .IsUnique()
                .HasFilter("([asset_tag] IS NOT NULL)");

            entity.Property(e => e.asset_tag).HasMaxLength(60);
            entity.Property(e => e.condition_status)
                .HasMaxLength(30)
                .HasDefaultValue("good");
            entity.Property(e => e.last_checked_at).HasPrecision(0);
            entity.Property(e => e.name).HasMaxLength(150);
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.quantity).HasDefaultValue((short)1);

            entity.HasOne(d => d.asset_type).WithMany(p => p.room_assets)
                .HasForeignKey(d => d.asset_type_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_assets_type");

            entity.HasOne(d => d.room).WithMany(p => p.room_assets)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_room_assets_room");
        });

        modelBuilder.Entity<room_registration>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__room_reg__3213E83FF43D4B38");

            entity.HasIndex(e => new { e.requested_start_date, e.requested_end_date }, "idx_registrations_dates");

            entity.HasIndex(e => new { e.student_id, e.status }, "idx_registrations_student_status");

            entity.HasIndex(e => e.registration_code, "uq_registrations_code").IsUnique();

            entity.Property(e => e.note).HasColumnType("text");
            entity.Property(e => e.preferred_room_type).HasMaxLength(80);
            entity.Property(e => e.registration_code).HasMaxLength(40);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("pending");
            entity.Property(e => e.submitted_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.preferred_building).WithMany(p => p.room_registrations)
                .HasForeignKey(d => d.preferred_building_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_registrations_building");

            entity.HasOne(d => d.student).WithMany(p => p.room_registrations)
                .HasForeignKey(d => d.student_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_registrations_student");
        });

        modelBuilder.Entity<rule_acknowledgement>(entity =>
        {
            entity.HasKey(e => new { e.rule_id, e.student_id }).HasName("PK__rule_ack__5B89A2FFB7CE735D");

            entity.Property(e => e.acknowledged_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.rule).WithMany(p => p.rule_acknowledgements)
                .HasForeignKey(d => d.rule_id)
                .HasConstraintName("fk_rule_ack_rule");

            entity.HasOne(d => d.student).WithMany(p => p.rule_acknowledgements)
                .HasForeignKey(d => d.student_id)
                .HasConstraintName("fk_rule_ack_student");
        });

        modelBuilder.Entity<service>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__services__3213E83FE74D8CF8");

            entity.HasIndex(e => e.code, "uq_services_code").IsUnique();

            entity.Property(e => e.code).HasMaxLength(40);
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.name).HasMaxLength(150);
            entity.Property(e => e.unit)
                .HasMaxLength(40)
                .HasDefaultValue("month");
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<service_subscription>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__service___3213E83F6A7CA83C");

            entity.HasIndex(e => new { e.contract_id, e.status }, "idx_service_subscriptions_contract");

            entity.HasIndex(e => new { e.service_id, e.start_date, e.end_date }, "idx_service_subscriptions_service");

            entity.Property(e => e.agreed_unit_price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(12, 3)");
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("active");

            entity.HasOne(d => d.contract).WithMany(p => p.service_subscriptions)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_service_subscriptions_contract");

            entity.HasOne(d => d.service).WithMany(p => p.service_subscriptions)
                .HasForeignKey(d => d.service_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_service_subscriptions_service");
        });

        modelBuilder.Entity<service_usage>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__service___3213E83FC0787023");

            entity.HasIndex(e => new { e.contract_id, e.period_start }, "idx_service_usage_contract_period");

            entity.HasIndex(e => new { e.service_id, e.contract_id, e.period_start, e.period_end }, "uq_service_usage_period").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.quantity)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(12, 3)");
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.contract).WithMany(p => p.service_usages)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_service_usages_contract");

            entity.HasOne(d => d.recorded_by_user).WithMany(p => p.service_usages)
                .HasForeignKey(d => d.recorded_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_service_usages_recorder");

            entity.HasOne(d => d.service).WithMany(p => p.service_usages)
                .HasForeignKey(d => d.service_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_service_usages_service");
        });

        modelBuilder.Entity<student>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__students__3213E83FC05DBE05");

            entity.HasIndex(e => e.student_code, "uq_students_code").IsUnique();

            entity.HasIndex(e => e.user_id, "uq_students_user").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.emergency_contact_name).HasMaxLength(200);
            entity.Property(e => e.emergency_contact_phone).HasMaxLength(30);
            entity.Property(e => e.faculty).HasMaxLength(150);
            entity.Property(e => e.gender)
                .HasMaxLength(30)
                .HasDefaultValue("unspecified");
            entity.Property(e => e.student_code).HasMaxLength(50);

            entity.HasOne(d => d.user).WithOne(p => p.student)
                .HasForeignKey<student>(d => d.user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_students_user");
        });

        modelBuilder.Entity<student_violation>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__student___3213E83FE962BAA3");

            entity.HasIndex(e => new { e.status, e.occurred_at }, "idx_student_violations_status");

            entity.HasIndex(e => new { e.student_id, e.occurred_at }, "idx_student_violations_student_date");

            entity.HasIndex(e => e.violation_code, "uq_student_violations_code").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.description).HasColumnType("text");
            entity.Property(e => e.fine_amount).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.occurred_at).HasPrecision(0);
            entity.Property(e => e.resolution_note).HasColumnType("text");
            entity.Property(e => e.resolved_at).HasPrecision(0);
            entity.Property(e => e.status)
                .HasMaxLength(30)
                .HasDefaultValue("recorded");
            entity.Property(e => e.title).HasMaxLength(200);
            entity.Property(e => e.violation_code).HasMaxLength(40);

            entity.HasOne(d => d.contract).WithMany(p => p.student_violations)
                .HasForeignKey(d => d.contract_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_violations_contract");

            entity.HasOne(d => d.recorded_by_user).WithMany(p => p.student_violations)
                .HasForeignKey(d => d.recorded_by_user_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violations_recorder");

            entity.HasOne(d => d.rule).WithMany(p => p.student_violations)
                .HasForeignKey(d => d.rule_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_violations_rule");

            entity.HasOne(d => d.student).WithMany(p => p.student_violations)
                .HasForeignKey(d => d.student_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violations_student");
        });

        modelBuilder.Entity<user>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__users__3213E83FE7143A54");

            entity.HasIndex(e => e.email, "uq_users_email")
                .IsUnique()
                .HasFilter("([email] IS NOT NULL)");

            entity.HasIndex(e => e.username, "uq_users_username").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.full_name).HasMaxLength(200);
            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.last_login_at).HasPrecision(0);
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.phone).HasMaxLength(30);
            entity.Property(e => e.updated_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.username).HasMaxLength(100);
        });

        modelBuilder.Entity<user_role>(entity =>
        {
            entity.HasKey(e => new { e.user_id, e.role_id }).HasName("PK__user_rol__6EDEA15390E16330");

            entity.Property(e => e.assigned_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.role).WithMany(p => p.user_roles)
                .HasForeignKey(d => d.role_id)
                .HasConstraintName("fk_user_roles_role");

            entity.HasOne(d => d.user).WithMany(p => p.user_roles)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_user_roles_user");
        });

        modelBuilder.Entity<user_session>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__user_ses__3213E83F920D4F62");

            entity.HasIndex(e => new { e.user_id, e.expires_at }, "idx_user_sessions_user_expiry");

            entity.HasIndex(e => e.token_hash, "uq_user_sessions_token_hash").IsUnique();

            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.expires_at).HasPrecision(0);
            entity.Property(e => e.ip_address).HasMaxLength(45);
            entity.Property(e => e.revoked_at).HasPrecision(0);
            entity.Property(e => e.token_hash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.user_agent).HasMaxLength(500);

            entity.HasOne(d => d.user).WithMany(p => p.user_sessions)
                .HasForeignKey(d => d.user_id)
                .HasConstraintName("fk_user_sessions_user");
        });

        modelBuilder.Entity<utility_meter>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__utility___3213E83F17864CFF");

            entity.HasIndex(e => new { e.room_id, e.service_id, e.is_active }, "idx_utility_meters_room_service");

            entity.HasIndex(e => e.meter_code, "uq_utility_meters_code").IsUnique();

            entity.Property(e => e.is_active).HasDefaultValue(true);
            entity.Property(e => e.meter_code).HasMaxLength(50);
            entity.Property(e => e.note).HasMaxLength(500);

            entity.HasOne(d => d.room).WithMany(p => p.utility_meters)
                .HasForeignKey(d => d.room_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_utility_meters_room");

            entity.HasOne(d => d.service).WithMany(p => p.utility_meters)
                .HasForeignKey(d => d.service_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_utility_meters_service");
        });

        modelBuilder.Entity<utility_reading>(entity =>
        {
            entity.HasKey(e => e.id).HasName("PK__utility___3213E83FC49AA295");

            entity.HasIndex(e => e.reading_date, "idx_utility_reading_date");

            entity.HasIndex(e => new { e.meter_id, e.reading_date }, "uq_utility_meter_date").IsUnique();

            entity.HasIndex(e => e.service_usage_id, "uq_utility_usage")
                .IsUnique()
                .HasFilter("([service_usage_id] IS NOT NULL)");

            entity.Property(e => e.consumption).HasColumnType("decimal(14, 3)");
            entity.Property(e => e.created_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.current_value).HasColumnType("decimal(14, 3)");
            entity.Property(e => e.note).HasMaxLength(500);
            entity.Property(e => e.previous_value).HasColumnType("decimal(14, 3)");
            entity.Property(e => e.total_amount).HasColumnType("decimal(14, 2)");
            entity.Property(e => e.unit_price).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.meter).WithMany(p => p.utility_readings)
                .HasForeignKey(d => d.meter_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_utility_reading_meter");

            entity.HasOne(d => d.recorded_by_user).WithMany(p => p.utility_readings)
                .HasForeignKey(d => d.recorded_by_user_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_utility_reading_recorder");

            entity.HasOne(d => d.service_usage).WithOne(p => p.utility_reading)
                .HasForeignKey<utility_reading>(d => d.service_usage_id)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_utility_reading_usage");
        });

        modelBuilder.Entity<v_building_occupancy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_building_occupancy");

            entity.Property(e => e.building_code).HasMaxLength(30);
            entity.Property(e => e.building_name).HasMaxLength(150);
        });

        modelBuilder.Entity<v_current_resident>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_current_residents");

            entity.Property(e => e.bed_number).HasMaxLength(30);
            entity.Property(e => e.building_name).HasMaxLength(150);
            entity.Property(e => e.checked_in_at).HasPrecision(0);
            entity.Property(e => e.contract_code).HasMaxLength(40);
            entity.Property(e => e.full_name).HasMaxLength(200);
            entity.Property(e => e.phone).HasMaxLength(30);
            entity.Property(e => e.room_number).HasMaxLength(30);
            entity.Property(e => e.student_code).HasMaxLength(50);
        });

        modelBuilder.Entity<v_invoice_balance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_invoice_balances");

            entity.Property(e => e.balance_amount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.invoice_code).HasMaxLength(40);
            entity.Property(e => e.paid_amount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.status).HasMaxLength(30);
            entity.Property(e => e.total_amount).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<v_monthly_invoice_summary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_monthly_invoice_summary");

            entity.Property(e => e.billed_amount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.collected_amount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.outstanding_amount).HasColumnType("decimal(38, 2)");
        });

        modelBuilder.Entity<v_registration_summary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_registration_summary");

            entity.Property(e => e.status).HasMaxLength(30);
        });

        modelBuilder.Entity<v_repair_summary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_repair_summary");

            entity.Property(e => e.priority).HasMaxLength(30);
            entity.Property(e => e.status).HasMaxLength(30);
        });

        modelBuilder.Entity<v_room_occupancy>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_room_occupancy");

            entity.Property(e => e.building_code).HasMaxLength(30);
            entity.Property(e => e.building_name).HasMaxLength(150);
            entity.Property(e => e.room_number).HasMaxLength(30);
        });

        modelBuilder.Entity<v_student_outstanding_balance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_student_outstanding_balances");

            entity.Property(e => e.full_name).HasMaxLength(200);
            entity.Property(e => e.outstanding_amount).HasColumnType("decimal(38, 2)");
            entity.Property(e => e.student_code).HasMaxLength(50);
        });

        modelBuilder.Entity<violation_invoice_item>(entity =>
        {
            entity.HasKey(e => e.violation_id).HasName("PK__violatio__8A989363DC17BBA1");

            entity.HasIndex(e => e.invoice_item_id, "uq_violation_invoice_item").IsUnique();

            entity.Property(e => e.violation_id).ValueGeneratedNever();
            entity.Property(e => e.linked_at)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.invoice_item).WithOne(p => p.violation_invoice_item)
                .HasForeignKey<violation_invoice_item>(d => d.invoice_item_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violation_invoice_item");

            entity.HasOne(d => d.violation).WithOne(p => p.violation_invoice_item)
                .HasForeignKey<violation_invoice_item>(d => d.violation_id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_violation_invoice_violation");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
