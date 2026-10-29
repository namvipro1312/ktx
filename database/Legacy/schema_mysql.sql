-- Database schema for a dormitory management system.
-- Target: MySQL 8.0.16+ (CHECK constraints are enforced from 8.0.16).
-- Run this file against a MySQL server. It creates the database if needed.

CREATE DATABASE IF NOT EXISTS qlktx
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE qlktx;

-- SP1: Accounts, roles, permissions, buildings, rooms and beds
CREATE TABLE IF NOT EXISTS users (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  username VARCHAR(100) NOT NULL,
  email VARCHAR(255) NULL,
  password_hash VARCHAR(255) NOT NULL,
  full_name VARCHAR(200) NOT NULL,
  phone VARCHAR(30) NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  last_login_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_users_username (username),
  UNIQUE KEY uq_users_email (email)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS roles (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code VARCHAR(50) NOT NULL,
  name VARCHAR(100) NOT NULL,
  description VARCHAR(255) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_roles_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS permissions (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code VARCHAR(100) NOT NULL,
  name VARCHAR(150) NOT NULL,
  module VARCHAR(80) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_permissions_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS user_roles (
  user_id BIGINT UNSIGNED NOT NULL,
  role_id BIGINT UNSIGNED NOT NULL,
  assigned_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_id, role_id),
  CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
  CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS role_permissions (
  role_id BIGINT UNSIGNED NOT NULL,
  permission_id BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (role_id, permission_id),
  CONSTRAINT fk_role_permissions_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE,
  CONSTRAINT fk_role_permissions_permission FOREIGN KEY (permission_id) REFERENCES permissions(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS user_sessions (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  token_hash CHAR(64) NOT NULL,
  expires_at DATETIME NOT NULL,
  revoked_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  ip_address VARCHAR(45) NULL,
  user_agent VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_user_sessions_token_hash (token_hash),
  KEY idx_user_sessions_user_expiry (user_id, expires_at),
  CONSTRAINT fk_user_sessions_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS password_reset_tokens (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  token_hash CHAR(64) NOT NULL,
  expires_at DATETIME NOT NULL,
  used_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_password_reset_token_hash (token_hash),
  KEY idx_password_reset_user_expiry (user_id, expires_at),
  CONSTRAINT fk_password_reset_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS login_attempts (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  username VARCHAR(100) NOT NULL,
  ip_address VARCHAR(45) NULL,
  succeeded BOOLEAN NOT NULL DEFAULT FALSE,
  attempted_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_login_attempts_username_time (username, attempted_at),
  KEY idx_login_attempts_ip_time (ip_address, attempted_at)
) ENGINE=InnoDB;

-- Initial roles and permission catalog. Assign role_permissions to match the
-- project's final authorization policy; do not hard-code authorization in SQL.
INSERT IGNORE INTO roles (code, name, description) VALUES
  ('system_admin', 'Quản trị hệ thống', 'Quản lý tài khoản và cấu hình hệ thống'),
  ('dorm_manager', 'Quản lý ký túc xá', 'Quản lý vận hành ký túc xá'),
  ('student', 'Sinh viên', 'Tài khoản sinh viên nội trú'),
  ('maintenance_staff', 'Nhân viên sửa chữa', 'Tiếp nhận và xử lý yêu cầu sửa chữa'),
  ('accountant', 'Kế toán', 'Quản lý hóa đơn và thanh toán');

INSERT IGNORE INTO permissions (code, name, module) VALUES
  ('users.manage', 'Quản lý tài khoản', 'sp1'),
  ('roles.manage', 'Quản lý vai trò và quyền', 'sp1'),
  ('buildings.manage', 'Quản lý tòa nhà', 'sp1'),
  ('rooms.manage', 'Quản lý phòng', 'sp1'),
  ('beds.manage', 'Quản lý giường', 'sp1'),
  ('registrations.create', 'Tạo đăng ký phòng', 'sp2'),
  ('registrations.review', 'Duyệt đăng ký phòng', 'sp2'),
  ('contracts.manage', 'Quản lý hợp đồng', 'sp3'),
  ('services.manage', 'Quản lý dịch vụ', 'sp4'),
  ('repairs.manage', 'Quản lý sửa chữa', 'sp4'),
  ('rules.manage', 'Quản lý nội quy', 'sp4'),
  ('invoices.manage', 'Quản lý hóa đơn', 'sp5'),
  ('payments.manage', 'Quản lý thanh toán', 'sp5'),
  ('reports.view', 'Xem thống kê', 'sp5'),
  ('checkin.manage', 'Quản lý nhận và trả phòng', 'sp3'),
  ('utilities.manage', 'Quản lý điện nước', 'sp4'),
  ('assets.manage', 'Quản lý tài sản', 'sp1'),
  ('violations.manage', 'Quản lý vi phạm', 'sp4'),
  ('guests.manage', 'Quản lý khách thăm', 'sp4'),
  ('audit.view', 'Xem nhật ký thao tác', 'sp1'),
  ('repairs.create', 'Gửi yêu cầu sửa chữa', 'sp4'),
  ('rules.view', 'Xem nội quy', 'sp4'),
  ('guests.request', 'Đăng ký khách thăm', 'sp4');

-- Starter permission matrix. Adjust it to the school's actual policy.
INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r CROSS JOIN permissions p
WHERE r.code = 'system_admin';

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
WHERE r.code = 'dorm_manager'
  AND p.code IN ('buildings.manage','rooms.manage','beds.manage','registrations.review',
                 'contracts.manage','checkin.manage','services.manage','utilities.manage',
                 'repairs.manage','rules.manage','assets.manage','violations.manage',
                 'guests.manage','reports.view','audit.view');

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
WHERE r.code = 'student'
  AND p.code IN ('registrations.create','repairs.create','rules.view','guests.request');

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
WHERE r.code = 'maintenance_staff'
  AND p.code IN ('repairs.manage','assets.manage');

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id FROM roles r JOIN permissions p
WHERE r.code = 'accountant'
  AND p.code IN ('invoices.manage','payments.manage','reports.view');

CREATE TABLE IF NOT EXISTS students (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  student_code VARCHAR(50) NOT NULL,
  date_of_birth DATE NULL,
  gender ENUM('male', 'female', 'other', 'unspecified') NOT NULL DEFAULT 'unspecified',
  faculty VARCHAR(150) NULL,
  course_year SMALLINT UNSIGNED NULL,
  emergency_contact_name VARCHAR(200) NULL,
  emergency_contact_phone VARCHAR(30) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_students_user (user_id),
  UNIQUE KEY uq_students_code (student_code),
  CONSTRAINT fk_students_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS buildings (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code VARCHAR(30) NOT NULL,
  name VARCHAR(150) NOT NULL,
  address VARCHAR(255) NULL,
  description TEXT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_buildings_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS rooms (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  building_id BIGINT UNSIGNED NOT NULL,
  room_number VARCHAR(30) NOT NULL,
  floor_number SMALLINT NULL,
  room_type VARCHAR(80) NULL,
  capacity SMALLINT UNSIGNED NOT NULL,
  monthly_price DECIMAL(12,2) NOT NULL DEFAULT 0,
  status ENUM('available', 'full', 'maintenance', 'inactive') NOT NULL DEFAULT 'available',
  description TEXT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_rooms_building_number (building_id, room_number),
  KEY idx_rooms_status (status),
  CONSTRAINT fk_rooms_building FOREIGN KEY (building_id) REFERENCES buildings(id) ON DELETE RESTRICT,
  CONSTRAINT chk_rooms_capacity CHECK (capacity > 0),
  CONSTRAINT chk_rooms_price CHECK (monthly_price >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS beds (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  room_id BIGINT UNSIGNED NOT NULL,
  bed_number VARCHAR(30) NOT NULL,
  status ENUM('available', 'occupied', 'maintenance', 'inactive') NOT NULL DEFAULT 'available',
  PRIMARY KEY (id),
  UNIQUE KEY uq_beds_room_number (room_id, bed_number),
  KEY idx_beds_room_status (room_id, status),
  CONSTRAINT fk_beds_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

-- SP2: Room applications and approval history
CREATE TABLE IF NOT EXISTS room_registrations (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  registration_code VARCHAR(40) NOT NULL,
  student_id BIGINT UNSIGNED NOT NULL,
  preferred_building_id BIGINT UNSIGNED NULL,
  preferred_room_type VARCHAR(80) NULL,
  requested_start_date DATE NOT NULL,
  requested_end_date DATE NOT NULL,
  submitted_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  status ENUM('pending', 'approved', 'rejected', 'cancelled', 'contracted') NOT NULL DEFAULT 'pending',
  note TEXT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_registrations_code (registration_code),
  KEY idx_registrations_student_status (student_id, status),
  KEY idx_registrations_dates (requested_start_date, requested_end_date),
  CONSTRAINT fk_registrations_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE RESTRICT,
  CONSTRAINT fk_registrations_building FOREIGN KEY (preferred_building_id) REFERENCES buildings(id) ON DELETE SET NULL,
  CONSTRAINT chk_registration_dates CHECK (requested_end_date > requested_start_date)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS registration_reviews (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  registration_id BIGINT UNSIGNED NOT NULL,
  reviewer_user_id BIGINT UNSIGNED NOT NULL,
  decision ENUM('approved', 'rejected', 'requested_changes') NOT NULL,
  comment TEXT NULL,
  reviewed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_registration_reviews_registration (registration_id, reviewed_at),
  CONSTRAINT fk_registration_reviews_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE CASCADE,
  CONSTRAINT fk_registration_reviews_user FOREIGN KEY (reviewer_user_id) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS registration_bed_offers (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  registration_id BIGINT UNSIGNED NOT NULL,
  bed_id BIGINT UNSIGNED NOT NULL,
  offered_by_user_id BIGINT UNSIGNED NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  status ENUM('offered', 'accepted', 'declined', 'expired', 'withdrawn') NOT NULL DEFAULT 'offered',
  expires_at DATETIME NULL,
  responded_at DATETIME NULL,
  note VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_bed_offers_registration (registration_id, status),
  KEY idx_bed_offers_bed_dates (bed_id, start_date, end_date),
  CONSTRAINT fk_bed_offers_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE CASCADE,
  CONSTRAINT fk_bed_offers_bed FOREIGN KEY (bed_id) REFERENCES beds(id) ON DELETE RESTRICT,
  CONSTRAINT fk_bed_offers_user FOREIGN KEY (offered_by_user_id) REFERENCES users(id) ON DELETE RESTRICT,
  CONSTRAINT chk_bed_offer_dates CHECK (end_date > start_date)
) ENGINE=InnoDB;

-- SP3: Contracts. The assigned bed is reserved for the contract date range.
CREATE TABLE IF NOT EXISTS contracts (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  contract_code VARCHAR(40) NOT NULL,
  registration_id BIGINT UNSIGNED NULL,
  student_id BIGINT UNSIGNED NOT NULL,
  bed_id BIGINT UNSIGNED NOT NULL,
  created_by_user_id BIGINT UNSIGNED NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  monthly_price DECIMAL(12,2) NOT NULL,
  deposit_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
  status ENUM('draft', 'active', 'expired', 'terminated', 'cancelled') NOT NULL DEFAULT 'draft',
  signed_at DATETIME NULL,
  terms TEXT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_contracts_code (contract_code),
  UNIQUE KEY uq_contracts_registration (registration_id),
  KEY idx_contracts_student_status (student_id, status),
  KEY idx_contracts_bed_dates (bed_id, start_date, end_date),
  CONSTRAINT fk_contracts_registration FOREIGN KEY (registration_id) REFERENCES room_registrations(id) ON DELETE SET NULL,
  CONSTRAINT fk_contracts_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE RESTRICT,
  CONSTRAINT fk_contracts_bed FOREIGN KEY (bed_id) REFERENCES beds(id) ON DELETE RESTRICT,
  CONSTRAINT fk_contracts_creator FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_contract_dates CHECK (end_date > start_date),
  CONSTRAINT chk_contract_prices CHECK (monthly_price >= 0 AND deposit_amount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS contract_amendments (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  contract_id BIGINT UNSIGNED NOT NULL,
  amendment_code VARCHAR(40) NOT NULL,
  amendment_type ENUM('renewal', 'price_change', 'date_change', 'bed_change', 'other') NOT NULL,
  old_values JSON NULL,
  new_values JSON NULL,
  reason TEXT NULL,
  effective_date DATE NOT NULL,
  created_by_user_id BIGINT UNSIGNED NULL,
  signed_at DATETIME NULL,
  document_path VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_contract_amendment_code (amendment_code),
  KEY idx_contract_amendments_contract (contract_id, effective_date),
  CONSTRAINT fk_contract_amendments_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT fk_contract_amendments_user FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS occupancy_records (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  contract_id BIGINT UNSIGNED NOT NULL,
  checked_in_at DATETIME NULL,
  checked_in_by_user_id BIGINT UNSIGNED NULL,
  checkin_condition JSON NULL,
  checkin_note TEXT NULL,
  expected_checkout_date DATE NOT NULL,
  checked_out_at DATETIME NULL,
  checked_out_by_user_id BIGINT UNSIGNED NULL,
  checkout_condition JSON NULL,
  checkout_note TEXT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_occupancy_contract (contract_id),
  KEY idx_occupancy_checkout (checked_out_at, expected_checkout_date),
  CONSTRAINT fk_occupancy_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT fk_occupancy_checkin_user FOREIGN KEY (checked_in_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT fk_occupancy_checkout_user FOREIGN KEY (checked_out_by_user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS deposit_transactions (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  contract_id BIGINT UNSIGNED NOT NULL,
  transaction_code VARCHAR(40) NOT NULL,
  transaction_type ENUM('collected', 'refunded', 'deducted') NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  method ENUM('cash', 'bank_transfer', 'online', 'other') NOT NULL DEFAULT 'bank_transfer',
  status ENUM('pending', 'completed', 'cancelled') NOT NULL DEFAULT 'completed',
  transaction_date DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  handled_by_user_id BIGINT UNSIGNED NULL,
  reason VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_deposit_transaction_code (transaction_code),
  KEY idx_deposit_contract_date (contract_id, transaction_date),
  CONSTRAINT fk_deposit_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT fk_deposit_handler FOREIGN KEY (handled_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_deposit_amount CHECK (amount > 0)
) ENGINE=InnoDB;

-- SP4: Services, service usage, repair requests and dormitory rules
CREATE TABLE IF NOT EXISTS services (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code VARCHAR(40) NOT NULL,
  name VARCHAR(150) NOT NULL,
  unit VARCHAR(40) NOT NULL DEFAULT 'month',
  unit_price DECIMAL(12,2) NOT NULL DEFAULT 0,
  is_metered BOOLEAN NOT NULL DEFAULT FALSE,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  description TEXT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_services_code (code),
  CONSTRAINT chk_services_price CHECK (unit_price >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS service_usages (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  service_id BIGINT UNSIGNED NOT NULL,
  contract_id BIGINT UNSIGNED NOT NULL,
  period_start DATE NOT NULL,
  period_end DATE NOT NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  unit_price DECIMAL(12,2) NOT NULL,
  recorded_by_user_id BIGINT UNSIGNED NULL,
  note VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_service_usage_period (service_id, contract_id, period_start, period_end),
  KEY idx_service_usage_contract_period (contract_id, period_start),
  CONSTRAINT fk_service_usages_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE RESTRICT,
  CONSTRAINT fk_service_usages_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT fk_service_usages_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_service_usage_dates CHECK (period_end > period_start),
  CONSTRAINT chk_service_usage_values CHECK (quantity >= 0 AND unit_price >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS utility_meters (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  room_id BIGINT UNSIGNED NOT NULL,
  service_id BIGINT UNSIGNED NOT NULL,
  meter_code VARCHAR(50) NOT NULL,
  installed_at DATE NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  note VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_utility_meters_code (meter_code),
  KEY idx_utility_meters_room_service (room_id, service_id, is_active),
  CONSTRAINT fk_utility_meters_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE RESTRICT,
  CONSTRAINT fk_utility_meters_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS utility_readings (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  meter_id BIGINT UNSIGNED NOT NULL,
  reading_date DATE NOT NULL,
  previous_value DECIMAL(14,3) NOT NULL,
  current_value DECIMAL(14,3) NOT NULL,
  consumption DECIMAL(14,3) NOT NULL,
  unit_price DECIMAL(12,2) NOT NULL,
  total_amount DECIMAL(14,2) NOT NULL,
  recorded_by_user_id BIGINT UNSIGNED NULL,
  service_usage_id BIGINT UNSIGNED NULL,
  note VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_utility_meter_date (meter_id, reading_date),
  UNIQUE KEY uq_utility_usage (service_usage_id),
  KEY idx_utility_reading_date (reading_date),
  CONSTRAINT fk_utility_reading_meter FOREIGN KEY (meter_id) REFERENCES utility_meters(id) ON DELETE RESTRICT,
  CONSTRAINT fk_utility_reading_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT fk_utility_reading_usage FOREIGN KEY (service_usage_id) REFERENCES service_usages(id) ON DELETE SET NULL,
  CONSTRAINT chk_utility_reading_values CHECK (previous_value >= 0 AND current_value >= previous_value AND consumption >= 0 AND unit_price >= 0 AND total_amount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS service_subscriptions (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  contract_id BIGINT UNSIGNED NOT NULL,
  service_id BIGINT UNSIGNED NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  agreed_unit_price DECIMAL(12,2) NOT NULL,
  status ENUM('active', 'paused', 'ended') NOT NULL DEFAULT 'active',
  note VARCHAR(500) NULL,
  PRIMARY KEY (id),
  KEY idx_service_subscriptions_contract (contract_id, status),
  KEY idx_service_subscriptions_service (service_id, start_date, end_date),
  CONSTRAINT fk_service_subscriptions_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT fk_service_subscriptions_service FOREIGN KEY (service_id) REFERENCES services(id) ON DELETE RESTRICT,
  CONSTRAINT chk_service_subscription_dates CHECK (end_date IS NULL OR end_date >= start_date),
  CONSTRAINT chk_service_subscription_amounts CHECK (quantity > 0 AND agreed_unit_price >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS repair_requests (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  request_code VARCHAR(40) NOT NULL,
  student_id BIGINT UNSIGNED NULL,
  room_id BIGINT UNSIGNED NOT NULL,
  reported_by_user_id BIGINT UNSIGNED NOT NULL,
  assigned_to_user_id BIGINT UNSIGNED NULL,
  title VARCHAR(200) NOT NULL,
  description TEXT NOT NULL,
  priority ENUM('low', 'normal', 'high', 'urgent') NOT NULL DEFAULT 'normal',
  status ENUM('submitted', 'accepted', 'in_progress', 'completed', 'rejected', 'cancelled') NOT NULL DEFAULT 'submitted',
  reported_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  completed_at DATETIME NULL,
  resolution_note TEXT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_repairs_code (request_code),
  KEY idx_repairs_status_priority (status, priority),
  KEY idx_repairs_room (room_id, reported_at),
  CONSTRAINT fk_repairs_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE SET NULL,
  CONSTRAINT fk_repairs_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE RESTRICT,
  CONSTRAINT fk_repairs_reporter FOREIGN KEY (reported_by_user_id) REFERENCES users(id) ON DELETE RESTRICT,
  CONSTRAINT fk_repairs_assignee FOREIGN KEY (assigned_to_user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS repair_request_updates (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  repair_request_id BIGINT UNSIGNED NOT NULL,
  updated_by_user_id BIGINT UNSIGNED NOT NULL,
  old_status ENUM('submitted', 'accepted', 'in_progress', 'completed', 'rejected', 'cancelled') NULL,
  new_status ENUM('submitted', 'accepted', 'in_progress', 'completed', 'rejected', 'cancelled') NULL,
  comment TEXT NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_repair_updates_request_date (repair_request_id, created_at),
  CONSTRAINT fk_repair_updates_request FOREIGN KEY (repair_request_id) REFERENCES repair_requests(id) ON DELETE CASCADE,
  CONSTRAINT fk_repair_updates_user FOREIGN KEY (updated_by_user_id) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS asset_types (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code VARCHAR(40) NOT NULL,
  name VARCHAR(120) NOT NULL,
  description VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_asset_types_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS room_assets (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  room_id BIGINT UNSIGNED NOT NULL,
  asset_type_id BIGINT UNSIGNED NOT NULL,
  asset_tag VARCHAR(60) NULL,
  name VARCHAR(150) NOT NULL,
  quantity SMALLINT UNSIGNED NOT NULL DEFAULT 1,
  condition_status ENUM('new', 'good', 'worn', 'damaged', 'disposed') NOT NULL DEFAULT 'good',
  acquired_at DATE NULL,
  last_checked_at DATETIME NULL,
  note VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_room_assets_tag (asset_tag),
  KEY idx_room_assets_room_type (room_id, asset_type_id),
  CONSTRAINT fk_room_assets_room FOREIGN KEY (room_id) REFERENCES rooms(id) ON DELETE RESTRICT,
  CONSTRAINT fk_room_assets_type FOREIGN KEY (asset_type_id) REFERENCES asset_types(id) ON DELETE RESTRICT,
  CONSTRAINT chk_room_asset_quantity CHECK (quantity > 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS dormitory_rules (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  title VARCHAR(200) NOT NULL,
  content TEXT NOT NULL,
  version VARCHAR(30) NOT NULL,
  effective_from DATE NOT NULL,
  effective_to DATE NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  created_by_user_id BIGINT UNSIGNED NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_rules_title_version (title, version),
  KEY idx_rules_active_dates (is_active, effective_from, effective_to),
  CONSTRAINT fk_rules_creator FOREIGN KEY (created_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_rule_dates CHECK (effective_to IS NULL OR effective_to >= effective_from)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS rule_acknowledgements (
  rule_id BIGINT UNSIGNED NOT NULL,
  student_id BIGINT UNSIGNED NOT NULL,
  acknowledged_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (rule_id, student_id),
  CONSTRAINT fk_rule_ack_rule FOREIGN KEY (rule_id) REFERENCES dormitory_rules(id) ON DELETE CASCADE,
  CONSTRAINT fk_rule_ack_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS guest_visits (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  student_id BIGINT UNSIGNED NOT NULL,
  guest_name VARCHAR(200) NOT NULL,
  guest_identity_no VARCHAR(80) NULL,
  guest_phone VARCHAR(30) NULL,
  visit_purpose VARCHAR(500) NULL,
  expected_arrival DATETIME NOT NULL,
  expected_departure DATETIME NULL,
  actual_arrival DATETIME NULL,
  actual_departure DATETIME NULL,
  approved_by_user_id BIGINT UNSIGNED NULL,
  status ENUM('pending', 'approved', 'rejected', 'checked_in', 'completed', 'cancelled') NOT NULL DEFAULT 'pending',
  note VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_guest_visits_student_date (student_id, expected_arrival),
  KEY idx_guest_visits_status (status, expected_arrival),
  CONSTRAINT fk_guest_visits_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE RESTRICT,
  CONSTRAINT fk_guest_visits_approver FOREIGN KEY (approved_by_user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- SP5: Invoices, invoice lines, payments and reporting views
CREATE TABLE IF NOT EXISTS invoices (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  invoice_code VARCHAR(40) NOT NULL,
  contract_id BIGINT UNSIGNED NOT NULL,
  period_start DATE NOT NULL,
  period_end DATE NOT NULL,
  issued_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  due_date DATE NOT NULL,
  status ENUM('draft', 'issued', 'partially_paid', 'paid', 'overdue', 'void') NOT NULL DEFAULT 'draft',
  note VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_invoices_code (invoice_code),
  UNIQUE KEY uq_invoice_contract_period (contract_id, period_start, period_end),
  KEY idx_invoices_status_due (status, due_date),
  CONSTRAINT fk_invoices_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE RESTRICT,
  CONSTRAINT chk_invoice_dates CHECK (period_end > period_start)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS invoice_items (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  invoice_id BIGINT UNSIGNED NOT NULL,
  item_type ENUM('room_rent', 'service', 'deposit', 'fine', 'other') NOT NULL,
  service_usage_id BIGINT UNSIGNED NULL,
  description VARCHAR(255) NOT NULL,
  quantity DECIMAL(12,3) NOT NULL DEFAULT 1,
  unit_price DECIMAL(12,2) NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  PRIMARY KEY (id),
  KEY idx_invoice_items_invoice (invoice_id),
  CONSTRAINT fk_invoice_items_invoice FOREIGN KEY (invoice_id) REFERENCES invoices(id) ON DELETE CASCADE,
  CONSTRAINT fk_invoice_items_usage FOREIGN KEY (service_usage_id) REFERENCES service_usages(id) ON DELETE SET NULL,
  CONSTRAINT chk_invoice_item_values CHECK (quantity >= 0 AND unit_price >= 0 AND amount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS payments (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  payment_code VARCHAR(40) NOT NULL,
  invoice_id BIGINT UNSIGNED NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  method ENUM('cash', 'bank_transfer', 'online', 'other') NOT NULL DEFAULT 'bank_transfer',
  status ENUM('pending', 'completed', 'failed', 'refunded') NOT NULL DEFAULT 'completed',
  transaction_reference VARCHAR(120) NULL,
  received_by_user_id BIGINT UNSIGNED NULL,
  paid_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  note VARCHAR(500) NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uq_payments_code (payment_code),
  UNIQUE KEY uq_payments_transaction_reference (transaction_reference),
  KEY idx_payments_invoice_status (invoice_id, status),
  KEY idx_payments_paid_at (paid_at),
  CONSTRAINT fk_payments_invoice FOREIGN KEY (invoice_id) REFERENCES invoices(id) ON DELETE RESTRICT,
  CONSTRAINT fk_payments_receiver FOREIGN KEY (received_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_payment_amount CHECK (amount > 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS payment_refunds (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  payment_id BIGINT UNSIGNED NOT NULL,
  refund_code VARCHAR(40) NOT NULL,
  amount DECIMAL(14,2) NOT NULL,
  reason VARCHAR(500) NULL,
  status ENUM('pending', 'completed', 'rejected') NOT NULL DEFAULT 'pending',
  method ENUM('cash', 'bank_transfer', 'online', 'other') NOT NULL DEFAULT 'bank_transfer',
  processed_by_user_id BIGINT UNSIGNED NULL,
  processed_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_payment_refunds_code (refund_code),
  KEY idx_payment_refunds_payment (payment_id, status),
  CONSTRAINT fk_payment_refunds_payment FOREIGN KEY (payment_id) REFERENCES payments(id) ON DELETE RESTRICT,
  CONSTRAINT fk_payment_refunds_user FOREIGN KEY (processed_by_user_id) REFERENCES users(id) ON DELETE SET NULL,
  CONSTRAINT chk_payment_refund_amount CHECK (amount > 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS student_violations (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  student_id BIGINT UNSIGNED NOT NULL,
  contract_id BIGINT UNSIGNED NULL,
  rule_id BIGINT UNSIGNED NULL,
  recorded_by_user_id BIGINT UNSIGNED NOT NULL,
  violation_code VARCHAR(40) NOT NULL,
  title VARCHAR(200) NOT NULL,
  description TEXT NULL,
  occurred_at DATETIME NOT NULL,
  fine_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
  status ENUM('recorded', 'under_review', 'resolved', 'appealed', 'cancelled') NOT NULL DEFAULT 'recorded',
  resolution_note TEXT NULL,
  resolved_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uq_student_violations_code (violation_code),
  KEY idx_student_violations_student_date (student_id, occurred_at),
  KEY idx_student_violations_status (status, occurred_at),
  CONSTRAINT fk_violations_student FOREIGN KEY (student_id) REFERENCES students(id) ON DELETE RESTRICT,
  CONSTRAINT fk_violations_contract FOREIGN KEY (contract_id) REFERENCES contracts(id) ON DELETE SET NULL,
  CONSTRAINT fk_violations_rule FOREIGN KEY (rule_id) REFERENCES dormitory_rules(id) ON DELETE SET NULL,
  CONSTRAINT fk_violations_recorder FOREIGN KEY (recorded_by_user_id) REFERENCES users(id) ON DELETE RESTRICT,
  CONSTRAINT chk_violation_fine CHECK (fine_amount >= 0)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS violation_invoice_items (
  violation_id BIGINT UNSIGNED NOT NULL,
  invoice_item_id BIGINT UNSIGNED NOT NULL,
  linked_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (violation_id),
  UNIQUE KEY uq_violation_invoice_item (invoice_item_id),
  CONSTRAINT fk_violation_invoice_violation FOREIGN KEY (violation_id) REFERENCES student_violations(id) ON DELETE RESTRICT,
  CONSTRAINT fk_violation_invoice_item FOREIGN KEY (invoice_item_id) REFERENCES invoice_items(id) ON DELETE RESTRICT
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS documents (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  entity_type ENUM('student', 'registration', 'contract', 'repair', 'invoice', 'violation', 'other') NOT NULL,
  entity_id BIGINT UNSIGNED NOT NULL,
  uploaded_by_user_id BIGINT UNSIGNED NULL,
  original_name VARCHAR(255) NOT NULL,
  storage_key VARCHAR(500) NOT NULL,
  mime_type VARCHAR(150) NULL,
  size_bytes BIGINT UNSIGNED NULL,
  checksum_sha256 CHAR(64) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_documents_entity (entity_type, entity_id),
  CONSTRAINT fk_documents_uploader FOREIGN KEY (uploaded_by_user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS notifications (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  title VARCHAR(200) NOT NULL,
  message TEXT NOT NULL,
  notification_type VARCHAR(60) NOT NULL DEFAULT 'general',
  entity_type VARCHAR(60) NULL,
  entity_id BIGINT UNSIGNED NULL,
  read_at DATETIME NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_notifications_user_unread (user_id, read_at, created_at),
  CONSTRAINT fk_notifications_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS audit_logs (
  id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NULL,
  action VARCHAR(100) NOT NULL,
  entity_type VARCHAR(80) NOT NULL,
  entity_id BIGINT UNSIGNED NULL,
  old_values JSON NULL,
  new_values JSON NULL,
  ip_address VARCHAR(45) NULL,
  user_agent VARCHAR(500) NULL,
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_audit_entity_date (entity_type, entity_id, created_at),
  KEY idx_audit_user_date (user_id, created_at),
  CONSTRAINT fk_audit_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE SET NULL
) ENGINE=InnoDB;

-- Report views. Invoice totals are derived from invoice_items and successful payments.
CREATE OR REPLACE VIEW v_invoice_balances AS
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
  SELECT invoice_id, SUM(amount) AS paid_amount
  FROM payments
  WHERE status = 'completed'
  GROUP BY invoice_id
) paid ON paid.invoice_id = i.id;

CREATE OR REPLACE VIEW v_room_occupancy AS
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
  AND CURDATE() >= c.start_date
  AND CURDATE() < c.end_date
GROUP BY b.id, b.code, b.name, r.id, r.room_number, r.capacity;

CREATE OR REPLACE VIEW v_building_occupancy AS
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

CREATE OR REPLACE VIEW v_current_residents AS
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
  AND CURDATE() >= c.start_date
  AND CURDATE() < c.end_date
  AND o.checked_in_at IS NOT NULL
  AND o.checked_out_at IS NULL;

CREATE OR REPLACE VIEW v_monthly_invoice_summary AS
SELECT DATE_FORMAT(i.period_start, '%Y-%m-01') AS billing_month,
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
GROUP BY DATE_FORMAT(i.period_start, '%Y-%m-01');

CREATE OR REPLACE VIEW v_registration_summary AS
SELECT status, COUNT(*) AS registration_count
FROM room_registrations
GROUP BY status;

CREATE OR REPLACE VIEW v_repair_summary AS
SELECT status, priority, COUNT(*) AS request_count
FROM repair_requests
GROUP BY status, priority;

CREATE OR REPLACE VIEW v_student_outstanding_balances AS
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

