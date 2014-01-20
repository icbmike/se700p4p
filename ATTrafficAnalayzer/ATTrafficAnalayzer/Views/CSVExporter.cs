using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views
{
    public class CSVExporter
    {
        private readonly string _outputFilename;
        private readonly DateSettings _settings;
        private readonly IDataSource _dataSource;
//        private readonly DataTableHelper _dtHelper;
        private readonly Configuration _config;
        private readonly IEnumerable<SummaryRow> _summaryConfig;
        private readonly string _configName;
        private int _amPeakIndex = 8;
        private int _pmPeakIndex = 4;

        /// <summary>
        /// Constructor for a CSVExporter
        /// </summary>
        /// <param name="outputFilename">Filename of output csv file</param>
        /// <param name="settings">DateSettings for daterange and interval</param>
        /// <param name="configName">Name of the config to be exported</param>
        /// <param name="AmPeakHour">...</param>
        /// <param name="PmPeakHour">...</param>
        public CSVExporter(String outputFilename, DateSettings settings, string configName, int AmPeakHour, int PmPeakHour, IDataSource dataSource)
        {
            _outputFilename = outputFilename;
            _settings = settings;
            _dataSource = dataSource;
//            _dtHelper = DataTableHelper.GetDataTableHelper();
            _configName = configName;
            _amPeakIndex = AmPeakHour;
            _pmPeakIndex = PmPeakHour;

            //Retrieve the config for the supplied name
            _config = _dataSource.GetConfiguration(configName);
            _summaryConfig = _dataSource.GetSummaryConfig(_configName);
        }

        /// <summary>
        /// Exports a report
        /// </summary>
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

            //Same loop as in ReportTable
            for (var day = 0; day < timeSpan.TotalDays; day++)
            {
                foreach (var approach in _config.Approaches)
                {
                    var dataTable = approach.GetDataTable(_settings, _config.Intersection, day);

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

        /// <summary>
        /// Exports a summary
        /// </summary>
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

            var config = _dataSource.GetSummaryConfig(_configName);
            string[] configColumnNames = { "Route Name", "Inbound Intersection", "Inbound Detectors", "Inbound Dividing Factor", "Outbound Intersection", "Outbound Detectors", "Outbound Dividing Factor" };
            var configColumnNamesString = string.Join(",", configColumnNames);
            lines.Add(configColumnNamesString);

            foreach (var row in _summaryConfig)
            {
                string[] rowString = { row.RouteName, row.SelectedIntersectionIn.ToString(), string.Join(" ", row.DetectorsIn), row.DividingFactorIn.ToString(), row.SelectedIntersectionOut.ToString(), string.Join(" ", row.DetectorsOut), row.DividingFactorOut.ToString() };
                lines.Add(string.Join(",", rowString));
            }

            lines.Add("");

            var messageBoxText = "Would you like to export weekends";
            const string caption = "Export settings";
            const MessageBoxButton button = MessageBoxButton.YesNo;
            const MessageBoxImage icon = MessageBoxImage.Question;

            var result = false;
            if (MessageBox.Show(messageBoxText, caption, button, icon).Equals(MessageBoxResult.Yes))
                result = true;


            Dictionary<string, DataTable> summaries = new Dictionary<string, DataTable>();
//            summaries.Add("AM Peak Hour Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.ReportConfiguration.DataTableHelper.AmPeakCalculator(_amPeakIndex), _settings.StartDate, _settings.EndDate, _summaryConfig, result));
//            summaries.Add("PM Peak Hour Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.ReportConfiguration.DataTableHelper.PmPeakCalculator(_pmPeakIndex), _settings.StartDate, _settings.EndDate, _summaryConfig, result));
//            summaries.Add("Total Traffic Volumes", _dtHelper.GetSummaryDataTable(new ATTrafficAnalayzer.Models.ReportConfiguration.DataTableHelper.SumCalculator(), _settings.StartDate, _settings.EndDate, _summaryConfig, result));

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

        /// <summary>
        /// private method to write the prepared lines to the filename specified.
        /// </summary>
        /// <param name="lines"></param>
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
