using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Modes
{
    public class SummaryMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private readonly UserControl _viewContainer;
        private SummaryConfigScreen _configView;
        private readonly SummaryTable _tableView;
        private SummaryViews _currentView;
        private List<SummaryStatistic> statistics;

        enum SummaryViews
        {
            Table,
            Configuration
        }

        public SummaryMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange, dateSettings)
        {
            //Standard Mode stuff
            ModeName = "Summaries";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_057_history.png", UriKind.Relative));
            
            _dataSource = dataSource;
            _viewContainer = new UserControl();

            //Create our views
            _configView =  new SummaryConfigScreen(_dataSource);
            _configView.ConfigurationSaved += ConfigViewOnConfigurationSaved;
            _tableView = new SummaryTable(dateSettings, dataSource);

            statistics = new List<SummaryStatistic>
            {
                new SummaryStatistic("Daily Totals", CalculateDailyTotals),
                new SummaryStatistic("Sample", (time, row) => time.Day)
            };

            _tableView.Statistics.AddRange(statistics);

            //Set the startup view
            _viewContainer.Content = _configView;
            _currentView = SummaryViews.Configuration;

        }

        private void ConfigViewOnConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            args.Mode = this;
            OnConfigurationSaved(args);
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {

        }

        public override void ShowConfigurable(BaseConfigurable configurable)
        {
            _tableView.Configuration = _dataSource.GetSummaryConfig(configurable.Name);
            _viewContainer.Content = _tableView;
            _currentView = SummaryViews.Table;
        }

        public override void ShowConfigurationView()
        {
            _viewContainer.Content = _configView;
            _currentView = SummaryViews.Configuration;
        }

        public override void EditConfigurable(BaseConfigurable configurable)
        {
        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            _tableView.DateSettingsChanged();
        }

        public override UserControl GetView()
        {
            return _viewContainer;
        }

        protected override string GetExportContent(BaseConfigurable configurable)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(configurable.Name);
            var summaryConfiguration = _dataSource.GetSummaryConfig(configurable.Name);


            stringBuilder.AppendLine(
                "Route Name,Intersection In,Detectors In,Dividing Factor In,Intersection Out,Detectors Out,Dividing Factor Out");

            foreach (var summaryRow in summaryConfiguration.SummaryRows)
            {
                stringBuilder.Append(summaryRow.RouteName + ",");
                stringBuilder.Append(summaryRow.SelectedIntersectionIn + ",");
                stringBuilder.Append(string.Join(" ", summaryRow.DetectorsIn) + ",");
                stringBuilder.Append(summaryRow.DividingFactorIn + ",");
                stringBuilder.Append(summaryRow.SelectedIntersectionOut + ",");
                stringBuilder.Append(string.Join(" ", summaryRow.DetectorsOut) + ",");
                stringBuilder.AppendLine(summaryRow.DividingFactorOut + "\n");
            }

            foreach (var summaryStatistic in statistics)
            {
                stringBuilder.AppendLine(summaryStatistic.Name);
                stringBuilder.Append("Date,");
                stringBuilder.AppendLine(string.Join(",", summaryConfiguration.SummaryRows.Select(row => row.RouteName)));

                for (var day = DateSettings.StartDate; day < DateSettings.EndDate; day = day.AddDays(1))
                {
                    stringBuilder.Append(day.ToShortDateString() + ",");
                    DateTime day1 = day; //Closure stuff ?
                    SummaryStatistic statistic = summaryStatistic;
                    stringBuilder.AppendLine(string.Join(",", summaryConfiguration.SummaryRows.Select(row => statistic.Calculation(day1, row))));
                }
                stringBuilder.AppendLine("");
            }

            return stringBuilder.ToString();
        }

        public override List<BaseConfigurable> PopulateReportBrowser()
        {
            return _dataSource.GetSummaryNames().Select(name => new SummaryConfigurable(name, this, _dataSource)).Cast<BaseConfigurable>().ToList();
        }


        private int CalculateDailyTotals(DateTime dateTime, SummaryRow summaryRow)
        {
            Console.WriteLine(dateTime);
            var totalVolumeForDay = _dataSource.GetTotalVolumeForDay(dateTime.Date, summaryRow.SelectedIntersectionIn, summaryRow.DetectorsIn);
            totalVolumeForDay += _dataSource.GetTotalVolumeForDay(dateTime.Date, summaryRow.SelectedIntersectionOut,
                summaryRow.DetectorsOut);
            return totalVolumeForDay;
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}