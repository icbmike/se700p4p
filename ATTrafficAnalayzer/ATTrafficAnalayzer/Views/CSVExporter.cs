using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views
{
    public class CSVExporter
    {
        private readonly string _outputFilename;
        private readonly SettingsTray _settings;
        private readonly DbHelper _dbHelper;
        private readonly DataTableHelper _dtHelper;
        private readonly Report _reportConfig;
        private readonly IEnumerable<SummaryRow> _summaryConfig;
        private readonly string _configName;
        private int _amPeakIndex = 8;
        private int _pmPeakIndex = 4;

        public CSVExporter(String outputFilename, SettingsTray settings, string configName, int AmPeakHour, int PmPeakHour)
        {
            _outputFilename = outputFilename;
            _settings = settings;
            _dbHelper = DbHelper.GetDbHelper();
            _dtHelper = DataTableHelper.GetDataTableHelper();
            _configName = configName;
            _amPeakIndex = AmPeakHour;
            _pmPeakIndex = PmPeakHour;

            //Retrieve the config for the supplied name
            _reportConfig = _dbHelper.GetConfiguration(configName);
            _summaryConfig = _dbHelper.GetSummaryConfig(_configName);
        }

        public void ExportReport()
        {
            using (var file = new StreamWriter(_outputFilename))
            {
                foreach (var approach in _reportConfig.Approaches)
                {
                    //The approach name and its detectors
                    file.Write(approach.Name + " - Detectors: ");
                    foreach (var detector in approach.Detectors)
                    {
                        file.Write(detector + " ");
                    }
                    file.Write("\n");

                    //Retrieve the volume information from the database
                    // List dates
                    var dates = new List<DateTime>();
                    for (var date = _settings.StartDate; date < _settings.EndDate; date = date.AddMinutes(_settings.Interval))
                    {
                        dates.Add(date);
                    }
                    var approachVolumes = new List<int>();
                    foreach (var detector in approach.Detectors)
                    {
                        if (approachVolumes.Count == 0)
                        {
                            approachVolumes.AddRange(_dbHelper.GetVolumes(_reportConfig.Intersection, detector, _settings.StartDate,
                                                                          _settings.EndDate));
                        }
                        else
                        {
                            var detectorVolumes = _dbHelper.GetVolumes(_reportConfig.Intersection, detector, _settings.StartDate,
                                                                          _settings.EndDate);
                            approachVolumes = approachVolumes.Zip(detectorVolumes, (i, i1) => i + i1).ToList();
                        }
                    }

                    //The row headings
                    for (var i = 0; i <= 12; i++)
                    {
                        if (i == 0)
                            file.Write(",");
                        else
                            file.Write((i - 1) + ",");
                    }
                    file.Write("\n");
                    //Each row
                    // Get volume store data //12 hours
                    for (var i = 0; i < 12; i++)
                    {

                        for (var j = 0; j < 13; j++)
                        {
                            if (j == 0)
                                file.Write(": " + _settings.Interval * i + ",");
                            else
                                file.Write(approachVolumes[i * 12 + j] + ",");
                        }
                        file.Write("\n");
                    }
                }
            }
        }

        public void ExportSummary()
        {
            var lines = new List<string>();

            lines.Add(_configName);
            lines.Add("");

            lines.Add(string.Format("Start date: {0}", _settings.StartDate.ToLongDateString()));
            lines.Add(string.Format("End date: {0}", _settings.EndDate.ToLongDateString()));
            lines.Add(string.Format("Morning peak hour: {0} AM", (_amPeakIndex == 0) ? 12 : _amPeakIndex));
            lines.Add(string.Format("Evening peak hour: {0} PM", (_pmPeakIndex == 0) ? 12 : _pmPeakIndex));
            lines.Add("");

            var config = _dbHelper.GetSummaryConfig(_configName);
            string[] configColumnNames = { "Route Name", "Inbound Intersection", "Inbound Detectors", "Inbound Dividing Factor", "Outbound Intersection", "Outbound Detectors", "Outbound Dividing Factor" };
            var configColumnNamesString = string.Join(",", configColumnNames);
            lines.Add(configColumnNamesString);

            foreach (var row in _summaryConfig)
            {
                string[] rowString = { row.RouteName, row.SelectedIntersectionIn.ToString(), string.Join(" ", row.DetectorsIn), row.DividingFactorIn.ToString(), row.SelectedIntersectionOut.ToString(), string.Join(" ", row.DetectorsOut), row.DividingFactorOut.ToString()};
                lines.Add(string.Join(",", rowString));
            }

            lines.Add("");

            Dictionary<string, DataTable> summaries = new Dictionary<string, DataTable>();
            summaries.Add("AM Peak Hour Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.AmPeakCalculator(_amPeakIndex), _settings.StartDate, _settings.EndDate, _summaryConfig));
            summaries.Add("PM Peak Hour Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.PmPeakCalculator(_pmPeakIndex), _settings.StartDate, _settings.EndDate, _summaryConfig));
            summaries.Add("Total Traffic Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.Configuration.DataTableHelper.SumCalculator(), _settings.StartDate, _settings.EndDate, _summaryConfig));

            foreach (var summary in summaries)
            {
                lines.Add(summary.Key);

                string[] columnNames = summary.Value.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName).
                                                  ToArray();
                var header = string.Join(",", columnNames);
                lines.Add(header);

                var valueLines = summary.Value.AsEnumerable()
                    .Select(row => string.Join(",", row.ItemArray));
                valueLines = valueLines.Select(row => row.Remove(row.IndexOf(','), 1));
                lines.AddRange(valueLines);

                lines.Add("");
            }

            try
            {                                         
                File.WriteAllLines(_outputFilename, lines);
            }
            catch (IOException)
            {
                var messageBoxText = "Cannot write to file. Please ensure the file is not open. Try again?";
                const string caption = "Export failed";
                const MessageBoxButton button = MessageBoxButton.OKCancel;
                const MessageBoxImage icon = MessageBoxImage.Error;

                var result = MessageBox.Show(messageBoxText, caption, button, icon);

                if (result.Equals(MessageBoxResult.OK))
                    ExportSummary();
            }
        }
    }
}
