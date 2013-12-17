using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Volume;

namespace ATTrafficAnalayzer.Models
{
    public class SqlceDataSource : IDataSource
    {
        private readonly string _connectionString;

        public SqlceDataSource()
        {
            var connectionSb = new SqlCeConnectionStringBuilder {DataSource = "TA.sdf"};
            _connectionString = connectionSb.ConnectionString;
        }

        public int GetVolumeForTimePeriod(int intersection, IList<int> detectorList, DateTime startDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public List<int> GetVolumes(int intersection, int detector, DateTime startDate, DateTime endDate)
        {
            List<int> volumes;
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                volumes = new List<int>();

                using (var query = conn.CreateCommand())
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
                            volumes.Add(reader.GetInt16(0));
                        }
                    }
                }
                conn.Close();
            }
            return volumes;
        }

          

        public int GetTotalVolumeForDay(DateTime date, int intersection, List<int> detectors)
        {
            throw new NotImplementedException();
        }

        public bool RemoveVolumes(DateTime date)
        {
            throw new NotImplementedException();
        }

        public List<int> GetIntersections()
        {
            List<int> intersections;
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                intersections = new List<int>();
                using (var query = conn.CreateCommand())
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

        public List<int> GetDetectorsAtIntersection(int intersection)
        {
            var detectors = new List<int>();
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT detector FROM intersections WHERE intersection_id = @intersection;";
                    query.Parameters.AddWithValue("@intersection", intersection);
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detectors.Add(reader.GetByte(0));
                        }
                    }
                }
                conn.Close();
            }
            return detectors;
        }

        public List<DateTime> GetImportedDates()
        {
            var importedDates = new List<DateTime>();

            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT DISTINCT CONVERT(DATETIME, CONVERT(NVARCHAR(11), dateTime, 101)) AS [DD-MM-YYYY] FROM volumes";
                     
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

        public bool VolumesExist(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExist(DateTime startDate, DateTime endDate, int intersection)
        {
            throw new NotImplementedException();
        }

        public bool VolumesExistForMonth(int month)
        {
            throw new NotImplementedException();
        }

        public DateTime GetMostRecentImportedDate()
        {
            throw new NotImplementedException();
        }

        public Configuration GetConfiguration(string name)
        {
            Configuration result = null;
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT configs.intersection_id, approaches.approach_id, approaches.name, approach_detector_mapping.detector " +
                                        "FROM configs " +
                                        "INNER JOIN config_approach_mapping " +
                                        "ON configs.config_id = config_approach_mapping.config_id " +
                                        "INNER JOIN approaches " +
                                        "ON approaches.approach_id = config_approach_mapping.approach_id " +
                                        "INNER JOIN approach_detector_mapping " +
                                        "ON approaches.approach_id = approach_detector_mapping.approach_id " +
                                        "WHERE configs.name = @name";
                    query.Parameters.AddWithValue("@name", name);
                    using (var reader = query.ExecuteReader())
                    {
                      
                        if (reader.Read())
                        {
                            var intersection = reader.GetInt32(0);
                            var approaches = new List<Approach>();

                            var approachId = 0; //Seed id is 1, 0 should never be found in db
                            Approach currentApproach = null;
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

                            result = new Configuration(name, intersection, approaches, this);
                        }
                        
                    }
                }
                conn.Close();
            }
            return result;
        }

        public IEnumerable<SummaryRow> GetSummaryConfig(string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSummaryNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetConfigurationNames()
        {
            var names = new List<String>();
            using (var conn = new SqlCeConnection(_connectionString))
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

        public void AddConfiguration(Configuration config)
        {
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                conn.BeginTransaction();

                Int32 configId;

                //Insert into configs table
                using (var query = conn.CreateCommand())
                {
                    query.CommandText =
                        "INSERT INTO configs (name, date_last_used, intersection_id) VALUES (@name, GETDATE(), @intersection_id);";
                    query.Parameters.AddWithValue("@name", config.Name);
                    query.Parameters.AddWithValue("@intersection_id", config.Intersection);
                    query.ExecuteNonQuery();

                    query.CommandText = " SELECT CAST(@@Identity AS INT) as ID;";
                    configId = (Int32)query.ExecuteScalar();
                }

                foreach (var approach in config.Approaches)
                {
                    Int32 approachId;
                    //insert into approaches table
                    using (var query = conn.CreateCommand())
                    {
                        query.CommandText = "INSERT INTO approaches (name) VALUES (@approach);";
                        query.Parameters.AddWithValue("@approach", approach.Name);
                        query.ExecuteNonQuery();

                        query.CommandText = "SELECT CAST(@@Identity AS INT) as ID;";
                        approachId = (Int32)query.ExecuteScalar();
                    }

                    //insert into approach_detector_mapping 
                    using (var query = conn.CreateCommand())
                    {
                        query.CommandText = "INSERT INTO approach_detector_mapping (approach_id, detector) VALUES (@approach_id, @detector)";                       
                        
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
                        query.CommandText = "INSERT INTO config_approach_mapping (config_id, approach_id) VALUES (@config_id, @approach_id);";
                        query.Parameters.AddWithValue("@config_id", configId);
                        query.Parameters.AddWithValue("@approach_id", approachId);
                        query.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }
        }

        public void SaveMonthlySummaryConfig(string configName, IEnumerable<ReportConfiguration.SummaryRow> rows)
        {
            throw new NotImplementedException();
        }

        public void RemoveConfiguration(string name)
        {
            using (var conn = new SqlCeConnection(_connectionString))
            {
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "DELETE FROM configs WHERE name = @name;";
                    query.ExecuteNonQuery();
                }
            }
        }

        public void RemoveSummary(string name)
        {
            throw new NotImplementedException();
        }

        public bool SummaryExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool ConfigurationExists(string name)
        {
            long count;

            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();

                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT COUNT(*) FROM configs WHERE name = @configName;";
                    query.Parameters.AddWithValue("@configName", name);
                    count = (Int32)query.ExecuteScalar();
                }

                conn.Close();
            }

            return count.Equals(1); 
        }

        public Dictionary<int, List<int>> GetSuspectedFaults(DateTime startDate, DateTime endDate, int threshold)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Imports a single file
        /// </summary>
        /// <param name="filename">filename to import</param>
        /// <param name="updateProgress"></param>
        /// <param name="getDuplicatePolicy"></param>
        /// <param name="b">Worker thread</param>
        /// <param name="w"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">You may have passed in a bad filename</exception>
        public void ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            //Open the db connection
            FileStream fs;
            DuplicatePolicy duplicatePolicy;
            using (var dbConnection = new SqlCeConnection(_connectionString))
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

                using (var cmd = dbConnection.CreateCommand())
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
                                        catch (SqlCeException e)
                                        {
                                            if (e.ErrorCode.Equals(5) && !continuing)
                                            {
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
        }

        public bool VolumesTableEmpty()
        {
            var count = 0;
            using(var conn = new SqlCeConnection(_connectionString)){
                conn.Open();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT COUNT(*) FROM volumes;";
                    count = (Int32)query.ExecuteScalar();
                }
            }

            return count.Equals(0);
        }

        public void ClearData()
        {
            using (var conn = new SqlCeConnection(_connectionString))
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
            using (var conn = new SqlCeConnection(_connectionString))
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

       

        

        
    }
}
