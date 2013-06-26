using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;

namespace ATTrafficAnalayzer.VolumeModel
{
    class VolumeDBHelper
    {
        private const string DB_PATH = "Data Source=TAdb.db3";

        SQLiteDataAdapter configsDataAdapter;
        public DataSet configsDataSet = new DataSet();

        public VolumeDBHelper()
        {
            using (var dbConnection = new SQLiteConnection(DB_PATH))
            {
                dbConnection.Open();

                //Initialize a new database
                createVolumesTableIfNotExists(dbConnection);
                createApproachesTableIfNotExists(dbConnection);
                createConfigsTableIfNotExists(dbConnection);

                dbConnection.Close();
            }
        }

        private static void createConfigsTableIfNotExists(SQLiteConnection dbConnection)
        {
            const string createConfigsTableSQL = @"CREATE TABLE IF NOT EXISTS [configs] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";

            var createConfigsTableCommand = new SQLiteCommand(dbConnection);
            createConfigsTableCommand.CommandText = createConfigsTableSQL;
            createConfigsTableCommand.ExecuteNonQuery();
        }

        private static void createApproachesTableIfNotExists(SQLiteConnection dbConnection)
        {
            const string createApproachesTableSQL = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [name] TEXT  NULL,
                                    [approach] TEXT  NULL,

                                    PRIMARY KEY (name)
                                )";

            var createApproachesTableCommand = new SQLiteCommand(dbConnection);
            createApproachesTableCommand.CommandText = createApproachesTableSQL;
            createApproachesTableCommand.ExecuteNonQuery();
        }

        private static void createVolumesTableIfNotExists(SQLiteConnection dbConnection)
        {
            const string createVolumesTableSQL = @"CREATE TABLE IF NOT EXISTS [volumes] ( 
                                    [dateTime] DATETIME DEFAULT CURRENT_TIMESTAMP NULL, 
                                    [intersection] INTEGER  NULL,
                                    [detector] INTEGER  NULL,
                                    [volume] INTEGER  NULL,
                                    PRIMARY KEY ( dateTime, intersection, detector)
                                    
                                )";

