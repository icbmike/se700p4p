-- Script Date: 12/7/2013 5:08 PM  - ErikEJ.SqlCeScripting version 3.5.2.34
-- Database information:
-- Locale Identifier: 1033
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\Users\Mike\Documents\GitHub\se700p4p\ATTrafficAnalayzer\ATTrafficAnalayzer\TA.sdf
-- ServerVersion: 4.0.8876.1
-- DatabaseSize: 5.582 MB
-- SpaceAvailable: 3.994 GB
-- Created: 12/2/2013 3:55 PM

-- User Table information:
-- Number of tables: 4
-- approaches: 1 row(s)
-- configs: 1 row(s)
-- monthly_summaries: 0 row(s)
-- volumes: 75048 row(s)

CREATE TABLE [volumes] (
  [dateTime] datetime NOT NULL
, [intersection] int NOT NULL
, [detector] smallint NOT NULL
, [volume] smallint NULL
);
GO
CREATE TABLE [monthly_summaries] (
  [name] nvarchar(100) NOT NULL
, [config] ntext NULL
, [last_used] datetime NULL
);
GO
CREATE TABLE [configs] (
  [name] nvarchar(100) NOT NULL
, [config] ntext NULL
, [last_used] datetime NULL
);
GO
CREATE TABLE [approaches] (
  [approach] ntext NULL
, [id] int IDENTITY (1,1) NOT NULL
);
GO
ALTER TABLE [volumes] ADD CONSTRAINT [PK__volumes__0000000000000024] PRIMARY KEY ([dateTime],[intersection],[detector]);
GO
ALTER TABLE [monthly_summaries] ADD CONSTRAINT [PK_monthly_summaries] PRIMARY KEY ([name]);
GO
ALTER TABLE [configs] ADD CONSTRAINT [PK_configs] PRIMARY KEY ([name]);
GO
ALTER TABLE [approaches] ADD CONSTRAINT [PK__approaches__0000000000000069] PRIMARY KEY ([id]);
GO
CREATE INDEX [volumes_index] ON [volumes] ([intersection] ASC,[detector] ASC,[dateTime] DESC);
GO
CREATE UNIQUE INDEX [UQ__monthly_summaries__0000000000000050] ON [monthly_summaries] ([name] ASC);
GO

