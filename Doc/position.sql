use systemReport

go
CREATE TABLE accountSinco (
    account_sinco_id NVARCHAR(200) PRIMARY KEY,
    account_sinco_name NVARCHAR(255) NOT NULL,          -- Tên tài khoản
    account_type CHAR(2) NOT NULL,                      -- TM = Tiền mặt, NH = Ngân hàng
    bank_name NVARCHAR(200) NULL,                       -- Tên ngân hàng
    account_number NVARCHAR(200) NULL,                  -- Số tài khoản
    bank_branch NVARCHAR(200) NULL,                     -- Chi nhánh ngân hàng
    note NVARCHAR(500) NULL,                            -- Ghi chú
    status CHAR(1) NOT NULL DEFAULT '1',                -- 1 = Active, 0 = Inactive
    user_id2 INT NULL,
    datetime2 DATETIME NULL
);
INSERT INTO accountSinco (account_sinco_id, account_sinco_name, account_type, bank_name, account_number, bank_branch, note, status)
VALUES
('TM0001', N'Tiền mặt quỹ chính', 'TM', NULL, NULL, NULL, N'Tài khoản tiền mặt tại quỹ công ty', '1'),
('NH0001', N'Tài khoản Vietcombank', 'NH', N'Ngân hàng TMCP Ngoại thương Việt Nam', '0123456789', N'Chi nhánh Hà Nội', N'Tài khoản giao dịch chính', '1'),
('NH0002', N'Tài khoản BIDV', 'NH', N'Ngân hàng Đầu tư và Phát triển Việt Nam', '9876543210', N'Chi nhánh Hồ Chí Minh', N'Tài khoản thanh toán dự phòng', '1');

go
CREATE TABLE positionGroup (
    position_group_id NVARCHAR(200) PRIMARY KEY,
    position_group_name NVARCHAR(255) NOT NULL,      -- Tên nhóm chức vụ (VD: Quản lý, Hành chính)
    description NVARCHAR(500) NULL,                  -- Mô tả
    status CHAR(1) NOT NULL DEFAULT '1', 
    user_id2 INT NULL,
    datetime2 DATETIME NULL
);
INSERT INTO positionGroup (position_group_id, position_group_name, description, status)
VALUES 
('PG001', N'Quản lý', N'Nhóm các chức vụ quản lý cấp cao và cấp trung', '1'),
('PG002', N'Chuyên môn', N'Nhóm các chức vụ yêu cầu chuyên môn kỹ thuật', '1'),
('PG003', N'Hành chính', N'Nhóm các chức vụ hành chính, văn phòng', '1'),
('PG004', N'Kế toán', N'Nhóm các chức vụ kế toán và tài chính', '1'),
('PG005', N'Điều dưỡng', N'Nhóm các chức vụ điều dưỡng, chăm sóc y tế', '1');


go
CREATE TABLE position (
    position_id NVARCHAR(200) PRIMARY KEY,
    position_name NVARCHAR(255) NOT NULL,       -- Tên chức vụ
    position_group_id INT NULL,                 -- FK -> position_group
    level INT NULL,                             -- Cấp bậc (1-5)
    description NVARCHAR(500) NULL,             -- Mô tả chức vụ
    status CHAR(1) NOT NULL DEFAULT '1',        -- 1 = Active, 0 = Inactive
    display_order INT NULL,                     -- Thứ tự hiển thị
    user_id2 INT NULL,
    datetime2 DATETIME NULL
);
INSERT INTO position (position_id, position_name, position_group_id, level, description, status, display_order)
VALUES
('POS001', N'Giám đốc', 'PG001', 5, N'Lãnh đạo cao nhất của doanh nghiệp', '1', 1),
('POS002', N'Phó giám đốc', 'PG001', 4, N'Hỗ trợ điều hành, phụ trách một số lĩnh vực', '1', 2),
('POS003', N'Trưởng phòng Kế toán', 'PG004', 4, N'Quản lý toàn bộ hoạt động kế toán', '1', 3),
('POS004', N'Chuyên viên Nhân sự', 'PG003', 2, N'Thực hiện nghiệp vụ quản lý nhân sự', '1', 4),
('POS005', N'Điều dưỡng trưởng', 'PG005', 3, N'Phụ trách công tác điều dưỡng và chăm sóc bệnh nhân', '1', 5);
