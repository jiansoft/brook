CREATE DATABASE [test];
GO
USE [test]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[account](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[name] [nvarchar](64) NOT NULL,
	[email] [varchar](64) NOT NULL,
 CONSTRAINT [PK_account] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_name]  DEFAULT ('') FOR [name]
GO
ALTER TABLE [dbo].[account] ADD  CONSTRAINT [DF_account_email]  DEFAULT ('') FOR [email]
GO
INSERT INTO [dbo].[account] (name,email)VALUES('Ben Nuttall', 'eddie@sqlserver.com');
INSERT INTO [dbo].[account] (name,email)VALUES('許功蓋', 'RedHat@sqlserver.com');
GO