IF EXISTS (SELECT * FROM sys.tables WHERE name = 'order$000000')
    DROP TABLE [dbo].[order$000000];

CREATE TABLE [dbo].[order$000000](
	[idGui] [varchar](64) NULL,
	[customerID] [varchar](16) NULL,
	[voucherNumer] [varchar](16) NULL,
	[voucherDate] [datetime] NULL,
	[note] [nvarchar](256) NULL,
	[totalAmount] [numeric](24, 2) NULL,
	[totalTax] [numeric](24, 2) NULL,
	[totalPayment] [numeric](24, 2) NULL,
	[status] [varchar](1) NULL,
	[userid0] [int] NULL,
	[userid2] [int] NULL,
	[datetime0] [datetime] NULL,
	[datetime2] [datetime] NULL,
	[ten_kh] [nvarchar](512) NULL,
	[email_kh] [nvarchar](128) NULL,
	[phone_kh] [nvarchar](33) NULL,
	[address_kh] [nvarchar](512) NULL,
	[order_code] [varchar](33) NULL,
	[order_date] [smalldatetime] NULL,
	[shipping_fee] [numeric](19, 4) NULL
) ON [PRIMARY]
GO
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'order$202506')
    DROP TABLE [dbo].[order$202506];

CREATE TABLE [dbo].[order$202506](
	[idGui] [varchar](64) NULL,
	[customerID] [varchar](16) NULL,
	[voucherNumer] [varchar](16) NULL,
	[voucherDate] [datetime] NULL,
	[note] [nvarchar](256) NULL,
	[totalAmount] [numeric](24, 2) NULL,
	[totalTax] [numeric](24, 2) NULL,
	[totalPayment] [numeric](24, 2) NULL,
	[status] [varchar](1) NULL,
	[userid0] [int] NULL,
	[userid2] [int] NULL,
	[datetime0] [datetime] NULL,
	[datetime2] [datetime] NULL,
	[ten_kh] [nvarchar](512) NULL,
	[email_kh] [nvarchar](128) NULL,
	[phone_kh] [nvarchar](33) NULL,
	[address_kh] [nvarchar](512) NULL,
	[order_code] [varchar](33) NULL,
	[order_date] [smalldatetime] NULL,
	[shipping_fee] [numeric](19, 4) NULL
) ON [PRIMARY]
GO
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'orderDetail$000000')
    DROP TABLE [dbo].[orderDetail$000000];

CREATE TABLE [dbo].[orderDetail$000000](
	[idGui] [varchar](64) NULL,
	[line_nbr] [int] NULL,
	[voucherNumer] [varchar](16) NULL,
	[voucherDate] [datetime] NULL,
	[itemID] [varchar](16) NULL,
	[uom] [nvarchar](16) NULL,
	[siteID] [varchar](16) NULL,
	[amount] [numeric](24, 2) NULL,
	[tax] [numeric](24, 2) NULL,
	[payment] [numeric](24, 2) NULL,
	[tax_code] [varchar](8) NULL,
	[tax_rate] [numeric](5, 2) NULL,
	[itemName] [nvarchar](256) NULL,
	[quantity] [numeric](19, 4) NULL,
	[price] [numeric](19, 4) NULL,
	[total] [numeric](19, 4) NULL
) ON [PRIMARY]
GO
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'orderDetail$202506')
    DROP TABLE [dbo].[orderDetail$202506];

CREATE TABLE [dbo].[orderDetail$202506](
	[idGui] [varchar](64) NULL,
	[line_nbr] [int] NULL,
	[voucherNumer] [varchar](16) NULL,
	[voucherDate] [datetime] NULL,
	[itemID] [varchar](16) NULL,
	[uom] [nvarchar](16) NULL,
	[siteID] [varchar](16) NULL,
	[amount] [numeric](24, 2) NULL,
	[tax] [numeric](24, 2) NULL,
	[payment] [numeric](24, 2) NULL,
	[tax_code] [varchar](8) NULL,
	[tax_rate] [numeric](5, 2) NULL,
	[itemName] [nvarchar](256) NULL,
	[quantity] [numeric](19, 4) NULL,
	[price] [numeric](19, 4) NULL,
	[total] [numeric](19, 4) NULL
) ON [PRIMARY]
GO


