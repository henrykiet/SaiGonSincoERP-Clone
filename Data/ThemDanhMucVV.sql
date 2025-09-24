
CREATE TABLE [dbo].[job](
	[jobCode] [nvarchar](16) NOT NULL,
	[jobName] [nvarchar](256) NULL,
	[jobName2] [nvarchar](256) NULL,
	[descript] [nvarchar](256) NULL,
	[status] [char](1) NULL,
	[userId0] [int] NULL,
	[userId2] [int] NULL,
	[datetime0] [datetime] NULL,
	[datetime2] [datetime] NULL,
 CONSTRAINT [PK_job] PRIMARY KEY CLUSTERED 
(
	[jobCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO


