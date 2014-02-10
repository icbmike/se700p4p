using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models
{
    public class SqliteDataSource : IDataSource
    {
        private static readonly string DbPath = new SQLiteConnectionStringBuilder
        {
            DataSource = "TAdb.db3",
            JournalMode = SQLiteJournalModeEnum.Off,
            ForeignKeys = true
        }.ConnectionString;

        public SqliteDataSource()
        {
            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection(DbPath))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [monthly_summaries] (
                                                             [name] nvarchar(100) NOT NULL
                                                           , [config] ntext NULL
                                                           , [last_used] datetime NULL
                                                            , CONSTRAINT [PK_monthly_summaries] PRIMARY KEY ([name])
                                                        );";
                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [intersections] (
                                              [intersection_id] int NOT NULL
                                            , [detector] tinyint NOT NULL
                                            , CONSTRAINT [PK_intersections] PRIMARY KEY ([intersection_id],[detector])
                                            );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [volumes] (
                                                      [intersection] int NOT NULL
                                                    , [detector] tinyint NOT NULL
                                                    , [dateTime] datetime NOT NULL
                                                    , [volume] smallint NULL
                                                    , CONSTRAINT [PK_volumes] PRIMARY KEY ([intersection],[detector],[dateTime])
                                                    , FOREIGN KEY ([intersection], [detector]) REFERENCES [intersections] ([intersection_id], [detector]) ON DELETE CASCADE ON UPDATE CASCADE
                                                    );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [configs] (
                                              [config_id] INTEGER NOT NULL
                                            , [name] nvarchar(100) NOT NULL UNIQUE ON CONFLICT ABORT
                                            , [date_last_used] datetime NULL
                                            , [intersection_id] int NULL
                                            , CONSTRAINT [PK_configs] PRIMARY KEY ([config_id])
                                            );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [config_approach_mapping] (
                                                    [config_id] int NOT NULL
                                                , [approach_id] int NOT NULL
                                                , CONSTRAINT [PK_config_approach_mapping] PRIMARY KEY ([config_id],[approach_id])
                                                , FOREIGN KEY ([config_id]) REFERENCES [configs] ([config_id]) ON DELETE CASCADE ON UPDATE CASCADE
                                                );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [approaches] (
                                                  [approach_id] INTEGER NOT NULL
                                                , [name] nvarchar(100) NULL
                                                , CONSTRAINT [PK_approaches] PRIMARY KEY ([approach_id])
                                                );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [approach_detector_mapping] (
                                              [approach_id] int NOT NULL
                                            , [detector] tinyint NOT NULL
                                            , CONSTRAINT [PK_approach_detector_mapping] PRIMARY KEY ([approach_id],[detector])
                                            , FOREIGN KEY ([approach_id]) REFERENCES [approaches] ([approach_id]) ON DELETE CASCADE ON UPDATE CASCADE
                                            );";

                        command.ExecuteNonQuery();


                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [red_light_running_configurations] (
                                               [id] INTEGER NOT NULL
                                              ,[name] nvarchar(100) NOT NULL
                                              ,CONSTRAINT [PK_red_light_running_configurations] PRIMARY KEY ([id])
                                              );";

                        command.ExecuteNonQuery();

                        command.CommandText = @"CREATE TABLE IF NOT EXISTS [red_light_running_site_mapping] (
                                                [red_light_running_config_id] int NOT NULL
                                              , [site_config_id] int NOT NULL
                                              , CONSTRAINT [PK_red_light_running_site_mapping] PRIMARY KEY ([red_light_running_config_id], [site_config_id])
                                              , FOREIGN KEY ([red_light_running_config_id]) REFERENCES [red_light_running_configurations] ([id]) ON DELETE CASCADE ON UPDATE CASCADE
                                              , FOREIGN KEY ([site_config_id]) REFERENCES [configs] ([config_id]) ON DELETE CASCADE ON UPDATE CASCADE
                                              );";

                        command.ExecuteNonQuery();

                        command.CommandText =
                            @"CREATE UNIQUE INDEX IF NOT EXISTS [UQ__monthly_summaries__0000000000000050] ON [monthly_summaries] ([name] ASC);";
                        command.ExecuteNonQuery();

                        command.CommandText =
                            @"CREATE INDEX IF NOT EXISTS [volumes_index] ON [volumes] ([intersection] ASC,[detector] ASC,[dateTime] ASC);";
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                connection.Close();
            }
        }

        #region Helper functions

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

            var command = new SQLiteCommand(dbConnection) {CommandText = sql};

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
        public void ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            //Open the db connection
            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                var shouldStopImporting = false;
                var continuing = false;

                var decodedFile = VolumeStoreDecoder.DecodeFile(filename);

                //Use transactions for sick Database rollbacks
                using (var transaction = dbConnection.BeginTransaction())
                {
                    using (var command = dbConnection.CreateCommand())
                    {
                        for (var index = 0; index < decodedFile.Count; index++)
                        {
                            var dateTimeRecord = decodedFile[index];

                            foreach (var volumeRecord in dateTimeRecord.VolumeRecords)
                            {
                                //Check if the intersection for this volume record is already in the database

                                command.CommandText =
                                    "SELECT COUNT(intersection_id) FROM intersections WHERE intersection_id = @intersection_id;";
                                
                                command.Parameters.Clear();
                                command.Parameters.AddWithValue("@intersection_id",
                                    volumeRecord.IntersectionNumber);
                                var intersectionExists = (Int64) command.ExecuteScalar() > 0;

                                foreach (var detector in volumeRecord.GetDetectors())
                                {
                                    if (!intersectionExists)
                                    {
                                        command.CommandText =
                                            "INSERT INTO intersections (intersection_id, detector) VALUES (@intersection_id, @detector);";

                                        command.Parameters.Clear();
                                        command.Parameters.AddWithValue("@intersection_id",
                                            volumeRecord.IntersectionNumber);
                                        command.Parameters.AddWithValue("@detector", detector);
                                        try
                                        {
                                            command.ExecuteNonQuery();
                                        }
                                        catch (SQLiteException e)
                                        {
                                            Console.WriteLine(e);
                                        }
                                    }

                                    command.CommandText =
                                        "INSERT INTO volumes (dateTime, intersection, detector, volume) VALUES (@dateTime, @intersection, @detector, @volume);";

                                    command.Parameters.Clear();

                                    command.Parameters.AddWithValue("@dateTime", dateTimeRecord.DateTime.AddMinutes(-5));

                                    //Make up for the fact that volumes are offset ahead 5 minutes
                                    command.Parameters.AddWithValue("@intersection", volumeRecord.IntersectionNumber);
                                    command.Parameters.AddWithValue("@detector", detector);
                                    command.Parameters.AddWithValue("@volume",
                                        volumeRecord.GetVolumeForDetector(detector));

                                    try
                                    {
                                        command.ExecuteNonQuery();
                                    }
                                    catch (SQLiteException e)
                                    {
                                        if (e.ErrorCode.Equals(SQLiteErrorCode.Constraint) && continuing) continue;

                                        var duplicatePolicy = getDuplicatePolicy();
                                        if (!duplicatePolicy.Equals(DuplicatePolicy.Continue))
                                        {
                                            shouldStopImporting = true;
                                            break;
                                        }
                                        continuing = true;
                                    }
                                }
                                if (shouldStopImporting) break;
                            }
                            if (shouldStopImporting) break;

                            updateProgress((int) (((float) index/decodedFile.Count)*100));
                        }
                    }
                    transaction.Commit();
                }
                dbConnection.Close();
            }
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
                    query.CommandText = "SELECT DISTINCT intersection_id FROM intersections;";
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
                using (var query = conn.CreateCommand())
                {
                    query.CommandText =
                        "SELECT DISTINCT detector FROM intersections WHERE intersection_id = @intersection;";
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
        public int GetTotalVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime,
            DateTime endDateTime)
        {
            int totalVolume;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                totalVolume = 0;
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

                        totalVolume += Convert.ToInt32(query.ExecuteScalar());
                    }
                }
                conn.Close();
            }
            return totalVolume;
        }

        /// <summary>
        ///     Confirms if there is no data in the volumes table
        /// </summary>
        /// <returns>True if there is no data</returns>
        public bool VolumesExist()
        {
            long reader;

            using (var dbConnection = new SQLiteConnection(DbPath))
            {
                dbConnection.Open();

                const string volumesNotEmptySql = "SELECT EXISTS(SELECT 1 FROM volumes LIMIT 1);";
                using (var volumesNotEmptyCmd = new SQLiteCommand(dbConnection) {CommandText = volumesNotEmptySql})
                {
                    reader = (Int64) volumesNotEmptyCmd.ExecuteScalar();
                }

                dbConnection.Close();
            }

            return !reader.Equals(1);
        }


        public void ClearData()
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "DELETE FROM approaches;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM approach_detector_mapping;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM configs;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM config_approach_mapping;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM intersections;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM monthly_summaries;";
                    command.ExecuteNonQuery();

                    command.CommandText = "DELETE FROM volumes;";
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        public void AddIntersection(int intersection, IEnumerable<int> detectors)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText =
                        "INSERT INTO intersections (intersection_id, detector) VALUES (@intersection_id, @detector);";
                    foreach (var detector in detectors)
                    {
                        query.Parameters.Clear();
                        query.Parameters.AddWithValue("@intersection_id", intersection);
                        query.Parameters.AddWithValue("@detector", detector);
                        query.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        public List<string> GetRedLightRunningConfigurationNames()
        {
            var names = new List<string>();

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT name FROM red_light_running_configurations;";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return names;
        }

        public void RemoveRedLightRunningConfiguration(string name)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "DELETE FROM red_light_running_configurations WHERE name = @name;";
                    command.Parameters.AddWithValue("@name", name);
                    command.ExecuteNonQuery();
                }
            }
        }

        public RedLightRunningConfiguration GetRedLightRunningConfiguration(string name)
        {
            var reportConfigurations = new List<ReportConfiguration.ReportConfiguration>();
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var command = conn.CreateCommand())
                {
                    //Time for a sick join
                    command.CommandText = @"SELECT configs.config_id, configs.name, configs.intersection_id, approaches.approach_id, approaches.name, approach_detector_mapping.detector
                                            FROM red_light_running_configurations AS RLRconfigs
                                            INNER JOIN red_light_running_site_mapping AS RLRsiteMappings
                                            ON RLRconfigs.id = RLRsiteMappings.red_light_running_config_id
                                            INNER JOIN configs
                                            ON RLRsiteMappings.site_config_id = configs.config_id
                                            INNER JOIN config_approach_mapping
                                            ON configs.config_id = config_approach_mapping.config_id
                                            INNER JOIN approaches
                                            ON approaches.approach_id = config_approach_mapping.approach_id
                                            INNER JOIN approach_detector_mapping
                                            ON approaches.approach_id = approach_detector_mapping.approach_id
                                            WHERE RLRconfigs.name = @name";
                    command.Parameters.AddWithValue("@name", name);
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read()) //No results
                            return new RedLightRunningConfiguration {Name = name, Sites = reportConfigurations};
                        
                        var configId = reader.GetInt32(0);
                        var approachId = reader.GetInt32(3);

                        var currentApproach = new Approach(reader.GetString(4), new List<int> {reader.GetByte(5)}, this);
                        var currentConfig = new ReportConfiguration.ReportConfiguration(reader.GetString(1),  reader.GetInt32(2),
                            new List<Approach>(), this);

                        while (reader.Read())
                        {
                            if (reader.GetInt32(0) != configId) //Starting a new config and a new approach
                            {
                                //Add current approach to current config
                                currentConfig.Approaches.Add(currentApproach);
                                //Add current config to config list
                                reportConfigurations.Add(currentConfig);
                                //Reset both
                                currentConfig = new ReportConfiguration.ReportConfiguration(reader.GetString(1),reader.GetInt32(2), new List<Approach>(), this);
                                currentApproach = new Approach(reader.GetString(4), new List<int>(), this);
                                //Reset configId and approachId
                                configId = reader.GetInt32(0);
                                approachId = reader.GetInt32(3);
                            }

                            else if (reader.GetInt32(3) != approachId) //Only starting a new approach 
                            {
                                //Add currentApproach to currentConfig
                                currentConfig.Approaches.Add(currentApproach);
                                //Start a new one
                                currentApproach = new Approach(reader.GetString(4), new List<int>(), this);
                                //Reset approachId
                                approachId = reader.GetInt32(3);
                            }

                            currentApproach.Detectors.Add(reader.GetByte(5));
                        }

                        //Add final things to that which they should belong to
                        currentConfig.Approaches.Add(currentApproach);
                        //Add current config to config list
                        reportConfigurations.Add(currentConfig);
                    }
                }
            }

            return new RedLightRunningConfiguration {Name = name, Sites = reportConfigurations};
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
            return VolumesExistForDateRange(date, date.AddDays(1));
        }

        /// <summary>
        ///     Checks if there are volumes over a period of time
        /// </summary>
        /// <param name="startDate">Date at the start of the period</param>
        /// <param name="endDate">Date at the end of the period</param>
        /// <returns>True if there are volumes between the dates</returns>
        public Boolean VolumesExistForDateRange(DateTime startDate, DateTime endDate)
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
        public void RemoveVolumes(DateTime date)
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

        /// <summary>
        ///     Checks if there are traffic volumes for an interseciton date
        /// </summary>
        /// <param name="startDate">date at the start of the time period</param>
        /// <param name="endDate">date at the end of the time period</param>
        /// <param name="intersection">intersection ID</param>
        /// <returns>True if volumes exist</returns>
        public Boolean VolumesExistForDateRange(DateTime startDate, DateTime endDate, int intersection)
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
                           @"SELECT ifnull(SUM(volume), 0) 
                            FROM volumes 
                            WHERE intersection = @intersection 
                            AND detector = @detector 
                            AND (dateTime BETWEEN @startDateTime AND @endDateTime);";

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
        public ReportConfiguration.ReportConfiguration GetConfiguration(string name)
        {
            ReportConfiguration.ReportConfiguration result = null;
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                using (var query = conn.CreateCommand())
                {
                    query.CommandText =
                        @"SELECT configs.intersection_id, approaches.approach_id, approaches.name, approach_detector_mapping.detector 
                        FROM configs 
                        INNER JOIN config_approach_mapping 
                        ON configs.config_id = config_approach_mapping.config_id 
                        INNER JOIN approaches 
                        ON approaches.approach_id = config_approach_mapping.approach_id 
                        INNER JOIN approach_detector_mapping 
                        ON approaches.approach_id = approach_detector_mapping.approach_id 
                        WHERE configs.name = @name";
                    query.Parameters.AddWithValue("@name", name);
                    using (var reader = query.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var intersection = reader.GetInt32(0);
                            var approaches = new List<Approach>();

                            var approachId = 0; //Seed id is 1, 0 should never be found in db
                            var currentApproach = new Approach(reader.GetString(2), new List<int>(), this);
                            approaches.Add(currentApproach);
                            currentApproach.Detectors.Add((reader.GetByte(3)));
                            approachId = reader.GetInt32(1);

                            while (reader.Read())
                            {
                                if (!reader.GetInt32(1).Equals(approachId))
                                {
                                    approachId = reader.GetInt32(1);
                                    currentApproach = new Approach(reader.GetString(2), new List<int>(), this);
                                    approaches.Add(currentApproach);
                                }
                                currentApproach.Detectors.Add((reader.GetByte(3)));
                            }

                            result = new ReportConfiguration.ReportConfiguration(name, intersection, approaches, this);
                        }
                    }
                }
                conn.Close();
            }
            return result;
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

        public List<string> GetConfigurationNames()
        {
            var names = new List<String>();
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT name FROM configs;";
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            names.Add(reader.GetString(0));
                        }
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
        public void AddConfiguration(ReportConfiguration.ReportConfiguration config)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    Int64 configId;

                    //Insert into configs table
                    using (var query = conn.CreateCommand())
                    {
                        query.CommandText =
                            "INSERT INTO configs (name, date_last_used, intersection_id) VALUES (@name, datetime('now'), @intersection_id);";
                        query.Parameters.AddWithValue("@name", config.Name);
                        query.Parameters.AddWithValue("@intersection_id", config.Intersection);
                        query.ExecuteNonQuery();

                        query.CommandText = " SELECT last_insert_rowid();";
                        configId = (Int64) query.ExecuteScalar();
                        Console.WriteLine("Config id: " + configId);
                    }

                    foreach (var approach in config.Approaches)
                    {
                        Int64 approachId;
                        //insert into approaches table
                        using (var query = conn.CreateCommand())
                        {
                            query.CommandText = "INSERT INTO approaches (name) VALUES (@approach);";
                            query.Parameters.AddWithValue("@approach", approach.ApproachName);
                            query.ExecuteNonQuery();

                            query.CommandText = "SELECT last_insert_rowid();";
                            approachId = (Int64) query.ExecuteScalar();
                            Console.WriteLine("Approach ID: " + approachId);
                        }

                        //insert into approach_detector_mapping 
                        using (var query = conn.CreateCommand())
                        {
                            query.CommandText =
                                "INSERT INTO approach_detector_mapping (approach_id, detector) VALUES (@approach_id, @detector)";

                            approach.Detectors.ForEach(d =>
                            {
                                query.Parameters.Clear();
                                query.Parameters.AddWithValue("@approach_id", approachId);
                                query.Parameters.AddWithValue("@detector", d);
                                query.ExecuteNonQuery();
                            });
                        }

                        //insert into config_approach_mapping
                        using (var query = conn.CreateCommand())
                        {
                            query.CommandText =
                                "INSERT INTO config_approach_mapping (config_id, approach_id) VALUES (@config_id, @approach_id);";
                            query.Parameters.AddWithValue("@config_id", configId);
                            query.Parameters.AddWithValue("@approach_id", approachId);
                            query.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                conn.Close();
            }
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
                using (var configExistsQuery = new SQLiteCommand(dbConnection) {CommandText = configExistsSql})
                {
                    configExistsQuery.Parameters.AddWithValue("@configName", name);
                    reader = (Int64) configExistsQuery.ExecuteScalar();
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
            var config = new JArray {rows.Select(row => row.ToJson())};

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

        public void RemoveConfiguration(string name)
        {
            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                //GET the ids of mapped approaches so that we can delete them
                var approachIds = new List<Int32>();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = @"SELECT approach_id FROM config_approach_mapping
                                        INNER JOIN configs
                                        WHERE config_approach_mapping.config_id = configs.config_id
                                        AND configs.name = @name";
                    query.Parameters.AddWithValue("@name", name);
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            approachIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                //DELETE THE APPROACHES
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = @"DELETE FROM approaches WHERE approach_id = @approach_id";

                    foreach (var approachId in approachIds)
                    {
                        query.Parameters.AddWithValue("@approach_id", approachId);
                        query.ExecuteNonQuery();
                    }
                }

                //DELETE THE CONFIG
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "DELETE FROM configs WHERE name = @name;";
                    query.Parameters.AddWithValue("@name", name);
                    query.ExecuteNonQuery();
                }
                conn.Close();
            }
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
                    query.CommandText = report
                        ? "DELETE FROM configs WHERE name = @name"
                        : "DELETE FROM monthly_summaries WHERE name = @name";
                    query.Parameters.AddWithValue("@name", name);

                    query.ExecuteNonQuery();
                }
                conn.Close();
            }
        }

        /// <summary>
        ///     Get the summary configuration to create a report
        /// </summary>
        /// <param name="name">ApproachName of summary</param>
        /// <returns>Rows in the Summary</returns>
        public SummaryConfiguration GetSummaryConfig(string name)
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
                            summaries.AddRange(
                                configArray.Select(
                                    summaryJson =>
                                        new SummaryRow((string) summaryJson["route_name"],
                                            (int) summaryJson["intersection_in"], (int) summaryJson["intersection_out"],
                                            summaryJson["detectors_in"].Select(t => (int) t).ToList(),
                                            summaryJson["detectors_out"].Select(t => (int) t).ToList(),
                                            (int) summaryJson["div_factor_in"], (int) summaryJson["div_factor_out"])));
                        }
                        reader.Close();
                    }
                }
                conn.Close();
            }
            return new SummaryConfiguration {SummaryRows = summaries, Name = name};
        }

        /// <summary>
        ///     ApproachName of all monthly summaries
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

                const string configExistsSql =
                    "SELECT EXISTS(SELECT 1 FROM monthly_summaries WHERE name = @name LIMIT 1);";
                using (var configExistsQuery = new SQLiteCommand(dbConnection) {CommandText = configExistsSql})
                {
                    configExistsQuery.Parameters.AddWithValue("@name", name);
                    reader = (Int64) configExistsQuery.ExecuteScalar();
                }

                dbConnection.Close();
            }

            return reader.Equals(1);
        }

        public bool ConfigurationExists(string name)
        {
            long count;

            using (var conn = new SQLiteConnection(DbPath))
            {
                conn.Open();

                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT COUNT(*) FROM configs WHERE name = @configName;";
                    query.Parameters.AddWithValue("@configName", name);
                    count = (Int64) query.ExecuteScalar();
                }

                conn.Close();
            }

            return count.Equals(1);
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
            return GetDataAdapter(sql,
                new Dictionary<string, object>
                {
                    {"@startDate", startDate},
                    {"@endDate", endDate},
                    {"@faultThreshold", faultThreshold}
                });
        }

        #endregion

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
                    command.CommandText =
                        "SELECT intersection as 'Intersection', group_concat(detector) as 'Faulty detectors'" +
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
                            foreach (var detector in reader.GetString(1).Split(new[] {","}, StringSplitOptions.None))
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