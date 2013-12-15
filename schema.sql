-- Script Date: 12/11/2013 3:03 PM  - ErikEJ.SqlCeScripting version 3.5.2.34
-- Database information:
-- Locale Identifier: 1033
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\Users\Mike\Documents\GitHub\se700p4p\ATTrafficAnalayzer\ATTrafficAnalayzer\TA.sdf
-- ServerVersion: 4.0.8876.1
-- DatabaseSize: 192 KB
-- SpaceAvailable: 3.999 GB
-- Created: 12/2/2013 3:55 PM

-- User Table information:
-- Number of tables: 7
-- approach_detector_mapping: 0 row(s)
-- approaches: 0 row(s)
-- config_approach_mapping: 0 row(s)
-- configs: 0 row(s)
-- intersections: 0 row(s)
-- monthly_summaries: 0 row(s)
-- volumes: 0 row(s)

CREATE TABLE [monthly_summaries] (
  [name] nvarchar(100) NOT NULL
, [config] ntext NULL
, [last_used] datetime NULL
);
GO
CREATE TABLE [intersections] (
  [intersection_id] int NOT NULL
, [detector] tinyint NOT NULL
);
GO
CREATE TABLE [volumes] (
  [intersection] int NOT NULL
, [detector] tinyint NOT NULL
, [dateTime] datetime NOT NULL
, [volume] smallint NULL
);
GO
CREATE TABLE [configs] (
  [config_id] int IDENTITY (1,1) NOT NULL
, [name] nvarchar(100) NULL
, [date_last_used] datetime NULL
);
GO
CREATE TABLE [config_approach_mapping] (
  [config_id] int  NOT NULL
, [approach_id] int NOT NULL
);
GO
CREATE TABLE [approaches] (
  [approach_id] int IDENTITY (1,1) NOT NULL
, [name] nvarchar(100) NULL
);
GO
CREATE TABLE [approach_detector_mapping] (
  [approach_id] int NOT NULL
, [detector] tinyint NOT NULL
);
GO
ALTER TABLE [monthly_summaries] ADD CONSTRAINT [PK_monthly_summaries] PRIMARY KEY ([name]);
GO
ALTER TABLE [intersections] ADD CONSTRAINT [PK_intersections] PRIMARY KEY ([intersection_id],[detector]);
GO
ALTER TABLE [volumes] ADD CONSTRAINT [PK_volumes] PRIMARY KEY ([intersection],[detector],[dateTime]);
GO
ALTER TABLE [configs] ADD CONSTRAINT [PK_configs] PRIMARY KEY ([config_id]);
GO
ALTER TABLE [config_approach_mapping] ADD CONSTRAINT [PK_config_approach_mapping] PRIMARY KEY ([config_id],[approach_id]);
GO
ALTER TABLE [approaches] ADD CONSTRAINT [PK_approaches] PRIMARY KEY ([approach_id]);
GO
ALTER TABLE [approach_detector_mapping] ADD CONSTRAINT [PK_approach_detector_mapping] PRIMARY KEY ([approach_id],[detector]);
GO
CREATE UNIQUE INDEX [UQ__monthly_summaries__0000000000000050] ON [monthly_summaries] ([name] ASC);
GO
CREATE INDEX [volumes_index] ON [volumes] ([intersection] ASC,[detector] ASC,[dateTime] ASC);
GO
ALTER TABLE [volumes] ADD CONSTRAINT [intersection_volume_relation] FOREIGN KEY ([intersection], [detector]) REFERENCES [intersections]([intersection_id], [detector]) ON DELETE CASCADE ON UPDATE CASCADE;
GO

