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
    public class RedLightRunningMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private readonly UserControl _viewContainer;
        private ReportConfig _siteConfigScreen;
        private RedLightRunningConfigScreen _configScreen;
        private RedLightRunningViewScreen _tableView;
        private Views _currentView;

        enum Views
        {
            SiteConfigScreen,
            ViewScreen,
            ConfigScreen
        }

        public RedLightRunningMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange,dateSettings)
        {
            //Standard mode stuff
            _dataSource = dataSource;
            _viewContainer = new UserControl();

            ModeName = "Red Light Running";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\red_traffic_light.png", UriKind.Relative));


            //Specific mode stuff
            _siteConfigScreen = new ReportConfig(_dataSource);
            _configScreen = new RedLightRunningConfigScreen(_dataSource);
            _configScreen.ConfigurationSaved += ConfigViewOnConfigurationSaved;
            _tableView = new RedLightRunningViewScreen(DateSettings, _dataSource);

            //Set the startup screen
            _currentView = Views.ConfigScreen;
            _viewContainer.Content = _configScreen;

        }

        public override UserControl GetView()
        {
            return _viewContainer;
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {

        }

        protected override string GetExportContent(BaseConfigurable configurable)
        {
            var stringBuilder = new StringBuilder();

            var configuration = _dataSource.GetRedLightRunningConfiguration(configurable.Name);
            stringBuilder.AppendLine(configurable.Name);

            stringBuilder.AppendLine("Site ID,Total Volume,Total Red Light Running Volume,Approaches...");

            foreach (var reportConfiguration in configuration.Sites)
            {
                stringBuilder.Append(reportConfiguration.Intersection + ",");
                stringBuilder.Append(_dataSource.GetTotalVolumeForDay(DateSettings.StartDate,
                    reportConfiguration.Intersection) + ",");

                string totalRedLightRunning;
                try
                {
                    totalRedLightRunning = reportConfiguration.GetTotalVolume(DateSettings).ToString();
                }
                catch (NoDataForDateSpecifiedException)
                {
                    totalRedLightRunning = "No data for date";
                }

                stringBuilder.Append(totalRedLightRunning + ",");

                foreach (var approach in reportConfiguration.Approaches)
                {
                    string approachTotal;
                    try
                    {
                        approachTotal = approach.GetTotal(DateSettings, reportConfiguration.Intersection, 0).ToString();
                    }
                    catch (NoDataForDateSpecifiedException e)
                    {
                        approachTotal = "No data for date";
                    }
                    stringBuilder.Append(approach.ApproachName + ": " + approachTotal + ",");
                }
                stringBuilder.AppendLine("");
            }

            return stringBuilder.ToString();
        }

        private void ConfigViewOnConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            args.Mode = this; //Set the mode now that we know it
            OnConfigurationSaved(args); //Bubble the event up
        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            _tableView.DateRangeChanged();
        }

        public override void EditConfigurable(BaseConfigurable configurable)
        {
            _configScreen.Configuration = _dataSource.GetRedLightRunningConfiguration(configurable.Name);
            ShowConfigurationView();
        }

        public override void ShowConfigurable(BaseConfigurable configurable)
        {
            _tableView.Configuration = _dataSource.GetRedLightRunningConfiguration(configurable.Name);
            _currentView = Views.ViewScreen;
            _viewContainer.Content = _tableView;
        }

        public override void ShowConfigurationView()
        {
            _currentView = Views.ConfigScreen;
            _viewContainer.Content = _configScreen;
        }

        public override List<BaseConfigurable> PopulateReportBrowser()
        {
            return
                _dataSource.GetRedLightRunningConfigurationNames()
                    .Select(name => new RedLightRunningConfigurable(name, this, _dataSource))
                    .Cast<BaseConfigurable>()
                    .ToList();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }

}