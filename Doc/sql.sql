create table customer(ma_kh varchar(16), ten_kh nvarchar(256), ten_kh2 nvarchar(256), dien_thoai varchar(64), email varchar(64), status varchar(1), userid0 int, userid2 int, datetime0 datetime, [datetime2] datetime)
GO
create table customerDetail(ma_kh varchar(16), line_nbr int, ghi_chu nvarchar(256))
GO
create table testPostLedger (idGui varchar(64), voucherNumer varchar(33))
GO

create table order$000000 (idGui varchar(64), customerID varchar(16), voucherNumer varchar(16), voucherDate datetime, note nvarchar(256)
	, totalAmount numeric(24, 2), totalTax numeric(24, 2), totalPayment numeric(24, 2), status varchar(1), userid0 int, userid2 int, datetime0 datetime, [datetime2] datetime)
GO
create table orderDetail$000000 (idGui varchar(64), line_nbr int, voucherNumer varchar(16), voucherDate datetime, itemID varchar(16), uom nvarchar(16), siteID varchar(16), amount numeric(24, 2), tax numeric(24, 2), payment numeric(24, 2))
GO
create table order$202506 (idGui varchar(64), customerID varchar(16), voucherNumer varchar(16), voucherDate datetime, note nvarchar(256)
	, totalAmount numeric(24, 2), totalTax numeric(24, 2), totalPayment numeric(24, 2), status varchar(1), userid0 int, userid2 int, datetime0 datetime, [datetime2] datetime)
GO
create table orderDetail$202506 (idGui varchar(64), line_nbr int, voucherNumer varchar(16), voucherDate datetime, itemID varchar(16), uom nvarchar(16), siteID varchar(16), amount numeric(24, 2), tax numeric(24, 2), payment numeric(24, 2))
GO

CREATE PROCEDURE postLedger
    @customer varchar(33),
	@voucherNumer varchar(33),
	@voucherDate datetime,
    @action varchar(16),
	@idGui varchar(64) = ''
AS
BEGIN
    insert into testPostLedger select @idGui, @voucherNumer
END