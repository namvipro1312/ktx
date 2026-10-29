-- SQL Server 2022 schema for the dormitory management system.
-- Creates a new qlktx database if it does not already exist.
IF DB_ID(N'qlktx') IS NULL
    CREATE DATABASE qlktx;
GO
USE qlktx;
GO
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO
CREATE TABLE dbo.users (
  id BIGINT NOT NULL IDENTITY(1,1),
  username NVARCHAR(100) NOT NULL,
  email NVARCHAR(255) NULL,
  password_hash NVARCHAR(255) NOT NULL,
  full_name NVARCHAR(200) NOT NULL,
  phone NVARCHAR(30) NULL,
  is_active BIT NOT NULL DEFAULT (1),
  last_login_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  updated_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_users_username UNIQUE (username)
);
GO
CREATE TABLE dbo.roles (
  id BIGINT NOT NULL IDENTITY(1,1),
  code NVARCHAR(50) NOT NULL,
  name NVARCHAR(100) NOT NULL,
  description NVARCHAR(255) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_roles_code UNIQUE (code)
);
GO
CREATE TABLE dbo.permissions (
  id BIGINT NOT NULL IDENTITY(1,1),
  code NVARCHAR(100) NOT NULL,
  name NVARCHAR(150) NOT NULL,
  module NVARCHAR(80) NOT NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_permissions_code UNIQUE (code)
);
GO
CREATE TABLE dbo.user_roles (
  user_id BIGINT NOT NULL,
  role_id BIGINT NOT NULL,
  assigned_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (user_id, role_id),
  CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
  CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.role_permissions (
  role_id BIGINT NOT NULL,
  permission_id BIGINT NOT NULL,
  PRIMARY KEY (role_id, permission_id),
  CONSTRAINT fk_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
  CONSTRAINT fk_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.user_sessions (
  id BIGINT NOT NULL IDENTITY(1,1),
  user_id BIGINT NOT NULL,
  token_hash CHAR(64) NOT NULL,
  expires_at DATETIME2(0) NOT NULL,
  revoked_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  ip_address NVARCHAR(45) NULL,
  user_agent NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_user_sessions_token_hash UNIQUE (token_hash),
  INDEX idx_user_sessions_user_expiry NONCLUSTERED (user_id, expires_at),
  CONSTRAINT fk_user_sessions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.password_reset_tokens (
  id BIGINT NOT NULL IDENTITY(1,1),
  user_id BIGINT NOT NULL,
  token_hash CHAR(64) NOT NULL,
  expires_at DATETIME2(0) NOT NULL,
  used_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_password_reset_token_hash UNIQUE (token_hash),
  INDEX idx_password_reset_user_expiry NONCLUSTERED (user_id, expires_at),
  CONSTRAINT fk_password_reset_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.login_attempts (
  id BIGINT NOT NULL IDENTITY(1,1),
  username NVARCHAR(100) NOT NULL,
  ip_address NVARCHAR(45) NULL,
  succeeded BIT NOT NULL DEFAULT (0),
  attempted_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_login_attempts_username_time NONCLUSTERED (username, attempted_at),
  INDEX idx_login_attempts_ip_time NONCLUSTERED (ip_address, attempted_at)
);
GO
CREATE TABLE dbo.students (
  id BIGINT NOT NULL IDENTITY(1,1),
  user_id BIGINT NOT NULL,
  student_code NVARCHAR(50) NOT NULL,
  date_of_birth DATE NULL,
  gender NVARCHAR(30) NOT NULL DEFAULT 'unspecified' CONSTRAINT chk_students_gender CHECK (gender IN (N'male', N'female', N'other', N'unspecified')),
  faculty NVARCHAR(150) NULL,
  course_year SMALLINT NULL,
  emergency_contact_name NVARCHAR(200) NULL,
  emergency_contact_phone NVARCHAR(30) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_students_user UNIQUE (user_id),
  CONSTRAINT uq_students_code UNIQUE (student_code),
  CONSTRAINT fk_students_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.buildings (
  id BIGINT NOT NULL IDENTITY(1,1),
  code NVARCHAR(30) NOT NULL,
  name NVARCHAR(150) NOT NULL,
  address NVARCHAR(255) NULL,
  description TEXT NULL,
  is_active BIT NOT NULL DEFAULT (1),
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_buildings_code UNIQUE (code)
);
GO
CREATE TABLE dbo.rooms (
  id BIGINT NOT NULL IDENTITY(1,1),
  building_id BIGINT NOT NULL,
  room_number NVARCHAR(30) NOT NULL,
  floor_number SMALLINT NULL,
  room_type NVARCHAR(80) NULL,
  capacity SMALLINT NOT NULL,
  monthly_price DECIMAL(12,2) NOT NULL DEFAULT 0,
  status NVARCHAR(30) NOT NULL DEFAULT 'available' CONSTRAINT chk_rooms_status CHECK (status IN (N'available', N'full', N'maintenance', N'inactive')),
  description TEXT NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  updated_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_rooms_building_number UNIQUE (building_id, room_number),
  INDEX idx_rooms_status NONCLUSTERED (status),
  CONSTRAINT fk_rooms_building FOREIGN KEY (building_id) REFERENCES buildings(id) ON DELETE NO ACTION,
  CONSTRAINT chk_rooms_capacity CHECK (capacity > 0),
  CONSTRAINT chk_rooms_price CHECK (monthly_price >= 0)
);
GO
CREATE TABLE dbo.beds (
  id BIGINT NOT NULL IDENTITY(1,1),
  room_id BIGINT NOT NULL,
  bed_number NVARCHAR(30) NOT NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'available' CONSTRAINT chk_beds_status CHECK (status IN (N'available', N'occupied', N'maintenance', N'inactive')),
  PRIMARY KEY (id),
  CONSTRAINT uq_beds_room_number UNIQUE (room_id, bed_number),
  INDEX idx_beds_room_status NONCLUSTERED (room_id, status),
  CONSTRAINT fk_beds_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.room_registrations (
  id BIGINT NOT NULL IDENTITY(1,1),
  registration_code NVARCHAR(40) NOT NULL,
  student_id BIGINT NOT NULL,
  preferred_building_id BIGINT NULL,
  preferred_room_type NVARCHAR(80) NULL,
  requested_start_date DATE NOT NULL,
  requested_end_date DATE NOT NULL,
  submitted_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  status NVARCHAR(30) NOT NULL DEFAULT 'pending' CONSTRAINT chk_room_registrations_status CHECK (status IN (N'pending', N'approved', N'rejected', N'cancelled', N'contracted')),
  note TEXT NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_registrations_code UNIQUE (registration_code),
  INDEX idx_registrations_student_status NONCLUSTERED (student_id, status),
  INDEX idx_registrations_dates NONCLUSTERED (requested_start_date, requested_end_date),
  CONSTRAINT fk_registrations_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE NO ACTION,
  CONSTRAINT fk_registrations_building FOREIGN KEY (preferred_building_id) REFERENCES buildings(id) ON DELETE SET NULL,
  CONSTRAINT chk_registration_dates CHECK (requested_end_date > requested_start_date)
);
GO
CREATE TABLE dbo.registration_reviews (
  id BIGINT NOT NULL IDENTITY(1,1),
  registration_id BIGINT NOT NULL,
  reviewer_user_id BIGINT NOT NULL,
  decision NVARCHAR(30) NOT NULL CONSTRAINT chk_registration_reviews_decision CHECK (decision IN (N'approved', N'rejected', N'requested_changes')),
  comment TEXT NULL,
  reviewed_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_registration_reviews_registration NONCLUSTERED (registration_id, reviewed_at),
  CONSTRAINT fk_registration_reviews_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE CASCADE,
  CONSTRAINT fk_registration_reviews_user FOREIGN KEY (reviewer_user_id) REFERENCES users(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.registration_bed_offers (
  id BIGINT NOT NULL IDENTITY(1,1),
  registration_id BIGINT NOT NULL,
  bed_id BIGINT NOT NULL,
  offered_by_user_id BIGINT NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'offered' CONSTRAINT chk_registration_bed_offers_status CHECK (status IN (N'offered', N'accepted', N'declined', N'expired', N'withdrawn')),
  expires_at DATETIME2(0) NULL,
  responded_at DATETIME2(0) NULL,
  note NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_bed_offers_registration NONCLUSTERED (registration_id, status),
  INDEX idx_bed_offers_bed_dates NONCLUSTERED (bed_id, start_date, end_date),
  CONSTRAINT fk_bed_offers_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE CASCADE,
  CONSTRAINT fk_bed_offers_bed FOREIGN KEY (bed_id) REFERENCES beds(id) ON DELETE NO ACTION,
  CONSTRAINT fk_bed_offers_user FOREIGN KEY (offered_by_user_id) REFERENCES users(id) ON DELETE NO ACTION,
  CONSTRAINT chk_bed_offer_dates CHECK (end_date > start_date)
);
GO
CREATE TABLE dbo.contracts (
  id BIGINT NOT NULL IDENTITY(1,1),
  contract_code NVARCHAR(40) NOT NULL,
  registration_id BIGINT NULL,
  student_id BIGINT NOT NULL,
  bed_id BIGINT NOT NULL,
  created_by_user_id BIGINT NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  monthly_price DECIMAL(12,2) NOT NULL,
  deposit_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
  status NVARCHAR(30) NOT NULL DEFAULT 'draft' CONSTRAINT chk_contracts_status CHECK (status IN (N'draft', N'active', N'expired', N'terminated', N'cancelled')),
  signed_at DATETIME2(0) NULL,
  terms TEXT NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_contracts_code UNIQUE (contract_code),
  INDEX idx_contracts_student_status NONCLUSTERED (student_id, status),
  INDEX idx_contracts_bed_dates NONCLUSTERED (bed_id, start_date, end_date),
  CONSTRAINT fk_contracts_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE SET NULL,
  CONSTRAINT fk_contracts_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE NO ACTION,
  CONSTRAINT fk_contracts_bed FOREIGN KEY (bed_id) REFERENCES beds(id) ON DELETE NO ACTION,
  CONSTRAINT fk_contracts_creator FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_contract_dates CHECK (end_date > start_date),
  CONSTRAINT chk_contract_prices CHECK (monthly_price >= 0 AND deposit_amount >= 0)
);
GO
CREATE TABLE dbo.contract_amendments (
  id BIGINT NOT NULL IDENTITY(1,1),
  contract_id BIGINT NOT NULL,
  amendment_code NVARCHAR(40) NOT NULL,
  amendment_type NVARCHAR(30) NOT NULL CONSTRAINT chk_contract_amendments_amendment_type CHECK (amendment_type IN (N'renewal', N'price_change', N'date_change', N'bed_change', N'other')),
  old_values NVARCHAR(MAX) NULL,
  new_values NVARCHAR(MAX) NULL,
  reason TEXT NULL,
  effective_date DATE NOT NULL,
  created_by_user_id BIGINT NULL,
  signed_at DATETIME2(0) NULL,
  document_path NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_contract_amendment_code UNIQUE (amendment_code),
  INDEX idx_contract_amendments_contract NONCLUSTERED (contract_id, effective_date),
  CONSTRAINT fk_contract_amendments_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT fk_contract_amendments_user FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO
CREATE TABLE dbo.occupancy_records (
  id BIGINT NOT NULL IDENTITY(1,1),
  contract_id BIGINT NOT NULL,
  checked_in_at DATETIME2(0) NULL,
  checked_in_by_user_id BIGINT NULL,
  checkin_condition NVARCHAR(MAX) NULL,
  checkin_note TEXT NULL,
  expected_checkout_date DATE NOT NULL,
  checked_out_at DATETIME2(0) NULL,
  checked_out_by_user_id BIGINT NULL,
  checkout_condition NVARCHAR(MAX) NULL,
  checkout_note TEXT NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_occupancy_contract UNIQUE (contract_id),
  INDEX idx_occupancy_checkout NONCLUSTERED (checked_out_at, expected_checkout_date),
  CONSTRAINT fk_occupancy_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT fk_occupancy_checkin_user FOREIGN KEY (checked_in_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT fk_occupancy_checkout_user FOREIGN KEY (checked_out_by_user_id) REFERENCES users(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.deposit_transactions (
  id BIGINT NOT NULL IDENTITY(1,1),
  contract_id BIGINT NOT NULL,
  transaction_code NVARCHAR(40) NOT NULL,
  transaction_type NVARCHAR(30) NOT NULL CONSTRAINT chk_deposit_transactions_transaction_type CHECK (transaction_type IN (N'collected', N'refunded', N'deducted')),
  amount DECIMAL(14,2) NOT NULL,
  method NVARCHAR(30) NOT NULL DEFAULT 'bank_transfer' CONSTRAINT chk_deposit_transactions_method CHECK (method IN (N'cash', N'bank_transfer', N'online', N'other')),
  status NVARCHAR(30) NOT NULL DEFAULT 'completed' CONSTRAINT chk_deposit_transactions_status CHECK (status IN (N'pending', N'completed', N'cancelled')),
  transaction_date DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  handled_by_user_id BIGINT NULL,
  reason NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_deposit_transaction_code UNIQUE (transaction_code),
  INDEX idx_deposit_contract_date NONCLUSTERED (contract_id, transaction_date),
  CONSTRAINT fk_deposit_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT fk_deposit_handler FOREIGN KEY (handled_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_deposit_amount CHECK (amount > 0)
);
GO
CREATE TABLE dbo.services (
  id BIGINT NOT NULL IDENTITY(1,1),
  code NVARCHAR(40) NOT NULL,
  name NVARCHAR(150) NOT NULL,
  unit NVARCHAR(40) NOT NULL DEFAULT 'month',
  unit_price DECIMAL(12,2) NOT NULL DEFAULT 0,
  is_metered BIT NOT NULL DEFAULT (0),
  is_active BIT NOT NULL DEFAULT (1),
  description TEXT NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_services_code UNIQUE (code),
  CONSTRAINT chk_services_price CHECK (unit_price >= 0)
);
GO
CREATE TABLE dbo.service_usages (
  id BIGINT NOT NULL IDENTITY(1,1),
  service_id BIGINT NOT NULL,
  contract_id BIGINT NOT NULL,
  period_start DATE NOT NULL,
  period_end DATE NOT NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  unit_price DECIMAL(12,2) NOT NULL,
  recorded_by_user_id BIGINT NULL,
  note NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_service_usage_period UNIQUE (service_id, contract_id, period_start, period_end),
  INDEX idx_service_usage_contract_period NONCLUSTERED (contract_id, period_start),
  CONSTRAINT fk_service_usages_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE NO ACTION,
  CONSTRAINT fk_service_usages_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT fk_service_usages_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_service_usage_dates CHECK (period_end > period_start),
  CONSTRAINT chk_service_usage_values CHECK (quantity >= 0 AND unit_price >= 0)
);
GO
CREATE TABLE dbo.utility_meters (
  id BIGINT NOT NULL IDENTITY(1,1),
  room_id BIGINT NOT NULL,
  service_id BIGINT NOT NULL,
  meter_code NVARCHAR(50) NOT NULL,
  installed_at DATE NULL,
  is_active BIT NOT NULL DEFAULT (1),
  note NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_utility_meters_code UNIQUE (meter_code),
  INDEX idx_utility_meters_room_service NONCLUSTERED (room_id, service_id, is_active),
  CONSTRAINT fk_utility_meters_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE NO ACTION,
  CONSTRAINT fk_utility_meters_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.utility_readings (
  id BIGINT NOT NULL IDENTITY(1,1),
  meter_id BIGINT NOT NULL,
  reading_date DATE NOT NULL,
  previous_value DECIMAL(14,3) NOT NULL,
  current_value DECIMAL(14,3) NOT NULL,
  consumption DECIMAL(14,3) NOT NULL,
  unit_price DECIMAL(12,2) NOT NULL,
  total_amount DECIMAL(14,2) NOT NULL,
  recorded_by_user_id BIGINT NULL,
  service_usage_id BIGINT NULL,
  note NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_utility_meter_date UNIQUE (meter_id, reading_date),
  INDEX idx_utility_reading_date NONCLUSTERED (reading_date),
  CONSTRAINT fk_utility_reading_meter FOREIGN KEY (meter_id) REFERENCES utility_meters(id) ON DELETE NO ACTION,
  CONSTRAINT fk_utility_reading_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT fk_utility_reading_usage FOREIGN KEY (service_usage_id) REFERENCES service_usages(id) ON DELETE SET NULL,
  CONSTRAINT chk_utility_reading_values CHECK (previous_value >= 0 AND current_value >= previous_value AND consumption >= 0 AND unit_price >= 0 AND total_amount >= 0)
);
GO
CREATE TABLE dbo.service_subscriptions (
  id BIGINT NOT NULL IDENTITY(1,1),
  contract_id BIGINT NOT NULL,
  service_id BIGINT NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  agreed_unit_price DECIMAL(12,2) NOT NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'active' CONSTRAINT chk_service_subscriptions_status CHECK (status IN (N'active', N'paused', N'ended')),
  note NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  INDEX idx_service_subscriptions_contract NONCLUSTERED (contract_id, status),
  INDEX idx_service_subscriptions_service NONCLUSTERED (service_id, start_date, end_date),
  CONSTRAINT fk_service_subscriptions_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT fk_service_subscriptions_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE NO ACTION,
  CONSTRAINT chk_service_subscription_dates CHECK (end_date IS NULL OR end_date >= start_date),
  CONSTRAINT chk_service_subscription_amounts CHECK (quantity > 0 AND agreed_unit_price >= 0)
);
GO
CREATE TABLE dbo.repair_requests (
  id BIGINT NOT NULL IDENTITY(1,1),
  request_code NVARCHAR(40) NOT NULL,
  student_id BIGINT NULL,
  room_id BIGINT NOT NULL,
  reported_by_user_id BIGINT NOT NULL,
  assigned_to_user_id BIGINT NULL,
  title NVARCHAR(200) NOT NULL,
  description TEXT NOT NULL,
  priority NVARCHAR(30) NOT NULL DEFAULT 'normal' CONSTRAINT chk_repair_requests_priority CHECK (priority IN (N'low', N'normal', N'high', N'urgent')),
  status NVARCHAR(30) NOT NULL DEFAULT 'submitted' CONSTRAINT chk_repair_requests_status CHECK (status IN (N'submitted', N'accepted', N'in_progress', N'completed', N'rejected', N'cancelled')),
  reported_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  completed_at DATETIME2(0) NULL,
  resolution_note TEXT NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_repairs_code UNIQUE (request_code),
  INDEX idx_repairs_status_priority NONCLUSTERED (status, priority),
  INDEX idx_repairs_room NONCLUSTERED (room_id, reported_at),
  CONSTRAINT fk_repairs_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE SET NULL,
  CONSTRAINT fk_repairs_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE NO ACTION,
  CONSTRAINT fk_repairs_reporter FOREIGN KEY (reported_by_user_id) REFERENCES users(id) ON DELETE NO ACTION,
  CONSTRAINT fk_repairs_assignee FOREIGN KEY (assigned_to_user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO
CREATE TABLE dbo.repair_request_updates (
  id BIGINT NOT NULL IDENTITY(1,1),
  repair_request_id BIGINT NOT NULL,
  updated_by_user_id BIGINT NOT NULL,
  old_status NVARCHAR(30) NULL CONSTRAINT chk_repair_request_updates_old_status CHECK (old_status IN (N'submitted', N'accepted', N'in_progress', N'completed', N'rejected', N'cancelled')),
  new_status NVARCHAR(30) NULL CONSTRAINT chk_repair_request_updates_new_status CHECK (new_status IN (N'submitted', N'accepted', N'in_progress', N'completed', N'rejected', N'cancelled')),
  comment TEXT NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_repair_updates_request_date NONCLUSTERED (repair_request_id, created_at),
  CONSTRAINT fk_repair_updates_request FOREIGN KEY (repair_request_id) REFERENCES repair_requests(id) ON DELETE CASCADE,
  CONSTRAINT fk_repair_updates_user FOREIGN KEY (updated_by_user_id) REFERENCES users(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.asset_types (
  id BIGINT NOT NULL IDENTITY(1,1),
  code NVARCHAR(40) NOT NULL,
  name NVARCHAR(120) NOT NULL,
  description NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_asset_types_code UNIQUE (code)
);
GO
CREATE TABLE dbo.room_assets (
  id BIGINT NOT NULL IDENTITY(1,1),
  room_id BIGINT NOT NULL,
  asset_type_id BIGINT NOT NULL,
  asset_tag NVARCHAR(60) NULL,
  name NVARCHAR(150) NOT NULL,
  quantity SMALLINT NOT NULL DEFAULT 1,
  condition_status NVARCHAR(30) NOT NULL DEFAULT 'good' CONSTRAINT chk_room_assets_condition_status CHECK (condition_status IN (N'new', N'good', N'worn', N'damaged', N'disposed')),
  acquired_at DATE NULL,
  last_checked_at DATETIME2(0) NULL,
  note NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  INDEX idx_room_assets_room_type NONCLUSTERED (room_id, asset_type_id),
  CONSTRAINT fk_room_assets_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE NO ACTION,
  CONSTRAINT fk_room_assets_type FOREIGN KEY (asset_type_id) REFERENCES asset_types(id) ON DELETE NO ACTION,
  CONSTRAINT chk_room_asset_quantity CHECK (quantity > 0)
);
GO
CREATE TABLE dbo.dormitory_rules (
  id BIGINT NOT NULL IDENTITY(1,1),
  title NVARCHAR(200) NOT NULL,
  content TEXT NOT NULL,
  version NVARCHAR(30) NOT NULL,
  effective_from DATE NOT NULL,
  effective_to DATE NULL,
  is_active BIT NOT NULL DEFAULT (1),
  created_by_user_id BIGINT NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_rules_title_version UNIQUE (title, version),
  INDEX idx_rules_active_dates NONCLUSTERED (is_active, effective_from, effective_to),
  CONSTRAINT fk_rules_creator FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_rule_dates CHECK (effective_to IS NULL OR effective_to >= effective_from)
);
GO
CREATE TABLE dbo.rule_acknowledgements (
  rule_id BIGINT NOT NULL,
  student_id BIGINT NOT NULL,
  acknowledged_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (rule_id, student_id),
  CONSTRAINT fk_rule_ack_rule FOREIGN KEY (rule_id) REFERENCES dormitory_rules(id) ON DELETE CASCADE,
  CONSTRAINT fk_rule_ack_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.guest_visits (
  id BIGINT NOT NULL IDENTITY(1,1),
  student_id BIGINT NOT NULL,
  guest_name NVARCHAR(200) NOT NULL,
  guest_identity_no NVARCHAR(80) NULL,
  guest_phone NVARCHAR(30) NULL,
  visit_purpose NVARCHAR(500) NULL,
  expected_arrival DATETIME2(0) NOT NULL,
  expected_departure DATETIME2(0) NULL,
  actual_arrival DATETIME2(0) NULL,
  actual_departure DATETIME2(0) NULL,
  approved_by_user_id BIGINT NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'pending' CONSTRAINT chk_guest_visits_status CHECK (status IN (N'pending', N'approved', N'rejected', N'checked_in', N'completed', N'cancelled')),
  note NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_guest_visits_student_date NONCLUSTERED (student_id, expected_arrival),
  INDEX idx_guest_visits_status NONCLUSTERED (status, expected_arrival),
  CONSTRAINT fk_guest_visits_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE NO ACTION,
  CONSTRAINT fk_guest_visits_approver FOREIGN KEY (approved_by_user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO
CREATE TABLE dbo.invoices (
  id BIGINT NOT NULL IDENTITY(1,1),
  invoice_code NVARCHAR(40) NOT NULL,
  contract_id BIGINT NOT NULL,
  period_start DATE NOT NULL,
  period_end DATE NOT NULL,
  issued_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  due_date DATE NOT NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'draft' CONSTRAINT chk_invoices_status CHECK (status IN (N'draft', N'issued', N'partially_paid', N'paid', N'overdue', N'void')),
  note NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_invoices_code UNIQUE (invoice_code),
  CONSTRAINT uq_invoice_contract_period UNIQUE (contract_id, period_start, period_end),
  INDEX idx_invoices_status_due NONCLUSTERED (status, due_date),
  CONSTRAINT fk_invoices_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE NO ACTION,
  CONSTRAINT chk_invoice_dates CHECK (period_end > period_start)
);
GO
CREATE TABLE dbo.invoice_items (
  id BIGINT NOT NULL IDENTITY(1,1),
  invoice_id BIGINT NOT NULL,
  item_type NVARCHAR(30) NOT NULL CONSTRAINT chk_invoice_items_item_type CHECK (item_type IN (N'room_rent', N'service', N'deposit', N'fine', N'other')),
  service_usage_id BIGINT NULL,
  description NVARCHAR(255) NOT NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  unit_price DECIMAL(12,2) NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  PRIMARY KEY (id),
  INDEX idx_invoice_items_invoice NONCLUSTERED (invoice_id),
  CONSTRAINT fk_invoice_items_invoice FOREIGN KEY (invoice_id) REFERENCES invoices(id) ON DELETE CASCADE,
  CONSTRAINT fk_invoice_items_usage FOREIGN KEY (service_usage_id) REFERENCES service_usages(id) ON DELETE SET NULL,
  CONSTRAINT chk_invoice_item_values CHECK (quantity >= 0 AND unit_price >= 0 AND amount >= 0)
);
GO
CREATE TABLE dbo.payments (
  id BIGINT NOT NULL IDENTITY(1,1),
  payment_code NVARCHAR(40) NOT NULL,
  invoice_id BIGINT NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  method NVARCHAR(30) NOT NULL DEFAULT 'bank_transfer' CONSTRAINT chk_payments_method CHECK (method IN (N'cash', N'bank_transfer', N'online', N'other')),
  status NVARCHAR(30) NOT NULL DEFAULT 'completed' CONSTRAINT chk_payments_status CHECK (status IN (N'pending', N'completed', N'failed', N'refunded')),
  transaction_reference NVARCHAR(120) NULL,
  received_by_user_id BIGINT NULL,
  paid_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  note NVARCHAR(500) NULL,
  PRIMARY KEY (id),
  CONSTRAINT uq_payments_code UNIQUE (payment_code),
  INDEX idx_payments_invoice_status NONCLUSTERED (invoice_id, status),
  INDEX idx_payments_paid_at NONCLUSTERED (paid_at),
  CONSTRAINT fk_payments_invoice FOREIGN KEY (invoice_id) REFERENCES invoices(id) ON DELETE NO ACTION,
  CONSTRAINT fk_payments_receiver FOREIGN KEY (received_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_payment_amount CHECK (amount > 0)
);
GO
CREATE TABLE dbo.payment_refunds (
  id BIGINT NOT NULL IDENTITY(1,1),
  payment_id BIGINT NOT NULL,
  refund_code NVARCHAR(40) NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  reason NVARCHAR(500) NULL,
  status NVARCHAR(30) NOT NULL DEFAULT 'pending' CONSTRAINT chk_payment_refunds_status CHECK (status IN (N'pending', N'completed', N'rejected')),
  method NVARCHAR(30) NOT NULL DEFAULT 'bank_transfer' CONSTRAINT chk_payment_refunds_method CHECK (method IN (N'cash', N'bank_transfer', N'online', N'other')),
  processed_by_user_id BIGINT NULL,
  processed_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_payment_refunds_code UNIQUE (refund_code),
  INDEX idx_payment_refunds_payment NONCLUSTERED (payment_id, status),
  CONSTRAINT fk_payment_refunds_payment FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE NO ACTION,
  CONSTRAINT fk_payment_refunds_user FOREIGN KEY (processed_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_payment_refund_amount CHECK (amount > 0)
);
GO
CREATE TABLE dbo.student_violations (
  id BIGINT NOT NULL IDENTITY(1,1),
  student_id BIGINT NOT NULL,
  contract_id BIGINT NULL,
  rule_id BIGINT NULL,
  recorded_by_user_id BIGINT NOT NULL,
  violation_code NVARCHAR(40) NOT NULL,
  title NVARCHAR(200) NOT NULL,
  description TEXT NULL,
  occurred_at DATETIME2(0) NOT NULL,
  fine_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
  status NVARCHAR(30) NOT NULL DEFAULT 'recorded' CONSTRAINT chk_student_violations_status CHECK (status IN (N'recorded', N'under_review', N'resolved', N'appealed', N'cancelled')),
  resolution_note TEXT NULL,
  resolved_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  CONSTRAINT uq_student_violations_code UNIQUE (violation_code),
  INDEX idx_student_violations_student_date NONCLUSTERED (student_id, occurred_at),
  INDEX idx_student_violations_status NONCLUSTERED (status, occurred_at),
  CONSTRAINT fk_violations_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE NO ACTION,
  CONSTRAINT fk_violations_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE SET NULL,
  CONSTRAINT fk_violations_rule FOREIGN KEY (rule_id) REFERENCES dormitory_rules(id) ON DELETE SET NULL,
  CONSTRAINT fk_violations_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE NO ACTION,
  CONSTRAINT chk_violation_fine CHECK (fine_amount >= 0)
);
GO
CREATE TABLE dbo.violation_invoice_items (
  violation_id BIGINT NOT NULL,
  invoice_item_id BIGINT NOT NULL,
  linked_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (violation_id),
  CONSTRAINT uq_violation_invoice_item UNIQUE (invoice_item_id),
  CONSTRAINT fk_violation_invoice_violation FOREIGN KEY (violation_id) REFERENCES student_violations(id) ON DELETE NO ACTION,
  CONSTRAINT fk_violation_invoice_item FOREIGN KEY (invoice_item_id) REFERENCES invoice_items(id) ON DELETE NO ACTION
);
GO
CREATE TABLE dbo.documents (
  id BIGINT NOT NULL IDENTITY(1,1),
  entity_type NVARCHAR(30) NOT NULL CONSTRAINT chk_documents_entity_type CHECK (entity_type IN (N'student', N'registration', N'contract', N'repair', N'invoice', N'violation', N'other')),
  entity_id BIGINT NOT NULL,
  uploaded_by_user_id BIGINT NULL,
  original_name NVARCHAR(255) NOT NULL,
  storage_key NVARCHAR(500) NOT NULL,
  mime_type NVARCHAR(150) NULL,
  size_bytes BIGINT NULL,
  checksum_sha256 CHAR(64) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_documents_entity NONCLUSTERED (entity_type, entity_id),
  CONSTRAINT fk_documents_uploader FOREIGN KEY (uploaded_by_user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO
CREATE TABLE dbo.notifications (
  id BIGINT NOT NULL IDENTITY(1,1),
  user_id BIGINT NOT NULL,
  title NVARCHAR(200) NOT NULL,
  message TEXT NOT NULL,
  notification_type NVARCHAR(60) NOT NULL DEFAULT 'general',
  entity_type NVARCHAR(60) NULL,
  entity_id BIGINT NULL,
  read_at DATETIME2(0) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_notifications_user_unread NONCLUSTERED (user_id, read_at, created_at),
  CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);
GO
CREATE TABLE dbo.audit_logs (
  id BIGINT NOT NULL IDENTITY(1,1),
  user_id BIGINT NULL,
  action NVARCHAR(100) NOT NULL,
  entity_type NVARCHAR(80) NOT NULL,
  entity_id BIGINT NULL,
  old_values NVARCHAR(MAX) NULL,
  new_values NVARCHAR(MAX) NULL,
  ip_address NVARCHAR(45) NULL,
  user_agent NVARCHAR(500) NULL,
  created_at DATETIME2(0) NOT NULL DEFAULT SYSDATETIME(),
  PRIMARY KEY (id),
  INDEX idx_audit_entity_date NONCLUSTERED (entity_type, entity_id, created_at),
  INDEX idx_audit_user_date NONCLUSTERED (user_id, created_at),
  CONSTRAINT fk_audit_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
);
GO
CREATE UNIQUE NONCLUSTERED INDEX uq_users_email ON dbo.users (email) WHERE email IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX uq_contracts_registration ON dbo.contracts (registration_id) WHERE registration_id IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX uq_utility_usage ON dbo.utility_readings (service_usage_id) WHERE service_usage_id IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX uq_room_assets_tag ON dbo.room_assets (asset_tag) WHERE asset_tag IS NOT NULL;
CREATE UNIQUE NONCLUSTERED INDEX uq_payments_transaction_reference ON dbo.payments (transaction_reference) WHERE transaction_reference IS NOT NULL;


















IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE code = 'system_admin')
    INSERT INTO dbo.roles (code, name, description) VALUES ('system_admin', N'Quản trị hệ thống', N'Quản lý tài khoản và cấu hình hệ thống');
IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE code = 'dorm_manager')
    INSERT INTO dbo.roles (code, name, description) VALUES ('dorm_manager', N'Quản lý ký túc xá', N'Quản lý vận hành ký túc xá');
IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE code = 'student')
    INSERT INTO dbo.roles (code, name, description) VALUES ('student', N'Sinh viên', N'Tài khoản sinh viên nội trú');
IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE code = 'maintenance_staff')
    INSERT INTO dbo.roles (code, name, description) VALUES ('maintenance_staff', N'Nhân viên sửa chữa', N'Tiếp nhận và xử lý yêu cầu sửa chữa');
IF NOT EXISTS (SELECT 1 FROM dbo.roles WHERE code = 'accountant')
    INSERT INTO dbo.roles (code, name, description) VALUES ('accountant', N'Kế toán', N'Quản lý hóa đơn và thanh toán');

IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'users.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('users.manage', N'Quản lý tài khoản', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'roles.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('roles.manage', N'Quản lý vai trò và quyền', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'buildings.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('buildings.manage', N'Quản lý tòa nhà', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'rooms.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('rooms.manage', N'Quản lý phòng', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'beds.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('beds.manage', N'Quản lý giường', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'registrations.create')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('registrations.create', N'Tạo đăng ký phòng', 'sp2');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'registrations.review')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('registrations.review', N'Duyệt đăng ký phòng', 'sp2');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'contracts.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('contracts.manage', N'Quản lý hợp đồng', 'sp3');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'services.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('services.manage', N'Quản lý dịch vụ', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'repairs.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('repairs.manage', N'Quản lý sửa chữa', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'rules.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('rules.manage', N'Quản lý nội quy', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'invoices.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('invoices.manage', N'Quản lý hóa đơn', 'sp5');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'payments.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('payments.manage', N'Quản lý thanh toán', 'sp5');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'reports.view')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('reports.view', N'Xem thống kê', 'sp5');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'checkin.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('checkin.manage', N'Quản lý nhận và trả phòng', 'sp3');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'utilities.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('utilities.manage', N'Quản lý điện nước', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'assets.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('assets.manage', N'Quản lý tài sản', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'violations.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('violations.manage', N'Quản lý vi phạm', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'guests.manage')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('guests.manage', N'Quản lý khách thăm', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'audit.view')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('audit.view', N'Xem nhật ký thao tác', 'sp1');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'repairs.create')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('repairs.create', N'Gửi yêu cầu sửa chữa', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'rules.view')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('rules.view', N'Xem nội quy', 'sp4');
IF NOT EXISTS (SELECT 1 FROM dbo.permissions WHERE code = 'guests.request')
    INSERT INTO dbo.permissions (code, name, module) VALUES ('guests.request', N'Đăng ký khách thăm', 'sp4');


INSERT INTO dbo.role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM dbo.roles r CROSS JOIN dbo.permissions p
WHERE r.code = 'system_admin'
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);

INSERT INTO dbo.role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM dbo.roles r JOIN dbo.permissions p ON 1 = 1
WHERE r.code = 'dorm_manager'
  AND p.code IN ('buildings.manage','rooms.manage','beds.manage','registrations.review',
                 'contracts.manage','checkin.manage','services.manage','utilities.manage',
                 'repairs.manage','rules.manage','assets.manage','violations.manage',
                 'guests.manage','reports.view','audit.view')
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);

INSERT INTO dbo.role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM dbo.roles r JOIN dbo.permissions p ON 1 = 1
WHERE r.code = 'student'
  AND p.code IN ('registrations.create','repairs.create','rules.view','guests.request')
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);

INSERT INTO dbo.role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM dbo.roles r JOIN dbo.permissions p ON 1 = 1
WHERE r.code = 'maintenance_staff'
  AND p.code IN ('repairs.manage','assets.manage')
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);

INSERT INTO dbo.role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM dbo.roles r JOIN dbo.permissions p ON 1 = 1
WHERE r.code = 'accountant'
  AND p.code IN ('invoices.manage','payments.manage','reports.view')
  AND NOT EXISTS (SELECT 1 FROM dbo.role_permissions rp WHERE rp.role_id = r.id AND rp.permission_id = p.id);






































































GO
CREATE OR ALTER VIEW v_invoice_balances AS
SELECT i.id AS invoice_id,
       i.invoice_code,
       i.contract_id,
       i.period_start,
       i.period_end,
       i.due_date,
       i.status,
       COALESCE(items.total_amount, 0) AS total_amount,
       COALESCE(paid.paid_amount, 0) AS paid_amount,
       COALESCE(items.total_amount, 0) - COALESCE(paid.paid_amount, 0) AS balance_amount
FROM invoices i
LEFT JOIN (
  SELECT invoice_id, SUM(amount) AS total_amount
  FROM invoice_items
  GROUP BY invoice_id
) items ON items.invoice_id = i.id
LEFT JOIN (
  SELECT p.invoice_id,
         SUM(p.amount - COALESCE(r.refunded_amount, 0)) AS paid_amount
  FROM payments p
  LEFT JOIN (
    SELECT payment_id, SUM(amount) AS refunded_amount
    FROM payment_refunds
    WHERE status = 'completed'
    GROUP BY payment_id
  ) r ON r.payment_id = p.id
  WHERE p.status = 'completed'
  GROUP BY p.invoice_id
) paid ON paid.invoice_id = i.id;

GO
CREATE OR ALTER VIEW v_room_occupancy AS
SELECT b.id AS building_id,
       b.code AS building_code,
       b.name AS building_name,
       r.id AS room_id,
       r.room_number,
       r.capacity,
       COUNT(DISTINCT bd.id) AS configured_beds,
       COUNT(DISTINCT c.id) AS active_contracts,
       GREATEST(LEAST(r.capacity, COUNT(DISTINCT bd.id)) - COUNT(DISTINCT c.id), 0) AS available_slots
FROM buildings b
JOIN rooms r ON r.building_id = b.id
LEFT JOIN beds bd ON bd.room_id = r.id
LEFT JOIN contracts c ON c.bed_id = bd.id
  AND c.status = 'active'
  AND CONVERT(date, SYSDATETIME()) >= c.start_date
  AND CONVERT(date, SYSDATETIME()) < c.end_date
GROUP BY b.id, b.code, b.name, r.id, r.room_number, r.capacity;

GO
CREATE OR ALTER VIEW v_building_occupancy AS
SELECT building_id,
       building_code,
       building_name,
       COUNT(*) AS room_count,
       SUM(capacity) AS nominal_capacity,
       SUM(configured_beds) AS configured_beds,
       SUM(active_contracts) AS current_occupants,
       SUM(available_slots) AS available_slots
FROM v_room_occupancy
GROUP BY building_id, building_code, building_name;

GO
CREATE OR ALTER VIEW v_current_residents AS
SELECT c.id AS contract_id,
       c.contract_code,
       s.id AS student_id,
       s.student_code,
       u.full_name,
       u.phone,
       b.name AS building_name,
       r.room_number,
       bd.bed_number,
       c.start_date,
       c.end_date,
       o.checked_in_at
FROM contracts c
JOIN students s ON s.id = c.student_id
JOIN users u ON u.id = s.user_id
JOIN beds bd ON bd.id = c.bed_id
JOIN rooms r ON r.id = bd.room_id
JOIN buildings b ON b.id = r.building_id
LEFT JOIN occupancy_records o ON o.contract_id = c.id
WHERE c.status = 'active'
  AND CONVERT(date, SYSDATETIME()) >= c.start_date
  AND CONVERT(date, SYSDATETIME()) < c.end_date
  AND o.checked_in_at IS NOT NULL
  AND o.checked_out_at IS NULL;

GO
CREATE OR ALTER VIEW v_monthly_invoice_summary AS
SELECT DATEFROMPARTS(YEAR(i.period_start), MONTH(i.period_start), 1) AS billing_month,
       COUNT(DISTINCT i.id) AS invoice_count,
       SUM(COALESCE(it.total_amount, 0)) AS billed_amount,
       SUM(COALESCE(vb.paid_amount, 0)) AS collected_amount,
       SUM(COALESCE(vb.balance_amount, 0)) AS outstanding_amount
FROM invoices i
LEFT JOIN (
  SELECT invoice_id, SUM(amount) AS total_amount
  FROM invoice_items
  GROUP BY invoice_id
) it ON it.invoice_id = i.id
LEFT JOIN v_invoice_balances vb ON vb.invoice_id = i.id
WHERE i.status <> 'void'
GROUP BY DATEFROMPARTS(YEAR(i.period_start), MONTH(i.period_start), 1);

GO
CREATE OR ALTER VIEW v_registration_summary AS
SELECT status, COUNT(*) AS registration_count
FROM room_registrations
GROUP BY status;

GO
CREATE OR ALTER VIEW v_repair_summary AS
SELECT status, priority, COUNT(*) AS request_count
FROM repair_requests
GROUP BY status, priority;

GO
CREATE OR ALTER VIEW v_student_outstanding_balances AS
SELECT s.id AS student_id,
       s.student_code,
       u.full_name,
       COUNT(vb.invoice_id) AS unpaid_invoice_count,
       SUM(GREATEST(vb.balance_amount, 0)) AS outstanding_amount
FROM students s
JOIN users u ON u.id = s.user_id
JOIN contracts c ON c.student_id = s.id
JOIN v_invoice_balances vb ON vb.contract_id = c.id
WHERE vb.balance_amount > 0
GROUP BY s.id, s.student_code, u.full_name;
GO
