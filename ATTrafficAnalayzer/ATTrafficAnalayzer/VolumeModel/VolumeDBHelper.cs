﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ATTrafficAnalayzer.VolumeModel
{
    class VolumeDBHelper
    {
        string dbFile = "Data Source=TAdb.db3";

        public VolumeDBHelper()
        {

            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            //Check if tables exist in database file create if they don't
            createVolumesTableIfNotExists(conn);
            createApproachesTableIfNotExists(conn);
            createConfigsTableIfNotExists(conn);

            conn.Close();
        }

        private void createConfigsTableIfNotExists(SQLiteConnection conn)
        {

            var createTable = @"CREATE TABLE IF NOT EXISTS [configs] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";
            SQLiteCommand create = new SQLiteCommand(conn);
            create.CommandText = createTable;
            create.ExecuteNonQuery();

        }

        private void createApproachesTableIfNotExists(SQLiteConnection conn)
        {

            var createTable = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [name] TEXT  NULL,
                                    [approach] TEXT  NULL,

                                    PRIMARY KEY (name)
                                )";
            SQLiteCommand create = new SQLiteCommand(conn);
            create.CommandText = createTable;
            create.ExecuteNonQuery();

        }

        private void createVolumesTableIfNotExists(SQLiteConnection conn)
        {

            var createTable = @"CREATE TABLE IF NOT EXISTS [volumes] ( 
                                    [dateTime] DATETIME DEFAULT CURRENT_TIMESTAMP NULL, 
                                    [intersection] INTEGER  NULL,
                                    [detector] INTEGER  NULL,
                                    [volume] INTEGER  NULL,
                                    PRIMARY KEY ( dateTime, intersection, detector)
                                    
                                )";
            SQLiteCommand create = new SQLiteCommand(conn);
            create.CommandText = createTable;
            create.ExecuteNonQuery();

        }

        #region Volume Related Methods
        public void importFile(string filename)
        {

            //Open the db connection
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();

            //Load the file into memory
            FileStream fs = new FileStream(filename, FileMode.Open);
            int sizeInBytes = (int)fs.Length;
            byte[] byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            bool alreadyLoaded = false;

            //Now decrypt it
            int index = 0;
            DateTimeRecord currentDateTime = null;

            using (SQLiteCommand cmd = new SQLiteCommand(conn))
            {
                using (SQLiteTransaction transaction = conn.BeginTransaction())
                {

                    while (index < sizeInBytes) //seek through the byte array untill we reach the end
                    {
                        int recordSize = byteArray[index] + byteArray[index + 1] * 256; //The record size is stored in two bytes, little endian

                        index += 2;

                        byte[] record;
                        if (recordSize % 2 == 0) //Records with odd record length have a trailing null byte.
                        {
                            record = byteArray.Skip(index).Take(recordSize).ToArray();
                            index += recordSize;
                        }
                        else
                        {
                            record = byteArray.Skip(index).Take(recordSize + 1).ToArray();
                            index += recordSize + 1;
                        }

                        //Find out what kind of data we have
                        RecordType recordType = RecordFactory.checkRecordType(record);

                        //Construct the appropriate record type
                        switch (recordType)
                        {
                            case RecordType.DATETIME:
                                currentDateTime = RecordFactory.createDateTimeRecord(record);
                                break;
                            case RecordType.VOLUME:
                                VolumeRecord volumeRecord = RecordFactory.createVolumeRecord(record, recordSize);

                                foreach (int detector in volumeRecord.GetDetectors())
                                {
                                   

                                    cmd.CommandText = "INSERT INTO volumes (dateTime, intersection, detector, volume) VALUES (@dateTime, @intersection, @detector, @volume);";
                                    
                                    cmd.Parameters.Clear();
                                 
                                    cmd.Parameters.AddWithValue("@dateTime", currentDateTime.dateTime);
                                    cmd.Parameters.AddWithValue("@intersection", volumeRecord.IntersectionNumber);
                                    cmd.Parameters.AddWithValue("@detector", detector);
                                    cmd.Parameters.AddWithValue("@volume", volumeRecord.GetVolumeForDetector(detector));

                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException e)
                                    {

                                        if(e.ReturnCode.Equals(SQLiteErrorCode.Constraint))
                                        {
                                            
                                            alreadyLoaded = true;
                                        }
                                        
                                        break;
                                    }
                                }
                                
                                break;
                        }
                        if (alreadyLoaded)
                        {
                            MessageBox.Show("Volume information from " + filename + " has already been loaded");
                            break;
                        }
                    }
                    transaction.Commit();
                }
                
            }
            conn.Close();
            fs.Close();
        }

        public List<int> getIntersections()
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            var intersections = new List<int>();
            using (SQLiteCommand query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT DISTINCT intersection FROM volumes;";
                SQLiteDataReader reader = query.ExecuteReader();

                while (reader.Read())
                {
                    intersections.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
            return intersections;

        }

        public List<int> getDetectorsAtIntersection(int intersection)
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            List<int> detectors = new List<int>();
            using (SQLiteCommand query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT DISTINCT detector FROM volumes WHERE intersection = @intersection;";
                query.Parameters.AddWithValue("@intersection", intersection);
                SQLiteDataReader reader = query.ExecuteReader();

                while (reader.Read())
                {
                    detectors.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
            return detectors;
        }

        public int getVolume(int intersection, int detector, DateTime dateTime)
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            int volume;
            using (SQLiteCommand query = new SQLiteCommand(conn))
            {

                query.CommandText = "SELECT volume from volumes WHERE intersection = '@intersection' AND detector = '@detector' AND dateTime = '@dateTime';";

                query.Parameters.AddWithValue("@intersection", intersection);
                query.Parameters.AddWithValue("@detector", intersection);
                query.Parameters.AddWithValue("@dateTime", dateTime);

                SQLiteDataReader reader = query.ExecuteReader();
                if (reader.RecordsAffected != 1)
                {
                    throw new Exception("WHOAH");
                }
                volume = reader.GetInt32(0);
            }
            conn.Close();
            return volume;
        }
        #endregion

        #region Configuration Related Methods

        public List<String> getConfigurations()
        {
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            List<String> configs = new List<string>();
            using (SQLiteCommand query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT name FROM configs;";
                SQLiteDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    configs.Add(reader.GetString(0));
                }
            }
            conn.Close();
            return configs;
        }

        public List<Approach> getApproaches(String configName)
        {
            throw new NotImplementedException();
        }

        public bool removeConfiguration(String name)
        {
            Logger.Info("deleting " + name, "db helper");
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            String sql = string.Format("DELETE FROM configs WHERE name='{0}';", name);
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            try
            {
                command.ExecuteNonQuery();
                conn.Close();
            }
            catch (SQLiteException e)
            {
                Logger.Error(e.StackTrace, "DBHelper");
                return false;
            }
            return true;
        }

        public bool renameConfig(String oldName, String newName)
        {
            Logger.Info("renaming '@oldName' to '@newName'", "db helper");
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            String sql = "UPDATE configs SET name=@newName WHERE name=@oldName;";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            try
            {
                command.ExecuteNonQuery();
                conn.Close();
            }
            catch (SQLiteException)
            {
                return false;
            }
            return true;
        }

        public bool addConfig(String name)
        {
            Logger.Info("inserting @name", "db helper");
            SQLiteConnection conn = new SQLiteConnection(dbFile);
            conn.Open();
            String sql = "INSERT INTO configs (name) VALUES (@name);";
            SQLiteCommand command = new SQLiteCommand(sql, conn);
            try
            {
                command.ExecuteNonQuery();
                conn.Close();
            }
            catch (SQLiteException)
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
