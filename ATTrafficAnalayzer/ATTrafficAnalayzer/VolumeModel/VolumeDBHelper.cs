using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SQLite;
using System.Data;
using System.IO;
using System.Windows;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.VolumeModel
{
    class VolumeDbHelper
    {
        private const string DbPath = "Data Source=TAdb.db3";

        SQLiteDataAdapter _configsDataAdapter;
        private DataSet _configsDataSet = new DataSet();
        private static VolumeDbHelper _instance;
        private static readonly object SyncLock = new object();

        public static VolumeDbHelper GetDbHelper()
        {
            lock (SyncLock)
            {
                if (_instance == null)
                {
                    _instance = new VolumeDbHelper();
                }
                return _instance;
            }
        }

        private VolumeDbHelper()
        {
            CreateVolumesTableIfNotExists();
            CreateApproachesTableIfNotExists();
            CreateConfigsTableIfNotExists();
        }

        #region Helper Functions

        /// <summary>
        ///     Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        private DataTable GetDataTable(string sql)
        {
            var dataTable = new DataTable();

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql };
                var reader = createConfigsTableCommand.ExecuteReader();
                dataTable.Load(reader);
                reader.Close();

                dbConnection.Close();
            }

            return dataTable;
        }

        /// <summary>
        ///     Allows the programmer to interact with the database for purposes other than a query.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>
        private int ExecuteNonQuery(string sql)
        {
            var rowsUpdated = 0;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql };
                rowsUpdated = createConfigsTableCommand.ExecuteNonQuery();

                dbConnection.Close();
            }

            return rowsUpdated;
        }

        /// <summary>
        ///     Allows the programmer to retrieve single items from the DB.
        /// </summary>
        /// <param name="sql">The query to run.</param>
        /// <returns>A string.</returns>
        private object ExecuteScalar(string sql)
        {
            object reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql };
                reader = createConfigsTableCommand.ExecuteScalar();

                dbConnection.Close();
            }

            return reader;
        }

        /// <summary>
        ///     Allows the programmer to easily update rows in the DB.
        /// </summary>
        /// <param name="tableName">The table to update.</param>
        /// <param name="data">A dictionary containing Column names and their new values.</param>
        /// <param name="where">The where clause for the update statement.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Update(String tableName, Dictionary<String, String> data, String where)
        {
            var valuesToUpdate = "";
            var returnCode = true;

            if (data.Count >= 1)
            {
                foreach (var val in data)
                {
                    valuesToUpdate += String.Format(" {0} = {1},", val.Key, val.Value);
                }
                valuesToUpdate = valuesToUpdate.Substring(0, valuesToUpdate.Length - 1);
            }
            try
            {
                ExecuteNonQuery(String.Format("UPDATE {0} SET {1} WHERE {2};", tableName, valuesToUpdate, where));
            }
            catch
            {
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily delete rows from the DB.
        /// </summary>
        /// <param name="tableName">The table from which to delete.</param>
        /// <param name="where">The where clause for the delete.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Delete(String tableName, String where)
        {
            var returnCode = true;
            try
            {
                ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1};", tableName, where));
            }
            catch (Exception fail)
            {
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily insert into the DB
        /// </summary>
        /// <param name="tableName">The table into which we insert the data.</param>
        /// <param name="data">A dictionary containing the column names and data for the insert.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool Insert(String tableName, Dictionary<String, String> data)
        {
            var columns = "";
            var values = "";
            var returnCode = true;

            foreach (var val in data)
            {
                columns += String.Format(" {0},", val.Key);
                values += String.Format(" '{0}',", val.Value);
            }

            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);

            try
            {
                this.ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
            }
            catch (Exception fail)
            {
                MessageBox.Show(fail.Message);
                returnCode = false;
            }

            return returnCode;
        }

        /// <summary>
        ///     Allows the programmer to easily delete all data from the DB.
        /// </summary>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearDB()
        {
            DataTable tables;

            try
            {
                tables = this.GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
                foreach (DataRow table in tables.Rows)
                {
                    this.ClearTable(table["NAME"].ToString());
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Allows the user to easily clear all data from a specific table.
        /// </summary>
        /// <param name="table">The name of the table to clear.</param>
        /// <returns>A boolean true or false to signify success or failure.</returns>
        public bool ClearTable(String table)
        {
            try
            {
                this.ExecuteNonQuery(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }


        #endregion

        #region Table Initialization

        private void CreateConfigsTableIfNotExists()
        {
            const string createConfigsTableSql = @"CREATE TABLE IF NOT EXISTS [configs] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";
            ExecuteNonQuery(createConfigsTableSql);
        }

        private void CreateApproachesTableIfNotExists()
        {
            const string createApproachesTableSql = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                    [approach] TEXT  NULL
                                )";
            ExecuteNonQuery(createApproachesTableSql);
        }

        private void CreateVolumesTableIfNotExists()
        {
            const string createVolumesTableSql = @"CREATE TABLE IF NOT EXISTS [volumes] ( 
                                    [dateTime] DATETIME DEFAULT CURRENT_TIMESTAMP NULL, 
                                    [intersection] INTEGER  NULL,
                                    [detector] INTEGER  NULL,
                                    [volume] INTEGER  NULL,

                                    PRIMARY KEY ( dateTime, intersection, detector)
                                )";
            ExecuteNonQuery(createVolumesTableSql);
        }

        #endregion

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

        public ReportConfiguration GetConfiguration(string name)
        {
            Console.WriteLine(name);
            var conn = new SQLiteConnection(DbPath);
            conn.Open();

            JObject configJson = null;

            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT config FROM configs WHERE name = @name;";
                query.Parameters.AddWithValue("@name", name);
                var reader = query.ExecuteReader();

                if (reader.Read())
                    configJson = JObject.Parse(reader.GetString(0));
            }
            if (configJson != null)
            {
                var approaches = new List<Approach>();
                foreach (var approachID in (JArray)configJson["approaches"])
                {
                    using (var query = new SQLiteCommand(conn))
                    {
                        query.CommandText = "SELECT approach FROM approaches WHERE id = @id;";
                        query.Parameters.AddWithValue("@id", approachID);

                        var reader = query.ExecuteReader();
                        JObject approachJson = null;

                        if (reader.Read())
                            approachJson = JObject.Parse(reader.GetString(0));

                        approaches.Add(new Approach((string)approachJson["name"], approachJson["detectors"].Select(t => (int)t).ToList()));
                    }
                }
                conn.Close();
                return new ReportConfiguration(name, (int)configJson["intersection"], approaches);
            }
            conn.Close();
            return null;
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

            var configJson = config.ToJson();
            var conn = new SQLiteConnection(DbPath);
            conn.Open();

            foreach (Approach approach in config.Approaches)
            {
                //INSERT APPROACHES INTO TABLE
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "INSERT INTO approaches (approach) VALUES (@approach);";
                    query.Parameters.AddWithValue("@approach", approach.ToJson().ToString());
                    query.ExecuteNonQuery();
                }
                //GET IDS SO THAT WE CAN ADD IT TO THE REPORT CONFIGURATION
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT last_insert_rowid();";

                    var rowID = (Int64)query.ExecuteScalar();
                    ((JArray)configJson["approaches"]).Add(rowID);
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
            }
            catch (SQLiteException)
            {
                Logger.Error("Could not synchronize database", "DB Helper");
            }
            finally
            {
                Logger.Error("Successfully synchronized database", "DB Helper");
            }
        }

        public bool ConfigExists(String configName)
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var configExistsSql = "SELECT EXISTS(SELECT 1 FROM configs WHERE name = @configName LIMIT 1);";
                var configExistsQuery = new SQLiteCommand(dbConnection) { CommandText = configExistsSql };

                configExistsQuery.Parameters.AddWithValue("@configName", configName);
                reader = (Int64)configExistsQuery.ExecuteScalar();

                dbConnection.Close();
            }

            return reader.Equals(1);
        }

        #endregion

        public List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            List<int> volumes = new List<int>();

            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT volume " +
                                    "FROM volumes " +
                                    "WHERE intersection = @intersection " +
                                    "AND detector = @detector " +
                                    "AND (dateTime BETWEEN @startDate AND @endDate);";
                query.Parameters.AddWithValue("@intersection", intersection);
                query.Parameters.AddWithValue("@detector", detector);
                query.Parameters.AddWithValue("@startDate", startDate);
                query.Parameters.AddWithValue("@endDate", endDate);
                var reader = query.ExecuteReader();
                while (reader.Read())
                {
                    volumes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();

            return volumes;
        }
    }
}
