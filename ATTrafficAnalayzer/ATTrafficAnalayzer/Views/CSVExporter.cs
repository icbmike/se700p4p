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
            var lines = new List<string>();

            lines.Add(_configName);
            lines.Add("");

            var start = _settings.StartDate.ToLongDateString();
            start = start.Replace(',', ' ');
            var end = _settings.EndDate.ToLongDateString();
            end = end.Replace(',', ' ');

            lines.Add(string.Format("Start date: {0}", start));
            lines.Add(string.Format("End date: {0}", end));
            lines.Add(string.Format("Interval: {0} min", _settings.Interval));
            lines.Add("");

            var timeSpan = _settings.EndDate - _settings.StartDate;

            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                foreach (var approach in _reportConfig.Approaches)
                {
                    var dataTable = approach.GetDataTable(_settings, _reportConfig.Intersection, 24, 0, day);

                    string[] columnNames = dataTable.Columns.Cast<DataColumn>().
                                                  Select(column => column.ColumnName).
                                                  ToArray();
                    var header = string.Join(",", columnNames);
                    lines.Add(header);

                    var valueLines = dataTable.AsEnumerable()
         .Select(row => string.Join(",", row.ItemArray));
                    lines.AddRange(valueLines);

                    lines.Add("");
                }
            }

            WriteToFile(lines);
        }

        public void ExportSummary()
        {
            var lines = new List<string>();

            lines.Add(_configName);
            lines.Add("");

            var start = _settings.StartDate.ToLongDateString();
            start = start.Replace(',', ' ');
            var end = _settings.EndDate.ToLongDateString();
            end = end.Replace(',', ' ');

            lines.Add(string.Format("Start date: {0}", start));
            lines.Add(string.Format("End date: {0}", end));
            lines.Add(string.Format("Morning peak hour: {0} AM", (_amPeakIndex == 0) ? 12 : _amPeakIndex));
            lines.Add(string.Format("Evening peak hour: {0} PM", (_pmPeakIndex == 0) ? 12 : _pmPeakIndex));
            lines.Add("");

            var config = _dbHelper.GetSummaryConfig(_configName);
            string[] configColumnNames = { "Route Name", "Inbound Intersection", "Inbound Detectors", "Inbound Dividing Factor", "Outbound Intersection", "Outbound Detectors", "Outbound Dividing Factor" };
            var configColumnNamesString = string.Join(",", configColumnNames);
            lines.Add(configColumnNamesString);

            foreach (var row in _summaryConfig)
            {
                string[] rowString = { row.RouteName, row.SelectedIntersectionIn.ToString(), string.Join(" ", row.DetectorsIn), row.DividingFactorIn.ToString(), row.SelectedIntersectionOut.ToString(), string.Join(" ", row.DetectorsOut), row.DividingFactorOut.ToString() };
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

            WriteToFile(lines);
        }

        private void WriteToFile(List<string> lines)
        {
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

            System.Windows.Forms.MessageBox.Show("Successfully exported");
        }
    }
}
