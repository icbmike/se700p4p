using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
        private readonly SummaryConfigScreen _configView;
        private readonly SummaryTable _tableView;
        private SummaryViews _currentView;
        private readonly List<SummaryStatistic> statistics;
        private int _amPeakTime;
        private int _pmPeakTime;

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

            //Some defaults
            AmPeakTime = 7;
            PmPeakTime = 4;

            //Create our views
            _configView =  new SummaryConfigScreen(_dataSource);
            _configView.ConfigurationSaved += ConfigViewOnConfigurationSaved;
            _tableView = new SummaryTable(dateSettings, dataSource);

            statistics = new List<SummaryStatistic>
            {
                new SummaryStatistic("Daily Totals", CalculateDailyTotals),
                new SummaryStatistic("AM Peak Volumes", CalculateAmPeakVolumes),
                new SummaryStatistic("PM Peak Volumes", CalculatePmPeakVolumes)
            };

            _tableView.Statistics.AddRange(statistics);

            //Set the startup view
            _viewContainer.Content = _configView;
            _currentView = SummaryViews.Configuration;

        }

        private int CalculatePmPeakVolumes(DateTime dateTime, SummaryRow row)
        {
            return CalculatePeakVolumes(dateTime, row, false);
        }

        private int CalculateAmPeakVolumes(DateTime dateTime, SummaryRow row)
        {
            return CalculatePeakVolumes(dateTime, row, true);
        }

        private int CalculatePeakVolumes(DateTime dateTime, SummaryRow row, bool isAM)
        {
            
            var date = dateTime.AddHours(isAM ? _amPeakTime : _pmPeakTime);
            if (_dataSource.VolumesExist(dateTime))
            {
                return _dataSource.GetVolumeForTimePeriod(row.SelectedIntersectionIn, row.DetectorsIn, date, date.AddHours(1)) +
                    _dataSource.GetVolumeForTimePeriod(row.SelectedIntersectionOut, row.DetectorsOut, date, date.AddHours(1));
            }
            return 0;
        }

        public int AmPeakTime
        {
            get { return _amPeakTime; }
            set
            {
                _amPeakTime = value;
                Console.WriteLine(value);
                if (_tableView != null) _tableView.AMPeakTime = value;
            }
        }

        public int PmPeakTime
        {
            get { return _pmPeakTime; }
            set
            {
                _pmPeakTime = value;
                if (_tableView != null) _tableView.PMPeakTime = value;
            }
        }

        private void ConfigViewOnConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            args.Mode = this;
            OnConfigurationSaved(args);
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
            var amlabel = new Label()
            {
                Content = "AM Peak Time: ",
                Style = Application.Current.FindResource("ToolbarLableStyle") as Style
            };

            var pmlabel = new Label()
            {
                Content = "PM Peak Time: ",
                Style = Application.Current.FindResource("ToolbarLableStyle") as Style
            };

            var times = Enumerable.Range(1, 11).ToList();
            times.Insert(0, 12); //Stupid conventional timing

            var amComboBox = new ComboBox()
            {
                DataContext = this,
                Style = Application.Current.FindResource("ToolbarComboBoxStyle") as Style
            };
            times.ForEach(time => amComboBox.Items.Add(time + " AM"));
            amComboBox.SelectedIndex = 8;

            amComboBox.SetBinding(Selector.SelectedValueProperty,
                new Binding("AmPeakTime") { Converter = new SummaryPeakTimeConverter(), ConverterParameter = "AM" });

            var pmComboBox = new ComboBox()
            {
                DataContext = this,
                Style = Application.Current.FindResource("ToolbarComboBoxStyle") as Style
            };

            times.ForEach(time => pmComboBox.Items.Add(time + " PM"));
            pmComboBox.SelectedIndex = 5;
            pmComboBox.SetBinding(Selector.SelectedValueProperty,
                new Binding("PmPeakTime") { Converter = new SummaryPeakTimeConverter(), ConverterParameter = "PM"});

            //Add all the things to the toolbar
            toolbar.Items.Add(amlabel);
            toolbar.Items.Add(amComboBox);
            toolbar.Items.Add(pmlabel);
            toolbar.Items.Add(pmComboBox);
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
            _configView.Configuration = _dataSource.GetSummaryConfig(configurable.Name);
            ShowConfigurationView();
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