            var createVolumesTableCommand = new SQLiteCommand(dbConnection);
            createVolumesTableCommand.CommandText = createVolumesTableSQL;
            createVolumesTableCommand.ExecuteNonQuery();
        }


        #region Volume Related Methods

        public static void importFile(string filename)
        {
            //Open the db connection
            var dbConnection = new SQLiteConnection(DB_PATH);
            dbConnection.Open();

            //Load the file into memory
            var fs = new FileStream(filename, FileMode.Open);
            var sizeInBytes = (int)fs.Length;
            var byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            var alreadyLoaded = false;

            //Now decrypt it
            var index = 0;
            DateTimeRecord currentDateTime = null;

            using (var cmd = new SQLiteCommand(dbConnection))
            {
                using (var transaction = dbConnection.BeginTransaction())
                {

                    while (index < sizeInBytes) //seek through the byte array untill we reach the end
                    {
                        var recordSize = byteArray[index] + byteArray[index + 1] * 256; //The record size is stored in two bytes, little endian

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
                        var recordType = RecordFactory.checkRecordType(record);

                        //Construct the appropriate record type
                        switch (recordType)
                        {
                            case RecordType.Datetime:
                                currentDateTime = RecordFactory.createDateTimeRecord(record);
                                break;
                            case RecordType.Volume:
                                var volumeRecord = RecordFactory.createVolumeRecord(record, recordSize);

                                foreach (var detector in volumeRecord.GetDetectors())
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

                                        if (e.ReturnCode.Equals(SQLiteErrorCode.Constraint))
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
            dbConnection.Close();
            fs.Close();
        }

        public static List<int> GetIntersections()
        {
            var conn = new SQLiteConnection(DB_PATH);
            conn.Open();
            var intersections = new List<int>();
            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT DISTINCT intersection FROM volumes;";
                var reader = query.ExecuteReader();

                while (reader.Read())
                {
                    intersections.Add(reader.GetInt32(0));
                }
            }
            conn.Close();
            return intersections;

        }

        public static List<int> GetDetectorsAtIntersection(int intersection)
        {
            var conn = new SQLiteConnection(DB_PATH);
            conn.Open();
            var detectors = new List<int>();
            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT DISTINCT detector FROM volumes WHERE intersection = @intersection;";
                query.Parameters.AddWithValue("@intersection", intersection);
                var reader = query.ExecuteReader();

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
            var conn = new SQLiteConnection(DB_PATH);
            conn.Open();
            int volume;
            using (var query = new SQLiteCommand(conn))
            {

                query.CommandText = "SELECT volume from volumes WHERE intersection = '@intersection' AND detector = '@detector' AND dateTime = '@dateTime';";

                query.Parameters.AddWithValue("@intersection", intersection);
                query.Parameters.AddWithValue("@detector", intersection);
                query.Parameters.AddWithValue("@dateTime", dateTime);

                var reader = query.ExecuteReader();
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

        public DataView GetConfigs()
        {
            //TODO change to USING but stops deleting
            var dbConnection = new SQLiteConnection(DB_PATH);
            dbConnection.Open();

            try
            {
                var getConfigsSQL = new SQLiteCommand("SELECT name FROM configs;", dbConnection);
                configsDataAdapter = new SQLiteDataAdapter(getConfigsSQL);
                configsDataAdapter.Fill(configsDataSet);
            }
            catch (SQLiteException e)
            {
                Logger.Error(e.ToString(), "DB Helper");
            }
            finally
            {
                Logger.Info("Retrieved configs data", "DB Helper");
            }

            dbConnection.Close();

            initializeConfigs();

            return configsDataSet.Tables[0].DefaultView;
        }

        public void initializeConfigs()
        {
            var dbCoomandBuilder = new SQLiteCommandBuilder(configsDataAdapter);

            var configsPrimaryKeys = new DataColumn[1];
            configsPrimaryKeys[0] = configsDataSet.Tables[0].Columns["name"];
            configsDataSet.Tables[0].PrimaryKey = configsPrimaryKeys;
        }

        public List<Approach> getApproaches(String configName)
        {
            throw new NotImplementedException();
        }

        public void removeConfig(string configToDelete)
        {
            removeConfigFromDataSet(configToDelete);
            syncDatabase();            
        }

        public bool renameConfig(String oldName, String newName)
        {
            //Logger.Info("renaming '@oldName' to '@newName'", "db helper");
            //SQLiteConnection conn = new SQLiteConnection(DB_PATH);
            //conn.Open();
            //String sql = "UPDATE configs SET name=@newName WHERE name=@oldName;";
            //SQLiteCommand command = new SQLiteCommand(sql, conn);
            //try
            //{
            //    command.ExecuteNonQuery();
            //    conn.Close();
            //}
            //catch (SQLiteException)
            //{
            //    return false;
            //}
            return true;
        }

        public bool addConfig(String name)
        {
            //Logger.Info("inserting @name", "db helper");
            //SQLiteConnection conn = new SQLiteConnection(DB_PATH);
            //conn.Open();
            //String sql = "INSERT INTO configs (name) VALUES (@name);";
            //SQLiteCommand command = new SQLiteCommand(sql, conn);
            //try
            //{
            //    command.ExecuteNonQuery();
            //    conn.Close();
            //}
            //catch (SQLiteException)
            //{
            //    return false;
            //}
            return true;
        }

        public void removeConfigFromDataSet(String configToDelete)
        {
            try
            {
                //Get row and delete it
                var configs = configsDataSet.Tables[0].Rows;
                var rowToDelete = configs.Find(configToDelete);
                rowToDelete.Delete();
            }
            catch (Exception)
            {
                Logger.Error("Could not deleted " + configToDelete + " from the dataset", "DB Helper");
            }
            finally
            {
                Logger.Error("Successfully deleted " + configToDelete + " from the dataset", "DB Helper");
            }
        }

        public void syncDatabase()
        {
            try
            {
                configsDataAdapter.Update(configsDataSet);
            } catch (SQLiteException)
            {
                Logger.Error("Could not synchronize database", "DB Helper");
            }
            finally
            {
                Logger.Error("Successfully synchronized database", "DB Helper");
            }
        }

        #endregion
    }
}
