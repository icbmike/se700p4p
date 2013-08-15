using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Windows;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Volume;
using ATTrafficAnalayzer.Views.Screens;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models
{
    public class DbHelper
    {
        private const string DbPath = "Data Source=TAdb.db3";

        private static DbHelper _instance;
        private static readonly object SyncLock = new object();

        private DbHelper()
        {
            CreateVolumesTableIfNotExists();
            CreateApproachesTableIfNotExists();
            CreateConfigsTableIfNotExists();
            CreateMonthlySummariesTableIfNotExists();
        }

        #region Helper functions

        /// <summary>
        ///     Allows the programmer to run a query against the Database.
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set.</returns>
        private static DataTable GetDataTable(string sql)
        {
            var dataTable = new DataTable();

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql };
                SQLiteDataReader reader = createConfigsTableCommand.ExecuteReader();
                dataTable.Load(reader);
                reader.Close();

                dbConnection.Close();
            }

            return dataTable;
        }

        private static SQLiteDataAdapter GetDataAdapter(string sql)
        {
            return GetDataAdapter(sql, null);
        }

        private static SQLiteDataAdapter GetDataAdapter(string sql, Dictionary<string, object> parameters)
        {
            var dbConnection = new SQLiteConnection(DbPath);
            dbConnection.Open();

            var command = new SQLiteCommand(dbConnection) { CommandText = sql };

            if (parameters != null)
            {
                foreach (string parameterName in parameters.Keys)
                {
                    command.Parameters.AddWithValue(parameterName, parameters[parameterName]);
                }
            }
            var dataAdapter = new SQLiteDataAdapter(command);

            dbConnection.Close();

            return dataAdapter;
        }

        /// <summary>
        ///     Allows the programmer to interact with the database for purposes other than a query.
        /// </summary>
        /// <param name="sql">The SQL to be run.</param>
        /// <returns>An Integer containing the number of rows updated.</returns>
        private static int ExecuteNonQuery(string sql)
        {
            int rowsUpdated = 0;

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
        private static object ExecuteScalar(string sql)
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
        private static bool Update(String tableName, Dictionary<String, String> data, String where)
        {
            string valuesToUpdate = "";
            bool returnCode = true;

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
        private static bool Delete(String tableName, String where)
        {
            bool returnCode = true;
            try
            {
                ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1};", tableName, where));
            }
            catch (Exception)
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
        private static bool Insert(String tableName, Dictionary<String, String> data)
        {
            string columns = "";
            string values = "";
            bool returnCode = true;

            foreach (var val in data)
            {
                columns += String.Format(" {0},", val.Key);
                values += String.Format(" '{0}',", val.Value);
            }

            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);

            try
            {
                ExecuteNonQuery(String.Format("insert into {0}({1}) values({2});", tableName, columns, values));
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
        private static bool ClearDb()
        {
            DataTable tables;

            try
            {
                tables = GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
                foreach (DataRow table in tables.Rows)
                {
                    ClearTable(table["NAME"].ToString());
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
        private static bool ClearTable(String table)
        {
            try
            {
                ExecuteNonQuery(String.Format("delete from {0};", table));
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Table Initialization

        private static void CreateConfigsTableIfNotExists()
        {
            const string createConfigsTableSql = @"CREATE TABLE IF NOT EXISTS [configs] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";
            ExecuteNonQuery(createConfigsTableSql);
        }

        private static void CreateMonthlySummariesTableIfNotExists()
        {
            const string createVolumesTableSql = @"CREATE TABLE IF NOT EXISTS [monthly_summaries] ( 
                                    [name] TEXT  NULL,
                                    [config] TEXT  NULL,
                                    [last_used] DATETIME,

                                    PRIMARY KEY (name)
                                )";
            ExecuteNonQuery(createVolumesTableSql);
        }

        private static void CreateApproachesTableIfNotExists()
        {
            const string createApproachesTableSql = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                    [approach] TEXT  NULL
                                )";
            ExecuteNonQuery(createApproachesTableSql);
        }

        private static void CreateVolumesTableIfNotExists()
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

        public enum DuplicatePolicy
        {
            Skip, SkipAll, Continue
        }


        public static DuplicatePolicy ImportFile(BackgroundWorker b, DoWorkEventArgs w, string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            //Open the db connection
            var dbConnection = new SQLiteConnection(DbPath);
            dbConnection.Open();

            //Load the file into memory
            var fs = new FileStream(filename, FileMode.Open);
            var sizeInBytes = (int)fs.Length;
            var byteArray = new byte[sizeInBytes];
            fs.Read(byteArray, 0, sizeInBytes);

            bool alreadyLoaded = false;

            //Now decrypt it
            int index = 0;
            DateTimeRecord currentDateTime = null;
            DuplicatePolicy duplicatePolicy = DuplicatePolicy.Continue;
            bool continuing = false;

            using (var cmd = new SQLiteCommand(dbConnection))
            {
                using (SQLiteTransaction transaction = dbConnection.BeginTransaction())
                {
                    while (index < sizeInBytes) //seek through the byte array untill we reach the end
                    {
                        int recordSize = byteArray[index] + byteArray[index + 1] * 256;
                        //The record size is stored in two bytes, little endian

                        index += 2;
                        var progress = (int)(((float)index / sizeInBytes) * 100);
                        updateProgress(progress);


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
                        VolumeRecordType recordType = VolumeRecordFactory.CheckRecordType(record);

                        //Construct the appropriate record type
                        switch (recordType)
                        {
                            case VolumeRecordType.Datetime:
                                currentDateTime = VolumeRecordFactory.CreateDateTimeRecord(record);
                                break;
                            case VolumeRecordType.Volume:
                                VolumeRecord volumeRecord = VolumeRecordFactory.CreateVolumeRecord(record, recordSize);

                                foreach (int detector in volumeRecord.GetDetectors())
                                {
                                    cmd.CommandText =
                                        "INSERT INTO volumes (dateTime, intersection, detector, volume) VALUES (@dateTime, @intersection, @detector, @volume);";

                                    cmd.Parameters.Clear();

                                    cmd.Parameters.AddWithValue("@dateTime", currentDateTime.DateTime.AddMinutes(-5));
                                    //Make up for the fact that volumes are offset ahead 5 minutes
                                    cmd.Parameters.AddWithValue("@intersection", volumeRecord.IntersectionNumber);
                                    cmd.Parameters.AddWithValue("@detector", detector);
                                    cmd.Parameters.AddWithValue("@volume", volumeRecord.GetVolumeForDetector(detector));

                                    try
                                    {
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException e)
                                    {
                                        if (e.ReturnCode.Equals(SQLiteErrorCode.Constraint) && !continuing)
                                        {

                                            Logger.Error("DBHELPER",
                                                         e + "\nDetector: " + detector + "\nIntersection: " +
                                                         volumeRecord.IntersectionNumber + "\nDate Time: " +
                                                         currentDateTime.DateTime);

                                            duplicatePolicy = getDuplicatePolicy();
                                            if (!duplicatePolicy.Equals(DuplicatePolicy.Continue))
                                            {
                                                alreadyLoaded = true;
                                                break;
                                            }
                                            else
                                            {

                                                continuing = true;
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                        if (alreadyLoaded) break;
                    }
                    transaction.Commit();
                }
            }

            dbConnection.Close();
            fs.Close();
            return duplicatePolicy;
        }

        public static List<int> GetIntersections()
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            var intersections = new List<int>();
            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT DISTINCT intersection FROM volumes;";
                SQLiteDataReader reader = query.ExecuteReader();

                while (reader.Read())
                    intersections.Add(reader.GetInt32(0));
            }
            conn.Close();
            return intersections;
        }

        public List<int> GetDetectorsAtIntersection(int intersection)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            var detectors = new List<int>();
            using (var query = new SQLiteCommand(conn))
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

        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            int volume;
            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText =
                    "SELECT volume from volumes WHERE intersection = '@intersection' AND detector = '@detector' AND dateTime = '@dateTime';";

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

        public static bool VolumesTableEmpty()
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                const string volumesNotEmptySql = "SELECT EXISTS(SELECT 1 FROM volumes LIMIT 1);";
                var volumesNotEmptyCmd = new SQLiteCommand(dbConnection) { CommandText = volumesNotEmptySql };

                reader = (Int64)volumesNotEmptyCmd.ExecuteScalar();

                dbConnection.Close();
            }

            return !reader.Equals(1);
        }

        public List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            var volumes = new List<int>();

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
                //TODO change between to exclude upper limit instead of subtracting a second from endDate
                query.Parameters.AddWithValue("@endDate", endDate.AddSeconds(-1));
                SQLiteDataReader reader = query.ExecuteReader();
                while (reader.Read())
                {
                    volumes.Add(reader.GetInt32(0));
                }
            }
            conn.Close();

            return volumes;
        }

        public Boolean VolumesExistForDateRange(DateTime startDate, DateTime endDate)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();
            Boolean result;
            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT volume " +
                                    "FROM volumes " +
                                    "WHERE (dateTime BETWEEN @startDate AND @endDate);";
                query.Parameters.AddWithValue("@startDate", startDate);
                query.Parameters.AddWithValue("@endDate", endDate);
                SQLiteDataReader reader = query.ExecuteReader();
                result = reader.HasRows;
            }
            conn.Close();
            return result;
        }

        #endregion

        #region Config Related Methods

        public SQLiteDataAdapter GetConfigsDataAdapter()
        {
            const string getCongifsSql = "SELECT name FROM configs;";
            return GetDataAdapter(getCongifsSql);
        }

        public Report GetConfiguration(string name)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();

            JObject configJson = null;

            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT config FROM configs WHERE name = @name;";
                query.Parameters.AddWithValue("@name", name);
                SQLiteDataReader reader = query.ExecuteReader();

                if (reader.Read())
                    configJson = JObject.Parse(reader.GetString(0));
            }
            if (configJson != null)
            {
                var approaches = new List<Approach>();
                foreach (JToken approachID in (JArray)configJson["approaches"])
                {
                    using (var query = new SQLiteCommand(conn))
                    {
                        query.CommandText = "SELECT approach FROM approaches WHERE id = @id;";
                        query.Parameters.AddWithValue("@id", approachID);

                        SQLiteDataReader reader = query.ExecuteReader();
                        JObject approachJson = null;

                        if (reader.Read())
                            approachJson = JObject.Parse(reader.GetString(0));

                        approaches.Add(new Approach((string)approachJson["name"],
                                                    approachJson["detectors"].Select(t => (int)t).ToList()));
                    }
                }
                conn.Close();
                return new Report(name, (int)configJson["intersection"], approaches);
            }
            conn.Close();
            return null;
        }

        public void addConfiguration(Report config)
        {
            JObject configJson = config.ToJson();
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
            using (var query = new SQLiteCommand(conn))
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
            return GetConfiguration(configName).Approaches;
        }

        public bool ConfigExists(String configName)
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                string configExistsSql = "SELECT EXISTS(SELECT 1 FROM configs WHERE name = @configName LIMIT 1);";
                var configExistsQuery = new SQLiteCommand(dbConnection) { CommandText = configExistsSql };

                configExistsQuery.Parameters.AddWithValue("@configName", configName);
                reader = (Int64)configExistsQuery.ExecuteScalar();

                dbConnection.Close();
            }

            return reader.Equals(1);
        }

        #endregion

        #region Miscellaneous

        public List<DateTime> GetImportedDates()
        {
            var importedDates = new List<DateTime>();

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText =
                        "SELECT DISTINCT strftime('%Y-%m-%d', dateTime) FROM volumes ORDER BY DATE(dateTime) ASC;";
                    SQLiteDataReader reader = query.ExecuteReader();
                    while (reader.Read())
                    {
                        importedDates.Add(reader.GetDateTime(0));
                    }
                }
            }
            return importedDates;
        }

        public SQLiteDataAdapter GetFaultsDataAdapter(DateTime startDate, DateTime endDate)
        {
            const string sql = "SELECT intersection as 'Intersection', group_concat(detector) as 'Faulty detectors'" +
                               "FROM volumes WHERE volume > 100  AND (dateTime BETWEEN @startDate AND @endDate)" +
                               "GROUP BY intersection";
            return GetDataAdapter(sql, new Dictionary<string, object> { { "@startDate", startDate }, { "@endDate", endDate } });
        }

        #endregion

        public static DbHelper GetDbHelper()
        {
            lock (SyncLock)
            {
                return _instance ?? (_instance = new DbHelper());
            }
        }

        public SQLiteDataAdapter GetMonthlySummariesDataAdapter()
        {
            return GetDataAdapter("SELECT name FROM monthly_summaries;");
        }

        public bool VolumesExistForMonth(int month)
        {
            return true; //Needs to be implemented
        }

        public void SaveMonthlySummaryConfig(string configName, IEnumerable<SummaryRow> rows)
        {
            var config = new JArray { rows.Select(row => row.ToJson()) };

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = "INSERT INTO monthly_summaries (name, config) VALUES (@name, @config);";
                    command.Parameters.AddWithValue("@name", configName);
                    command.Parameters.AddWithValue("@config", config.ToString());

                    command.ExecuteNonQuery();
                }
            }
        }

        public string GetSummaryConfig(string name)
        {
            var conn = new SQLiteConnection(DbPath);
            conn.Open();

            using (var query = new SQLiteCommand(conn))
            {
                query.CommandText = "SELECT config FROM monthly_summaries WHERE name = @name";
                query.Parameters.AddWithValue("@name", name);

                SQLiteDataReader reader = query.ExecuteReader();

                if (reader.Read())
                    return reader.GetString(0);
            }
            conn.Close();
            return null;
        }
    }
}
