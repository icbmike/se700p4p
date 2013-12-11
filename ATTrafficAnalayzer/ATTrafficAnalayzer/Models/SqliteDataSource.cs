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
    public class SqliteDataSource : IDataSource
    {
        private static readonly string DbPath = new SQLiteConnectionStringBuilder
            {
                DataSource = "TAdb.db3"
            }.ConnectionString;

        private static SqliteDataSource _instance;
        private static readonly object SyncLock = new object();

        /// <summary>
        ///     Initialises database tables
        /// </summary>
        private SqliteDataSource()
        {
            CreateVolumesTableIfNotExists();
            CreateApproachesTableIfNotExists();
            CreateConfigsTableIfNotExists();
            CreateMonthlySummariesTableIfNotExists();
        }

        #region Helper functions

        /// <summary>
        ///     Allows the programmer to run a query against the Database
        /// </summary>
        /// <param name="sql">The SQL to run</param>
        /// <returns>A DataTable containing the result set</returns>
        private static DataTable GetDataTable(string sql)
        {
            var dataTable = new DataTable();

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                using (var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql })
                {
                    using (SQLiteDataReader reader = createConfigsTableCommand.ExecuteReader())
                    {
                        dataTable.Load(reader);
                    }
                }
                dbConnection.Close();
            }
            return dataTable;
        }

        /// <summary>
        ///     Gets an adapter to the database
        /// </summary>
        /// <param name="sql">The sql query</param>
        /// <returns>A data adapter for the given query</returns>
        private static SQLiteDataAdapter GetDataAdapter(string sql)
        {
            return GetDataAdapter(sql, null);
        }

        /// <summary>
        ///     Gets an adapter to the database
        /// </summary>
        /// <param name="sql">The sql query</param>
        /// <param name="parameters">The parameters for the sql</param>
        /// <returns>A data adapter for the given query</returns>
        private static SQLiteDataAdapter GetDataAdapter(string sql, Dictionary<string, object> parameters)
        {
            var dbConnection = new SQLiteConnection(DbPath);
            dbConnection.Open();

            var command = new SQLiteCommand(dbConnection) { CommandText = sql };

            if (parameters != null)
            {
                foreach (var parameterName in parameters.Keys)
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
            int rowsUpdated;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                using (var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql })
                {
                    rowsUpdated = createConfigsTableCommand.ExecuteNonQuery();
                }

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

                using (var createConfigsTableCommand = new SQLiteCommand(dbConnection) { CommandText = sql })
                {
                    reader = createConfigsTableCommand.ExecuteScalar();
                }

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
            var valuesToUpdate = "";
            var returnCode = true;

            if (data.Count >= 1)
            {
                valuesToUpdate = data.Aggregate(valuesToUpdate, (current, val) => current + String.Format(" {0} = {1},", val.Key, val.Value));
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
            var returnCode = true;
            try
            {
                System.Windows.Forms.MessageBox.Show(String.Format("DELETE FROM {0} WHERE {1};", tableName, where));
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
            try
            {
                var tables = GetDataTable("select NAME from SQLITE_MASTER where type='table' order by NAME;");
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

        /// <summary>
        /// Create the configs table in the database
        /// </summary>
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

        /// <summary>
        /// Create the summaries table in the database
        /// </summary>
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

        /// <summary>
        /// Create the approaches table in the database
        /// </summary>
        private static void CreateApproachesTableIfNotExists()
        {
            const string createApproachesTableSql = @"CREATE TABLE IF NOT EXISTS [approaches] ( 
                                    [id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                                    [approach] TEXT  NULL
                                )";
            ExecuteNonQuery(createApproachesTableSql);
        }

        /// <summary>
        /// Create the volumes table in the database
        /// </summary>
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

        /// <summary>
        ///     Imports a single file
        /// </summary>
        /// <param name="b">Worker thread</param>
        /// <param name="w"></param>
        /// <param name="filename">filename to import</param>
        /// <param name="updateProgress"></param>
        /// <param name="getDuplicatePolicy"></param>
        /// <returns></returns>
        public DuplicatePolicy ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            //Open the db connection
            FileStream fs;
            DuplicatePolicy duplicatePolicy;
            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                //Load the file into memory
                fs = new FileStream(filename, FileMode.Open);
                var sizeInBytes = (int)fs.Length;
                var byteArray = new byte[sizeInBytes];
                fs.Read(byteArray, 0, sizeInBytes);

                var alreadyLoaded = false;

                //Now decrypt it
                var index = 0;
                DateTimeRecord currentDateTime = null;
                duplicatePolicy = DuplicatePolicy.Continue;
                var continuing = false;

                using (var cmd = new SQLiteCommand(dbConnection))
                {
                    using (var transaction = dbConnection.BeginTransaction())
                    {
                        while (index < sizeInBytes) //seek through the byte array untill we reach the end
                        {
                            var recordSize = byteArray[index] + byteArray[index + 1] * 256;
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
                            var recordType = VolumeRecordFactory.CheckRecordType(record);

                            //Construct the appropriate record type
                            switch (recordType)
                            {
                                case VolumeRecordType.Datetime:
                                    currentDateTime = VolumeRecordFactory.CreateDateTimeRecord(record);
                                    break;
                                case VolumeRecordType.Volume:
                                    var volumeRecord = VolumeRecordFactory.CreateVolumeRecord(record, recordSize);

                                    foreach (var detector in volumeRecord.GetDetectors())
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
                                            if (e.ErrorCode.Equals(SQLiteErrorCode.Constraint) && !continuing)
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
                                                continuing = true;
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
            }
            fs.Close();
            return duplicatePolicy;
        }

        /// <summary>
        ///     Get a list of intersections
        /// </summary>
        /// <returns>All intersections which have volume data in the database</returns>
        public List<int> GetIntersections()
        {
            List<int> intersections;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                intersections = new List<int>();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT DISTINCT intersection FROM volumes;";
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                            intersections.Add(reader.GetInt32(0));
                    }
                }
                conn.Close();
            }
            return intersections;
        }

        /// <summary>
        ///     Get a list of detectors at a specified intersection
        /// </summary>
        /// <param name="intersection">The intersection ID</param>
        /// <returns>List of detectors</returns>
        public List<int> GetDetectorsAtIntersection(int intersection)
        {
            List<int> detectors;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                detectors = new List<int>();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT DISTINCT detector FROM volumes WHERE intersection = @intersection;";
                    query.Parameters.AddWithValue("@intersection", intersection);
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detectors.Add(reader.GetInt32(0));
                        }
                    }
                }
                conn.Close();
            }
            return detectors;
        }

        /// <summary>
        ///     Get the volumes for a detector for a 5 min period at a specific datetime
        /// </summary>
        /// <param name="intersection">Intersection ID</param>
        /// <param name="detector">Detector ID</param>
        /// <param name="dateTime">Date & time</param>
        /// <returns>Traffic volume for the five minute period</returns>
        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            int volume;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText =
                        "SELECT volume from volumes WHERE intersection = '@intersection' AND detector = '@detector' AND dateTime = '@dateTime';";

                    query.Parameters.AddWithValue("@intersection", intersection);
                    query.Parameters.AddWithValue("@detector", detector);
                    query.Parameters.AddWithValue("@dateTime", dateTime);

                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.RecordsAffected != 1)
                        {
                            throw new Exception("WHOAH");
                        }
                        volume = reader.GetInt32(0);
                    }
                }
                conn.Close();
            }
            return volume;
        }

        /// <summary>
        ///     Get the volumes for a detector at a specific datetime
        /// </summary>
        /// <param name="intersection">Intersection ID</param>
        /// <param name="detectorList">List of detector to query volumes for</param>
        /// <param name="startDateTime">Start of the period</param>
        /// <param name="endDateTime">End of the period</param>
        /// <returns>The total volumes for the detectors between the start and end date</returns>
        public int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime)
        {
            int volume;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                volume = 0;
                using (var query = new SQLiteCommand(conn))
                {
                    foreach (var detector in detectorList)
                    {
                        query.CommandText =
                            "SELECT volume " +
                            "FROM volumes " +
                            "WHERE intersection = @intersection " +
                            "AND detector = @detector " +
                            "AND (dateTime BETWEEN @startDateTime AND @endDateTime);";

                        query.Parameters.AddWithValue("@intersection", intersection);
                        query.Parameters.AddWithValue("@detector", detector);
                        query.Parameters.AddWithValue("@startDateTime", startDateTime);
                        query.Parameters.AddWithValue("@endDateTime", endDateTime);

                        volume += Convert.ToInt32(query.ExecuteScalar());
                    }
                }
                conn.Close();
            }
            return volume;
        }

        /// <summary>
        ///     Confirms if there is no data in the volumes table
        /// </summary>
        /// <returns>True if there is no data</returns>
        public bool VolumesTableEmpty()
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                const string volumesNotEmptySql = "SELECT EXISTS(SELECT 1 FROM volumes LIMIT 1);";
                using (var volumesNotEmptyCmd = new SQLiteCommand(dbConnection) { CommandText = volumesNotEmptySql })
                {
                    reader = (Int64)volumesNotEmptyCmd.ExecuteScalar();
                }

                dbConnection.Close();
            }

            return !reader.Equals(1);
        }

        void IDataSource.ClearData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get the volumes for a single detector at a specific datetime
        /// </summary>
        /// <param name="intersection">Intersection ID</param>
        /// <param name="detector">Detector ID</param>
        /// <param name="startDate">datetime at the start of the period</param>
        /// <param name="endDate">datetime at the end of the period</param>
        /// <returns>Traffic volume for the five minute period</returns>
        public List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate)
        {
            List<int> volumes;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                volumes = new List<int>();

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
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            volumes.Add(reader.GetInt32(0));
                        }
                    }
                }
                conn.Close();
            }

            return volumes;
        }

        /// <summary>
        ///     Checks if volumes exist on a date
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if there are volumes for the day</returns>
        public Boolean VolumesExist(DateTime date)
        {
            return VolumesExist(date, date.AddDays(1));
        }

        /// <summary>
        ///     Checks if there are volumes over a period of time
        /// </summary>
        /// <param name="startDate">Date at the start of the period</param>
        /// <param name="endDate">Date at the end of the period</param>
        /// <returns>True if there are volumes between the dates</returns>
        public Boolean VolumesExist(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddSeconds(-1);
            Boolean result;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT * " +
                                        "FROM volumes " +
                                        "WHERE (datetime BETWEEN @startDate AND @endDate);";
                    query.Parameters.AddWithValue("@startDate", startDate);
                    query.Parameters.AddWithValue("@endDate", endDate);
                    using (var reader = query.ExecuteReader())
                    {
                        result = reader.HasRows;
                    }
                }
                conn.Close();
            }
            return result;
        }
        
        /// <summary>
        ///     Removes the traffic volume for a day
        /// </summary>
        /// <param name="date">Day</param>
        /// <returns>True if the deletion was successful</returns>
        public bool RemoveVolumes(DateTime date)
        {
            var returnCode = true;
            try
            {
                using (var dbConnection = new SQLiteConnection(DbPath))
                {
                    dbConnection.Open();

                    using (var query = new SQLiteCommand(dbConnection))
                    {
                        query.CommandText = "DELETE FROM volumes WHERE (dateTime BETWEEN @startDate AND @endDate);";
                        query.Parameters.AddWithValue("@startDate", date);
                        query.Parameters.AddWithValue("@endDate", date.AddDays(1).AddSeconds(-1));
                        query.ExecuteNonQuery();
                    }

                    dbConnection.Close();
                }
            }
            catch (Exception)
            {
                returnCode = false;
            }
            return returnCode;
        }

        /// <summary>
        ///     Checks if there are traffic volumes for an interseciton date
        /// </summary>
        /// <param name="startDate">date at the start of the time period</param>
        /// <param name="endDate">date at the end of the time period</param>
        /// <param name="intersection">intersection ID</param>
        /// <returns>True if volumes exist</returns>
        public Boolean VolumesExist(DateTime startDate, DateTime endDate, int intersection)
        {
            endDate = endDate.AddSeconds(-1);
            Boolean result;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT * " +
                                        "FROM volumes " +
                                        "WHERE intersection = @intersection " +
                                        "AND (datetime BETWEEN @startDate AND @endDate);";
                    query.Parameters.AddWithValue("@startDate", startDate);
                    query.Parameters.AddWithValue("@endDate", endDate);
                    query.Parameters.AddWithValue("@intersection", intersection);
                    using (var reader = query.ExecuteReader())
                    {
                        result = reader.HasRows;
                    }
                }
                conn.Close();
            }
            return result;
        }

        /// <summary>
        ///     Gets the daily total volumes for a list of detectors at an intersection
        /// </summary>
        /// <param name="date">Day</param>
        /// <param name="intersection">Intersection ID</param>
        /// <param name="detectors">List of detector IDs</param>
        /// <returns>Daily total</returns>
        public int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors)
        {
            int volume;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                volume = 0;
                using (var query = new SQLiteCommand(conn))
                {
                    foreach (var detector in detectors)
                    {
                        query.CommandText =
                            "SELECT SUM(volume) " +
                            "FROM volumes " +
                            "WHERE intersection = @intersection " +
                            "AND detector = @detector " +
                            "AND (dateTime BETWEEN @startDateTime AND @endDateTime);";

                        query.Parameters.AddWithValue("@intersection", intersection);
                        query.Parameters.AddWithValue("@detector", detector);
                        query.Parameters.AddWithValue("@startDateTime", date);
                        query.Parameters.AddWithValue("@endDateTime", date.AddDays(1));

                        volume += Convert.ToInt32(query.ExecuteScalar());
                    }
                }
                conn.Close();
            }
            return volume;
        }

        #endregion

        #region Configuration Related Methods

        /// <summary>
        ///     Adapater to the report configuration database table
        /// </summary>
        /// <returns>Data adapter</returns>
        public SQLiteDataAdapter GetConfigsDataAdapter()
        {
            const string getCongifsSql = "SELECT name FROM configs;";
            return GetDataAdapter(getCongifsSql);
        }

        /// <summary>
        ///     Date of the most recently imported traffic volumes
        /// </summary>
        /// <returns>Date</returns>
        public DateTime GetMostRecentImportedDate()
        {
            DateTime mostRecentDay;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = "SELECT MAX(dateTime) FROM volumes;";
                    var result = command.ExecuteScalar();
                    DateTime maxDate;

                    maxDate = result != DBNull.Value ? Convert.ToDateTime(result) : DateTime.Today.AddDays(-1);
                    mostRecentDay = new DateTime(maxDate.Year, maxDate.Month, maxDate.Day);
                }
            }
            return mostRecentDay;
        }

        /// <summary>
        ///     Retrieve a report configuration
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <returns>configuration</returns>
        public Configuration.Configuration GetConfiguration(string name)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                JObject configJson = null;

                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT config FROM configs WHERE name = @name;";
                    query.Parameters.AddWithValue("@name", name);
                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.Read())
                            configJson = JObject.Parse(reader.GetString(0));
                    }
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

                            JObject approachJson;
                            using (var reader = query.ExecuteReader())
                            {
                                approachJson = null;

                                if (reader.Read())
                                    approachJson = JObject.Parse(reader.GetString(0));
                            }

                            approaches.Add(new Approach((string)approachJson["name"],
                                                        approachJson["detectors"].Select(t => (int)t).ToList(), this));
                        }
                    }
                    conn.Close();
                    return new Configuration.Configuration(name, (int)configJson["intersection"], approaches);
                }
                conn.Close();
            }
            return null;
        }

        /// <summary>
        ///     Get a list of all reports
        /// </summary>
        /// <returns>List of all reports</returns>
        public List<string> GetReportNames()
        {
            var names = new List<string>();
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = "SELECT name FROM configs;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return names;
        }

        /// <summary>
        ///     Create a report record in the database
        /// </summary>
        /// <param name="config">Configuration configuration</param>
        public void AddConfiguration(Configuration.Configuration config)
        {
            var configJson = config.ToJson();
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                foreach (var approach in config.Approaches)
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
                conn.Close();
            }
        }

        /// <summary>
        ///     List of all approaches in a configuration
        /// </summary>
        /// <param name="configName">Config name</param>
        /// <returns>List of approaches</returns>
        public List<Approach> GetApproaches(String configName)
        {
            return GetConfiguration(configName).Approaches;
        }

        /// <summary>
        ///     Checks if a report exists
        /// </summary>
        /// <param name="name">Configuration name</param>
        /// <returns>True if it exists</returns>
        public bool ReportExists(String name)
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                const string configExistsSql = "SELECT EXISTS(SELECT 1 FROM configs WHERE name = @configName LIMIT 1);";
                using (var configExistsQuery = new SQLiteCommand(dbConnection) { CommandText = configExistsSql })
                {
                    configExistsQuery.Parameters.AddWithValue("@configName", name);
                    reader = (Int64)configExistsQuery.ExecuteScalar();
                }

                dbConnection.Close();
            }

            return reader.Equals(1);
        }

        #endregion

        #region Summary Related Methods

        /// <summary>
        ///     Update summary configuration in the database
        /// </summary>
        /// <param name="configName">Summary name</param>
        /// <param name="rows">New configuration contents</param>
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
                conn.Close();
            }
        }

        /// <summary>
        ///     Delete report
        /// </summary>
        /// <param name="name">nameReport </param>
        public void RemoveReport(string name)
        {
            RemoveConfig(name, true);
        }

        public void RemoveSummary(string name)
        {
            RemoveConfig(name, false);
        }

        /// <summary>
        ///     Delete the summary/report config from the database
        /// </summary>
        /// <param name="name">summary/report name</param>
        /// <param name="report">not used</param>
        private void RemoveConfig(string name, bool report) //or summary
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = report ? "DELETE FROM configs WHERE name = @name" : "DELETE FROM monthly_summaries WHERE name = @name";
                    query.Parameters.AddWithValue("@name", name);

                    query.ExecuteNonQuery();

                }
                conn.Close();
            }
        }

        /// <summary>
        ///     Get the summary configuration to create a report
        /// </summary>
        /// <param name="name">Name of summary</param>
        /// <returns>Rows in the Summary</returns>
        public IEnumerable<SummaryRow> GetSummaryConfig(string name)
        {
            var summaries = new List<SummaryRow>();

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                using (var query = new SQLiteCommand(conn))
                {
                    query.CommandText = "SELECT config FROM monthly_summaries WHERE name = @name";
                    query.Parameters.AddWithValue("@name", name);

                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var configArray = JArray.Parse(reader.GetString(0));
                            summaries.AddRange(configArray.Select(summaryJson => new SummaryRow((string)summaryJson["route_name"], (int)summaryJson["intersection_in"], (int)summaryJson["intersection_out"], summaryJson["detectors_in"].Select(t => (int)t).ToList(), summaryJson["detectors_out"].Select(t => (int)t).ToList(), (int)summaryJson["div_factor_in"], (int)summaryJson["div_factor_out"])));
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return summaries;
        }

        /// <summary>
        ///     Name of all monthly summaries
        /// </summary>
        /// <returns>List of summary names</returns>
        public List<string> GetSummaryNames()
        {
            var names = new List<string>();
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = "SELECT name FROM monthly_summaries;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
                        reader.Close();
                    }
                }
            }
            return names;
        }

        /// <summary>
        ///     Checks if there is a summary with the given name
        /// </summary>
        /// <param name="name">summary name</param>
        /// <returns>True if the summary exists</returns>
        public bool SummaryExists(String name)
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                const string configExistsSql = "SELECT EXISTS(SELECT 1 FROM monthly_summaries WHERE name = @name LIMIT 1);";
                using (var configExistsQuery = new SQLiteCommand(dbConnection) { CommandText = configExistsSql })
                {
                    configExistsQuery.Parameters.AddWithValue("@name", name);
                    reader = (Int64)configExistsQuery.ExecuteScalar();
                }

                dbConnection.Close();
            }

            return reader.Equals(1);
        }

        #endregion

        #region Miscellaneous

        /// <summary>
        ///     Dates of the data in the database
        /// </summary>
        /// <returns>Dates</returns>
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
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            importedDates.Add(reader.GetDateTime(0));
                        }
                    }
                }
                conn.Close();
            }
            return importedDates;
        }

        /// <summary>
        ///     Data adapter to the faults table
        /// </summary>
        /// <param name="startDate">date at the start of the time period</param>
        /// <param name="endDate">date at the end of the time period</param>
        /// <param name="faultThreshold">return all detectors with a value of this threshold</param>
        /// <returns>data adapter</returns>
        public SQLiteDataAdapter GetFaultsDataAdapter(DateTime startDate, DateTime endDate, int faultThreshold)
        {
            const string sql = "SELECT intersection as 'Intersection', group_concat(detector) as 'Faulty detectors'" +
                               "FROM volumes WHERE volume > @faultThreshold  AND (dateTime BETWEEN @startDate AND @endDate)" +
                               "GROUP BY intersection";
            return GetDataAdapter(sql, new Dictionary<string, object> { { "@startDate", startDate }, { "@endDate", endDate }, { "@faultThreshold", faultThreshold } });
        }

        #endregion

        /// <summary>
        ///     Provides the db helper singleton
        /// </summary>
        /// <returns>DB Helper instance</returns>
        public static SqliteDataSource GetDbHelper()
        {
            lock (SyncLock)
            {
                return _instance ?? (_instance = new SqliteDataSource());
            }
        }

        /// <summary>
        ///     Returns a data adapter for the summary table
        /// </summary>
        /// <returns>Data adapter</returns>
        public SQLiteDataAdapter GetMonthlySummariesDataAdapter()
        {
            return GetDataAdapter("SELECT name FROM monthly_summaries;");
        }

        /// <summary>
        ///     Checks if there are traffic volumes in the database for a given month
        /// </summary>
        /// <param name="month">Month number</param>
        /// <returns>True if there are volumes</returns>
        public bool VolumesExistForMonth(int month)
        {
            return true; //Needs to be implemented
        }
                           
        /// <summary>
        ///     Get a list of all faulty detectors between two dates
        /// </summary>
        /// <param name="startDate">date at the start of the time period</param>
        /// <param name="endDate">date at the end of the time period</param>
        /// <param name="threshold">list all detectors with values over this value</param>
        /// <returns>Detectors by intersection</returns>
        public Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold)
        {
            var suspectedFaults = new Dictionary<int, List<int>>();

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = new SQLiteCommand(conn))
                {
                    command.CommandText = "SELECT intersection as 'Intersection', group_concat(detector) as 'Faulty detectors'" +
                               "FROM volumes WHERE volume > @faultThreshold  AND (dateTime BETWEEN @startDate AND @endDate)" +
                               "GROUP BY intersection";
                    command.Parameters.AddWithValue("@faultThreshold", threshold);
                    command.Parameters.AddWithValue("@startDate", startDate);
                    command.Parameters.AddWithValue("@endDate", endDate);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var currentIntersection = reader.GetInt32(0);
                            suspectedFaults.Add(currentIntersection, new List<int>());
                            foreach (var detector in reader.GetString(1).Split(new[] { "," }, StringSplitOptions.None))
                            {
                                suspectedFaults[currentIntersection].Add(int.Parse(detector));
                            }

                        }
                    }
                }
            }

            return suspectedFaults;
        }
    }
}
