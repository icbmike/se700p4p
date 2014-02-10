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
            _tableView = new RedLightRunningViewScreen(DateSettings, _dataSource);

        }

        public override UserControl GetView()
        {
            return _viewContainer;
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
        }

        public override void EditConfigurable(Configurable configurable)
        {
            throw new NotImplementedException();
        }

        public override void ShowConfigurable(Configurable configurable)
        {
        }

        public override void ShowConfigurationView()
        {
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