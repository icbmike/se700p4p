using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using Newtonsoft.Json.Linq;

namespace ATTrafficAnalayzer.Models
{
    public class SqlceDataSource : IDataSource
    {
        private string _connectionString;

        public SqlceDataSource()
        {
            var connectionSb = new SqlCeConnectionStringBuilder();
            connectionSb.DataSource = "TA.sdf";
            _connectionString = connectionSb.ConnectionString;
        }

        public int GetVolume(int intersection, int detector, DateTime dateTime)
        {
            throw new NotImplementedException();
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

        public List<int> GetDetectorsAtIntersection(int intersection)
        {
            List<int> detectors;
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                detectors = new List<int>();
                using (var query = conn.CreateCommand())
                {
                    query.CommandText = "SELECT DISTINCT detector FROM volumes WHERE intersection = @intersection;";
                    query.Parameters.AddWithValue("@intersection", intersection);
                    using (var reader = query.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detectors.Add(reader.GetInt16(0));
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
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();

                JObject configJson = null;

                using (var query = conn.CreateCommand())
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
                        using (var query = conn.CreateCommand())
                        {
                            query.CommandText = "SELECT approach FROM approaches WHERE id = @id;";
                            query.Parameters.AddWithValue("@id", approachID.ToString());

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
                    return new ReportConfiguration.Configuration(name, (int)configJson["intersection"], approaches);
                }
                conn.Close();
            }
            return null;
        }

        public List<Approach> GetApproaches(string configName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SummaryRow> GetSummaryConfig(string name)
        {
            throw new NotImplementedException();
        }

        public List<string> GetSummaryNames()
        {
            throw new NotImplementedException();
        }

        public List<string> GetReportNames()
        {
            throw new NotImplementedException();
        }

        public void AddConfiguration(Configuration config)
        {
            using (var conn = new SqlCeConnection(_connectionString))
            {
                conn.Open();
                conn.BeginTransaction();
                //Add configuration
                //Foreach approach
                    //Add approach
                    //Add to config mapping table
                    //Add to detector mapping table

                Int32 configID;

                //Insert into configs table
                using (var query = conn.CreateCommand())
                {
                    query.CommandText =
                        "INSERT INTO configs (name, date_last_used) VALUES (@name, GETDATE()); SELECT @@Identity as ID;";
                    query.Parameters.AddWithValue("@name", config.ConfigName);
                    configID = (Int32)query.ExecuteScalar();
                }


                foreach (var approach in config.Approaches)
                {
                    Int32 approachID;
                    //insert into approaches table
                    using (var query = conn.CreateCommand())
                    {
                        query.CommandText = "INSERT INTO approaches (name) VALUES (@approach);SELECT @@Identity as ID;";
                        query.Parameters.AddWithValue("@approach", approach.Name);
                        approachID = (Int32)query.ExecuteScalar();
                    }

                    //insert into approach_detector_mapping and config_approach_mapping
                    using (var query = conn.CreateCommand())
                    {
                        new StringBuilder("INSERT INTO");
                        
                    }
                }
                conn.Close();
            }
        }

        public void SaveMonthlySummaryConfig(string configName, IEnumerable<ReportConfiguration.SummaryRow> rows)
        {
            throw new NotImplementedException();
        }

        public void RemoveReport(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveSummary(string name)
        {
            throw new NotImplementedException();
        }

        public bool SummaryExists(string name)
        {
            throw new NotImplementedException();
        }

        public bool ReportExists(string name)
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


        public DuplicatePolicy ImportFile(string filename, Action<int> updateProgress, Func<DuplicatePolicy> getDuplicatePolicy)
        {
            throw new NotImplementedException();
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
            ClearIntersections();
            ClearVolumes();
            ClearConfigurations();
            ClearApproaches();
        }

        public void AddIntersection(int intersection, IEnumerable<int> detectors)
        {
            throw new NotImplementedException();
        }

        private void ClearApproaches()
        {
            throw new NotImplementedException();
        }

        private void ClearConfigurations()
        {
            throw new NotImplementedException();
        }

        private void ClearVolumes()
        {
            throw new NotImplementedException();
        }

        private void ClearIntersections()
        {
            throw new NotImplementedException();
        }
    }
}
