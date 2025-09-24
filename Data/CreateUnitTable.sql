-- Tạo bảng Unit
CREATE TABLE Unit (
    UnitCode VARCHAR(16) PRIMARY KEY,
    UnitName NVARCHAR(128) NOT NULL,
    UnitName2 NVARCHAR(128) NOT NULL
);

-- Thêm dữ liệu mẫu
INSERT INTO Unit (UnitCode, UnitName, UnitName2) VALUES
('HQ', N'Tổng Công ty', 'Headquarters'),
('CN1', N'Chi nhánh 1', 'Branch 1'),
('CN2', N'Chi nhánh 2', 'Branch 2'),
('CN3', N'Chi nhánh 3', 'Branch 3'),
('VP', N'Văn phòng', 'Office'),
('KHO', N'Kho hàng', 'Warehouse'),
('SX', N'Phân xưởng sản xuất', 'Production Workshop');

-- Tạo index để tối ưu hiệu suất truy vấn
CREATE INDEX IX_Unit_UnitName ON Unit(UnitName);
CREATE INDEX IX_Unit_UnitName2 ON Unit(UnitName2); 