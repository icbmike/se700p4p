using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Windows;

namespace ATTrafficAnalayzer.VolumeModel
{
    class VolumeDbHelper
    {
        private const string DbPath = "Data Source=TAdb.db3";

        SQLiteDataAdapter _configsDataAdapter;
        private DataSet _configsDataSet = new DataSet();

        public VolumeDbHelper()
        {
            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                //Initialize a new database
                CreateVolumesTableIfNotExists(dbConnection);
                CreateApproachesTableIfNotExists(dbConnection);
                CreateConfigsTableIfNotExists(dbConnection);

                dbConnection.Close();
            }
        }

        private static void CreateConfigsTableIfNotExists(SQLiteConnection dbConnection)
        {
            const string createConfigsTableSql = @"CREATE TABLE IF NOT EXISTS [configs] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";

            var createConfigsTableCommand = new SQLiteCommand(dbConnection) {CommandText = createConfigsTableSql};
            createConfigsTableCommand.ExecuteNonQuery();
        }

        private static void CreateApproachesTableIfNotExists(SQLiteConnection dbConnection)
        {


            var createApproachesTableSql = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                    [approach] TEXT  NULL

                                )";

            var createApproachesTableCommand = new SQLiteCommand(dbConnection) {CommandText = createApproachesTableSql};
            createApproachesTableCommand.ExecuteNonQuery();
        }

        private static void CreateVolumesTableIfNotExists(SQLiteConnection dbConnection)
        {
            const string createVolumesTableSql = @"CREATE TABLE IF NOT EXISTS [volumes] ( 
                                    [dateTime] DATETIME DEFAULT CURRENT_TIMESTAMP NULL, 
                                    [intersection] INTEGER  NULL,
                                    [detector] INTEGER  NULL,
                                    [volume] INTEGER  NULL,
                                    PRIMARY KEY ( dateTime, intersection, detector)
                                    
                                )";

            var createVolumesTableCommand = new SQLiteCommand(dbConnection) {CommandText = createVolumesTableSql};
            createVolumesTableCommand.ExecuteNonQuery();
        }


        #region Volume Related Methods

        public static void ImportFile(string filename)
        {
            //Open the db connection
            var dbConnection = new SQLiteConnection(DbPath);
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
                        var recordType = RecordFactory.CheckRecordType(record);

                        //Construct the appropriate record type
                        switch (recordType)
                        {
                            case RecordType.Datetime:
                                currentDateTime = RecordFactory.CreateDateTimeRecord(record);
                                break;
                            case RecordType.Volume:
                                var volumeRecord = RecordFactory.CreateVolumeRecord(record, recordSize);

                                foreach (var detector in volumeRecord.GetDetectors())
                                {


                                    cmd.CommandText = "INSERT INTO volumes (dateTime, intersection, detector, volume) VALUES (@dateTime, @intersection, @detector, @volume);";

                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@dateTime", currentDateTime.DateTime);
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
                        if (!alreadyLoaded) continue;
                        MessageBox.Show("Volume information from " + filename + " has already been loaded");
                        break;
                    }
                    transaction.Commit();
                }

            }
            dbConnection.Close();
            fs.Close();
        }

        public static List<int> GetIntersections()
        {
            var conn = new SQLiteConnection(DbPath);
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
            var conn = new SQLiteConnection(DbPath);
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

        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            var conn = new SQLiteConnection(DbPath);
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
            var dbConnection = new SQLiteConnection(DbPath);
            dbConnection.Open();

            try
            {
                var getConfigsSql = new SQLiteCommand("SELECT name FROM configs;", dbConnection);
                _configsDataAdapter = new SQLiteDataAdapter(getConfigsSql);
                _configsDataAdapter.Fill(_configsDataSet);
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

            InitializeConfigs();

            return _configsDataSet.Tables[0].DefaultView;
        }

        private void InitializeConfigs()
        {
            new SQLiteCommandBuilder(_configsDataAdapter);

            var configsPrimaryKeys = new DataColumn[1];
            configsPrimaryKeys[0] = _configsDataSet.Tables[0].Columns["name"];
            _configsDataSet.Tables[0].PrimaryKey = configsPrimaryKeys;
        }

        public void addConfiguration(ReportConfiguration config)
        {

            var configJson = config.toJson();
            SQLiteConnection conn = new SQLiteConnection(DbPath);
            conn.Open();

            
            foreach(Approach approach in config.Approaches)
            {
                //INSERT APPROACHES INTO TABLE
                using (SQLiteCommand query = new SQLiteCommand(conn))
                {
                    query.CommandText = "INSERT INTO approaches (approach) VALUES (@approach);";
                    query.Parameters.AddWithValue("@approach", approach.toJSON().ToString());
                    query.ExecuteNonQuery();
                }
                //GET IDS SO THAT WE CAN ADD IT TO THE REPORT CONFIGURATION
                using (SQLiteCommand query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT last_insert_rowid();";
                    var rowID = (Int64)query.ExecuteScalar();
                    configJson.GetJSONArray("approaches").Put(rowID);
                }
            }

            //INSERT REPORT CONFIGURATION INTO TABLE
            using (SQLiteCommand query = new SQLiteCommand(conn))
            {
                query.CommandText = "INSERT INTO configs (name, config, last_used) VALUES (@name, @config, @last_used);";
                query.Parameters.AddWithValue("@name", config.ConfigName);
                query.Parameters.AddWithValue("@config", configJson.ToString());
                query.Parameters.AddWithValue("@last_used", DateTime.Today);
                query.ExecuteNonQuery();

            }            
        }

        public List<Approach> GetApproaches(String configName)

        {
            throw new NotImplementedException();
        }

        public void RemoveConfig(string configToDelete)
        {
            RemoveConfigFromDataSet(configToDelete);
            SyncDatabase();            
        }

        public void RemoveConfigFromDataSet(String configToDelete)
        {
            try
            {
                //Get row and delete it
                var configs = _configsDataSet.Tables[0].Rows;
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

        public void SyncDatabase()
        {
            try
            {
                _configsDataAdapter.Update(_configsDataSet);
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
