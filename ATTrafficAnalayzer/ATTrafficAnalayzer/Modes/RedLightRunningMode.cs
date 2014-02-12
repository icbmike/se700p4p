using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
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

        private void ConfigViewOnConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            args.Mode = this; //Set the mode now that we know it
            OnConfigurationSaved(args); //Bubble the event up
        }

        public override void EditConfigurable(Configurable configurable)
        {
            throw new NotImplementedException();
        }

        public override void ShowConfigurable(Configurable configurable)
        {
            _tableView.Configuration = _dataSource.GetRedLightRunningConfiguration(configurable.Name);
            _currentView = Views.ViewScreen;
            _viewContainer.Content = _tableView;
        }

        public override void ShowConfigurationView()
        {
            _currentView = Views.ConfigScreen;
            _viewContainer.Content = _configScreen;
            _configScreen.RefreshReportConfigurations();
        }

        public override List<Configurable> PopulateReportBrowser()
        {
            return
                _dataSource.GetRedLightRunningConfigurationNames()
                    .Select(name => new RedLightRunningConfigurable(name, this, _dataSource))
                    .Cast<Configurable>()
                    .ToList();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }

}