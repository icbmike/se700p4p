using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Models.Volume;

namespace ATTrafficAnalayzer.Models.ReportConfiguration
{
    public class Approach
    {
        private readonly IDataSource _dataSource;

        /// <summary>
        ///     Peak volumes
        /// </summary>
        public VolumeMetric AmPeak = new VolumeMetric();
        public VolumeMetric PmPeak = new VolumeMetric();
        private int _approachTotal;
        private DataTable _dataTable;
        private int _amPeak = -1;
        private int _pmPeak = -1;   
        private DateTime _amPeakTime;
        private DateTime _pmPeakTime;

        public string ApproachName { get; set; }
        public List<int> Detectors { get; set; }
        public int Id { get; set; }

        public int TotalVolume
        {
            get { return _approachTotal; }
        }

        public int AMPeakVolume
        {
            get { return _amPeak; }
        }

        public int PMPeakVolume
        {
            get { return _pmPeak; }
        }

        public DateTime AMPeakTime
        {
            get { return _amPeakTime; }
        }

        public DateTime PMPeakTime
        {
            get { return _pmPeakTime; }
        }

        /// <summary>
        ///     Creates the approach instance
        /// </summary>
        /// <param ApproachName="approachName">ApproachName of the approach</param>
        /// <param ApproachName="detectors">List of detectors </param>
        /// /// <param ApproachName="dataSource">An IDataSource</param>
        public Approach(string approachName, List<int> detectors, IDataSource dataSource)
        {
            ApproachName = approachName;
            Detectors = detectors;
            _dataSource = dataSource;
        }

        /// <summary>
        ///     Returns the ApproachName of the approach
        /// </summary>
        /// <returns>Approach ApproachName</returns>
        public override string ToString()
        {
            return ApproachName;
        }

        /// <summary>
        ///     Get a list of volumes
        /// </summary>
        /// <param ApproachName="intersection">Intersection ID</param>
        /// <param ApproachName="startDate">Start of the period</param>
        /// <param ApproachName="endDate">End of the period</param>
        /// <returns>List of volumes</returns>
        public List<int> GetVolumesList(int intersection, DateTime startDate, DateTime endDate)
        {
            var volumes = new List<int>();
            foreach (var detector in Detectors)
            {
                if (volumes.Count == 0)
                {
                    volumes.AddRange(_dataSource.GetVolumes(intersection, detector, startDate, endDate));
                }
                else
                {
                    var detectorVolumes = _dataSource.GetVolumes(intersection, detector, startDate, endDate);
                    volumes = volumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                }
            }
            return volumes;
        }

        /// <summary>
        ///     Gets all traffic volume data
        /// </summary>
        /// <param ApproachName="settings">Group of settings from </param>
        /// <param ApproachName="intersection">Intersection ID</param>
        /// <param ApproachName="limit">Number of data items to get</param>
        /// <param ApproachName="offset">Index of the starting data element</param>
        /// <param ApproachName="day">Day the volumes are required for</param>
        /// <returns></returns>
        public DataTable GetDataTable(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (_dataTable == null)
            {
                _dataTable = new DataTable();

                // Column headings
                for (var i = offset; i <= offset + limit; i++)
                    _dataTable.Columns.Add(i == 0 ? "Time" : String.Format("{0} hrs", i - 1), typeof (string));

                // List dates
                var dates = new List<DateTime>();
                for (var date = settings.StartDate; date < settings.EndDate; date = date.AddMinutes(settings.Interval))
                    dates.Add(date);

                // Get volume store data 24 hours
                var approachVolumes = GetVolumesList(intersection, settings.StartDate.AddDays(day),
                                                     settings.StartDate.AddDays(day + 1));
                //Need to do check here if the user hasnt imported data for the
                //Check that we actually have volumes that we need

                if (approachVolumes.Count == 0)
                {
                    return null;
                }
                for (var rowIndex = 0; rowIndex < 60; rowIndex += settings.Interval)
                {
                    var row = _dataTable.NewRow();
                    for (var columnIndex = 0; columnIndex <= limit; columnIndex++)
                    {
                        if (columnIndex == 0)
                            row[columnIndex] = rowIndex + " mins";
                        else
                        {
                            var cellValue = 0;
                            for (var i = 0; i < settings.Interval/5; i++)
                            {
                                cellValue += approachVolumes[(offset + columnIndex - 1)*12 + rowIndex/5 + i];
                            }
                            row[columnIndex] = cellValue;
                        }
                    }
                    _dataTable.Rows.Add(row);
                }

                var totalsRow = _dataTable.NewRow();
                totalsRow[0] = "Total";

                AmPeak.ClearApproaches();
                PmPeak.ClearApproaches();
                _approachTotal = 0;

                for (var j = 0; j < limit; j++)
                {
                    var total = CalculateColumnTotal(approachVolumes, j, 12);
                    totalsRow[j + 1] = total;
                    _approachTotal = TotalVolume + total;
                    if (j < limit/2)
                        AmPeak.CheckIfMax(total, j + " hrs");
                    else
                        PmPeak.CheckIfMax(total, j + " hrs");
                }
                _dataTable.Rows.Add(totalsRow);
            }
            return _dataTable;
        }

