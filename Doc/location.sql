use systemReport
drop table district,province,ward
CREATE TABLE location (
    province_code NVARCHAR(20) NOT NULL,
    province_name NVARCHAR(100) NOT NULL,
    district_code NVARCHAR(20) NOT NULL,
    district_name NVARCHAR(100) NOT NULL,
    ward_code NVARCHAR(20) NOT NULL,
    ward_name NVARCHAR(100) NOT NULL,
    unit_type NVARCHAR(20) NOT NULL, -- Phường/Xã/Thị trấn
    ward_code_old NVARCHAR(20) NULL,
    district_code_old NVARCHAR(20) NULL,
    province_code_old NVARCHAR(20) NULL,
    status CHAR(1) NOT NULL DEFAULT '1', -- 1: Active, 0: Inactive
    last_update_date DATE NULL,
    user_id2 INT NULL,
    datetime2 DATETIME NULL
    CONSTRAINT PK_location PRIMARY KEY (province_code, district_code, ward_code)
);
-- Bảng Tỉnh/Thành
CREATE TABLE province (
    province_code NVARCHAR(20) NOT NULL,          -- Mã QGHC mới (2 số)
    province_name NVARCHAR(100) NOT NULL,    -- Tên tỉnh/thành
    province_code_old NVARCHAR(20) NULL,     -- Mã cũ nếu có
    status CHAR(1) NOT NULL DEFAULT '1',           -- 1: Active, 0: Inactive
    last_update_date DATE NULL,              -- Ngày cập nhật bản đồ
    user_id2 INT NULL,
    datetime2 DATETIME NULL
    CONSTRAINT PK_province PRIMARY KEY (province_code)
);

-- Bảng Quận/Huyện
CREATE TABLE district (
    district_code NVARCHAR(20) NOT NULL,          -- Mã QGHC mới (3 số)
    district_name NVARCHAR(100) NOT NULL,    -- Tên quận/huyện
    province_code NVARCHAR(20) NOT NULL,          -- Liên kết tới province_code
    district_code_old NVARCHAR(20) NULL,     -- Mã cũ nếu có
    status CHAR(1) NOT NULL DEFAULT '1',           -- 1: Active, 0: Inactive
    last_update_date DATE NULL,              -- Ngày cập nhật bản đồ
    user_id2 INT NULL,
    datetime2 DATETIME NULL
    CONSTRAINT PK_district PRIMARY KEY (district_code)
);

-- Bảng Xã/Phường
CREATE TABLE ward (
    ward_code NVARCHAR(20) NOT NULL,              -- Mã QGHC mới (5 ký tự)
    ward_name NVARCHAR(100) NOT NULL,        -- Tên xã/phường
    district_code NVARCHAR(20) NOT NULL,          -- Liên kết tới district_code
    unit_type NVARCHAR(20) NOT NULL,         -- Phường/Xã/Thị trấn
    ward_code_old NVARCHAR(20) NULL,         -- Mã cũ nếu có
    status CHAR(1) NOT NULL DEFAULT '1',           -- 1: Active, 0: Inactive
    last_update_date DATE NULL,              -- Ngày cập nhật bản đồ
    user_id2 INT NULL,
    datetime2 DATETIME NULL
    CONSTRAINT PK_ward PRIMARY KEY (ward_code)
);
-- Province
INSERT INTO province (province_code, province_name, province_code_old, status, last_update_date, datetime2)
VALUES
  ('01', N'Thành phố Hà Nội', NULL, '1', '2025-07-01', GETDATE()),
  ('48', N'Thành phố Đà Nẵng', NULL, '1', '2025-07-01', GETDATE()),
  ('79', N'Thành phố Hồ Chí Minh', NULL, '1', '2025-07-01', GETDATE());

-- District
INSERT INTO district (district_code, district_name, province_code, district_code_old, status, last_update_date, datetime2)
VALUES
  ('001', N'Quận Ba Đình', '01', NULL, '1', '2025-07-01', GETDATE()),
  ('002', N'Quận Hoàn Kiếm', '01', NULL, '1', '2025-07-01', GETDATE()),
  ('07901', N'Quận Hải Châu', '48', NULL, '1', '2025-07-01', GETDATE()),
  ('07902', N'Quận Thanh Khê', '48', NULL, '1', '2025-07-01', GETDATE()),
  ('760', N'Quận 1', '79', NULL, '1', '2025-07-01', GETDATE()),
  ('761', N'Quận 3', '79', NULL, '1', '2025-07-01', GETDATE());

-- Ward
INSERT INTO ward (ward_code, ward_name, district_code, unit_type, ward_code_old, status, last_update_date, datetime2)
VALUES
  ('00001', N'Phường Phúc Xá', '001', N'Phường', NULL, '1', '2025-07-01', GETDATE()),
  ('00004', N'Phường Trúc Bạch', '001', N'Phường', NULL, '1', '2025-07-01', GETDATE()),
  ('00006', N'Phường Vĩnh Phúc', '001', N'Phường', NULL, '1', '2025-07-01', GETDATE()),
  ('00037', N'Phường Phúc Tân', '002', N'Phường', NULL, '1', '2025-07-01', GETDATE()),
  ('00040', N'Phường Đồng Xuân', '002', N'Phường', NULL, '1', '2025-07-01', GETDATE());

-- Location (master data)
INSERT INTO location (
    province_code, province_name,
    district_code, district_name,
    ward_code, ward_name,
    unit_type, ward_code_old, district_code_old, province_code_old,
    status, last_update_date, datetime2
)
VALUES
('01', N'Hà Nội', '001', N'Ba Đình', '00001', N'Phúc Xá', N'Phường', NULL, NULL, 'HN', '1', '2025-01-03', GETDATE()),
('01', N'Hà Nội', '001', N'Ba Đình', '00004', N'Trúc Bạch', N'Phường', NULL, NULL, 'HN', '1', '2025-01-03', GETDATE()),
('79', N'Hồ Chí Minh', '760', N'Quận 1', '26734', N'Bến Nghé', N'Phường', NULL, NULL, 'HCM', '1', '2025-01-03', GETDATE());
