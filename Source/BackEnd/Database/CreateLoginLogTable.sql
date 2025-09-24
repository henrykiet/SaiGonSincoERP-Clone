-- Tạo bảng LoginLogs để lưu log đăng nhập
CREATE TABLE [dbo].[LoginLogs](
    [LoginLogId] [int] IDENTITY(1,1) NOT NULL,
    [UserId] [int] NOT NULL,
    [UserName] [nvarchar](100) NOT NULL,
    [LoginTime] [datetime2](7) NOT NULL,
    [IpAddress] [nvarchar](45) NULL,
    [UserAgent] [nvarchar](500) NULL,
    [SessionId] [nvarchar](50) NOT NULL,
    [Status] [nvarchar](20) NOT NULL,
    [FailureReason] [nvarchar](500) NULL,
    [LogoutTime] [datetime2](7) NULL,
    [Unit] [nvarchar](100) NULL,
    CONSTRAINT [PK_LoginLogs] PRIMARY KEY CLUSTERED ([LoginLogId] ASC)
) ON [PRIMARY]
GO

-- Tạo index cho các trường thường được query
CREATE NONCLUSTERED INDEX [IX_LoginLogs_UserId] ON [dbo].[LoginLogs]([UserId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_LoginLogs_SessionId] ON [dbo].[LoginLogs]([SessionId] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_LoginLogs_LoginTime] ON [dbo].[LoginLogs]([LoginTime] ASC)
GO

CREATE NONCLUSTERED INDEX [IX_LoginLogs_Status] ON [dbo].[LoginLogs]([Status] ASC)
GO

-- Thêm foreign key constraint nếu bảng Users tồn tại
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
BEGIN
    ALTER TABLE [dbo].[LoginLogs] 
    ADD CONSTRAINT [FK_LoginLogs_Users] 
    FOREIGN KEY([UserId]) REFERENCES [dbo].[Users] ([UserId])
    ON DELETE CASCADE
END
GO

-- Thêm các cột mới vào bảng Users nếu chưa có
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'CurrentSessionId')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [CurrentSessionId] [nvarchar](50) NULL
END
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'CurrentSessionCreated')
BEGIN
    ALTER TABLE [dbo].[Users] ADD [CurrentSessionCreated] [datetime2](7) NULL
END
GO

-- Tạo index cho CurrentSessionId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_CurrentSessionId')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Users_CurrentSessionId] ON [dbo].[Users]([CurrentSessionId] ASC)
END
GO


