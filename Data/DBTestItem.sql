
--create table item (itemCode varchar(32) not null, itemName nvarchar(256) null, itemName2 nvarchar(256) null, uom nchar(16) null, accountItem varchar(32) null, typeItem varchar(32) null, [status] char(1) null, userId0 int null, userId2 int null, datetime0 datetime, [datetime2] datetime)
--GO
--ALTER TABLE item ADD CONSTRAINT PK_item PRIMARY KEY (itemCode);
--GO
--create table itemDetail1 (itemCode varchar(32) not null, line_nbr int not null, customerCode varchar(32) null, [address] nvarchar(256) null, [description] nvarchar(256) null)
--GO
--ALTER TABLE itemDetail1 ADD CONSTRAINT PK_itemDetail1 PRIMARY KEY (itemCode, line_nbr);

--insert into item select 'VT001', N'Vật tư 001', N'Item Name 001', N'pcs', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT002', N'Vật tư 002', N'Item Name 002', N'cái', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT003', N'Vật tư 003', N'Item Name 003', N'mét', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT004', N'Vật tư 004', N'Item Name 004', N'thùng', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT005', N'Vật tư 005', N'Item Name 005', N'cuộn', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT006', N'Vật tư 006', N'Item Name 006', N'bao', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()
--insert into item select 'VT007', N'Vật tư 006', N'Item Name 006', N'kg', '1561', '21', '1', 1, 1, GETDATE(), GETDATE()

--insert into itemDetail1 select 'VT007', 1, N'KH001', N'Thủ Đức', N'Diễn giản 001 khách hàng 007'
--insert into itemDetail1 select 'VT007', 2, N'KH001', N'Quận 7', N'Diễn giản 002 khách hàng 007'
--insert into itemDetail1 select 'VT007', 3, N'KH003', N'Quận 12', N'Diễn giản 003 khách hàng 007'