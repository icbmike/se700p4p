-- Script Date: 12/29/2013 2:02 PM  - ErikEJ.SqlCeScripting version 3.5.2.34
-- Database information:
-- Locale Identifier: 1033
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\Users\Mike\Documents\GitHub\se700p4p\ATTrafficAnalayzer\ATTrafficAnalayzer\Ta.sdf
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

SELECT 1;
PRAGMA foreign_keys=OFF;
BEGIN TRANSACTION;
CREATE TABLE [monthly_summaries] (
  [name] nvarchar(100) NOT NULL
, [config] ntext NULL
, [last_used] datetime NULL
, CONSTRAINT [PK_monthly_summaries] PRIMARY KEY ([name])
);
CREATE TABLE [intersections] (
  [intersection_id] int NOT NULL
, [detector] tinyint NOT NULL
, CONSTRAINT [PK_intersections] PRIMARY KEY ([intersection_id],[detector])
);
CREATE TABLE [volumes] (
  [intersection] int NOT NULL
, [detector] tinyint NOT NULL
, [dateTime] datetime NOT NULL
, [volume] smallint NULL
, CONSTRAINT [PK_volumes] PRIMARY KEY ([intersection],[detector],[dateTime])
, FOREIGN KEY ([intersection], [detector]) REFERENCES [intersections] ([intersection_id], [detector]) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE [configs] (
  [config_id] INTEGER NOT NULL
, [name] nvarchar(100) NULL
, [date_last_used] datetime NULL
, [intersection_id] int NULL
, CONSTRAINT [PK_configs] PRIMARY KEY ([config_id])
);
CREATE TABLE [config_approach_mapping] (
  [config_id] int NOT NULL
, [approach_id] int NOT NULL
, CONSTRAINT [PK_config_approach_mapping] PRIMARY KEY ([config_id],[approach_id])
, FOREIGN KEY ([config_id]) REFERENCES [configs] ([config_id]) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE TABLE [approaches] (
  [approach_id] INTEGER NOT NULL
, [name] nvarchar(100) NULL
, CONSTRAINT [PK_approaches] PRIMARY KEY ([approach_id])
);
CREATE TABLE [approach_detector_mapping] (
  [approach_id] int NOT NULL
, [detector] tinyint NOT NULL
, CONSTRAINT [PK_approach_detector_mapping] PRIMARY KEY ([approach_id],[detector])
);
CREATE UNIQUE INDEX [UQ__monthly_summaries__0000000000000050] ON [monthly_summaries] ([name] ASC);
CREATE INDEX [volumes_index] ON [volumes] ([intersection] ASC,[detector] ASC,[dateTime] ASC);
COMMIT;

