using System;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for StatsTable.xaml
    /// </summary>
    public partial class StatsTable
    {
        private readonly IDataSource _dataSource;
        private DateSettings _dateSettings;
        private string _configName;
        private readonly string _title;
        private readonly Func<DateTime, SummaryRow, int> _calculate;

        public StatsTable(IDataSource dataSource, DateSettings dateSettings,
            string configName, string title, Func<DateTime, SummaryRow, int> calculate)
        {
            _dataSource = dataSource;
            _dateSettings = dateSettings;
            _configName = configName;
            _title = title;
            _calculate = calculate;
            InitializeComponent();

            Render();
        }

        private void Render()
        {
            var summaryConfig = _dataSource.GetSummaryConfig(_configName);
            StatsSummary.Html = "[b]" + _title + "[/b]";

            //Setup the grid
            var grid = new GridView();
            var dataTable = new DataTable();

            //The date column
            var dateColumn = dataTable.Columns.Add("Date");
            grid.Columns.Add(new GridViewColumn
            {
                Header = dateColumn.ColumnName,
                DisplayMemberBinding = new Binding(dateColumn.ColumnName),
            });

            //Each route column
            foreach (var summaryRow in summaryConfig)
            {
                var routeColumn = dataTable.Columns.Add(summaryRow.RouteName);
                grid.Columns.Add(new GridViewColumn
                {
                    Header = routeColumn.ColumnName,
                    DisplayMemberBinding = new Binding(routeColumn.ColumnName),
                });
            }

            //Reapply the datatable to the grid/listview
            StatsListView.View = grid;
            StatsListView.ItemsSource = dataTable.DefaultView;


            //Each day apply the calculation we were given
            for (var day = _dateSettings.StartDate; day < _dateSettings.EndDate; day = day.AddDays(1))
            {
                var dataRow = dataTable.NewRow();
                dataRow["Date"] = day.ToShortDateString();
                foreach (var summaryRow in summaryConfig)
                {
                    dataRow[summaryRow.RouteName] = _calculate(day, summaryRow);
                }
                dataTable.Rows.Add(dataRow);
            }
        }

        //Stuff changed
        public void DateSettingsChanged()
        {            
            Render();
        }

        public void SelectedReportChanged(string newSelection)
        {
            _configName = newSelection;
            Render();
        }

        public event EventHandler VolumeDateCountsDontMatch;
    }
}