        /// <summary>
        /// Calculates the total for each column in the datagrid
        /// </summary>
        /// <param ApproachName="data">A List of data values</param>
        /// <param ApproachName="column">Column number</param>
        /// <param ApproachName="numberOfRows">Number of items in the list</param>
        /// <returns>Total traffic volume</returns>
        private static int CalculateColumnTotal(IList<int> data, int column, int numberOfRows)
        {
            return GetColumnData(data, column, numberOfRows).Sum();
        }

        /// <summary>
        ///     Port the data for a column as an IEnumerable
        /// </summary>
        /// <param ApproachName="data">Data values</param>
        /// <param ApproachName="column">Column number</param>
        /// <param ApproachName="numberOfRows">Number of items in the list</param>
        /// <returns>List of data</returns>
        private static IEnumerable<int> GetColumnData(IList<int> data, int column, int numberOfRows)
        {
            var columnList = new List<int>();
            for (var i = 0; i < numberOfRows; i++)
            {
                columnList.Add(data[i + column * numberOfRows]);
            }
            return columnList;
        }

        /// <summary>
        ///     Total volume for the approach
        /// </summary>
        /// <returns>Total traffic volume for the approach</returns>
        public int GetTotal(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (_dataTable == null) _dataTable = GetDataTable(settings, intersection, day, limit, offset);
            //The last row of the data table has totals in it
            var sum = 0;
            for (int i = 1; i < _dataTable.Rows[_dataTable.Rows.Count - 1].ItemArray.Length; i++)
            {
                var item = _dataTable.Rows[_dataTable.Rows.Count - 1].ItemArray[i];
                sum += int.Parse(item as string);
            }
            _approachTotal = sum;
            return sum;
        }

        public int GetPeak(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (AMPeakVolume == -1) GetAmPeak(settings, intersection, day, limit, offset);
            if (PMPeakVolume == -1) GetPmPeak(settings, intersection, day, limit, offset);
            return Math.Max(AMPeakVolume, PMPeakVolume);
        }

        private void Invalidate()
        {
            _dataTable = null;
            _amPeak = -1;
            _pmPeak = -1;
        }

        public int GetAmPeak(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (AMPeakVolume == -1)
            {
                if (_dataTable == null) _dataTable = GetDataTable(settings, intersection, day, limit, offset);
                var max = -1;
                var rowPos = 0;
                var columnPos = 0; 

                //Skip the last row so that we don't get totals
                for (var rowIndex = 0; rowIndex < _dataTable.Rows.Count - 1; rowIndex++)
                {
                    var row = _dataTable.Rows[rowIndex];
                    //Only go to the 12th column so that we're in the morning
                    for (var i = 1; i < 13; i++)
                    {
                        var candidate = int.Parse(row.ItemArray[i] as string);
                        if (candidate > max)
                        {
                            max = candidate;
                            //we can work out the datetime from the position in the table
                            rowPos = rowIndex;
                            columnPos = i;
                        }
                    }
                }
                //we can work out the datetime from the position in the table
                //The hour maps to the column pos - 1
                _amPeakTime = settings.StartDate.AddDays(day).AddHours(columnPos -1).AddMinutes(rowPos * settings.Interval);
                _amPeak = max;
                
            }
            return AMPeakVolume;
        }

        public int GetPmPeak(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (PMPeakVolume == -1)
            {
                if (_dataTable == null) _dataTable = GetDataTable(settings, intersection, day, limit, offset);
                var max = 0;
                var rowPos = 0;
                var columnPos = 0;
                //Row count -1 so we dont get the totals
                for (var rowIndex = 0; rowIndex < _dataTable.Rows.Count - 1; rowIndex++)
                {
                    var row = _dataTable.Rows[rowIndex];
                    //Start at the 13th column so we only get the afternoon
                    for (var i = 13; i < row.ItemArray.Count(); i++)
                    {
                        var candidate = int.Parse(row.ItemArray[i] as string);
                        if (candidate > max)
                        {
                            max = candidate;
                            //we can work out the datetime from the position in the table
                            rowPos = rowIndex;
                            columnPos = i;
                        }
                    }
                }
                _pmPeakTime = settings.StartDate.AddDays(day).AddHours(columnPos - 1).AddMinutes(rowPos * settings.Interval);
                _pmPeak = max;

            }
            return PMPeakVolume;
        }

        public DateTime GetPeakTime(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (AMPeakVolume == -1) GetAmPeak(settings, intersection, day, limit, offset);
            if (PMPeakVolume == -1) GetPmPeak(settings, intersection, day, limit, offset);
            if (AMPeakVolume > PMPeakVolume) return AMPeakTime;
            else return PMPeakTime;
        }

        public DateTime GetAmPeakTime(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (AMPeakVolume == -1)
            {
                GetAmPeak(settings, intersection, day, limit, offset);
            }
            return AMPeakTime;
        }

        public DateTime GetPmPeakTime(DateSettings settings, int intersection, int day, int limit = 24, int offset = 0)
        {
            if (PMPeakVolume == -1)
            {
                GetPmPeak(settings, intersection, day, limit, offset);
            }
            return PMPeakTime;
        }

        public void LoadDataTable(DateSettings dateSettings, int intersection, int day)
        {
            GetDataTable(dateSettings, intersection, day);
            GetAmPeakTime(dateSettings, intersection, day);
            GetPmPeakTime(dateSettings, intersection, day);
            GetTotal(dateSettings, intersection, day);
        }
    }
}
