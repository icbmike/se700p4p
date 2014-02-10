using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Modes
{
    public class SummaryMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private UserControl _viewContainer;
        private SummaryConfigScreen _configView;
        private SummaryTable _tableView;
        private SummaryViews currentView;

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

            //Set the startup view
            _viewContainer.Content = _configView;
            currentView = SummaryViews.Configuration;

        }

        private void ConfigViewOnConfigurationSaved(object sender, ConfigurationSavedEventArgs args)
        {
            args.Mode = this;
            OnConfigurationSaved(args);
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {

        }

        public override void ShowConfigurable(Configurable configurable)
        {
            if (currentView == SummaryViews.Table)
            {
                _tableView.Configuration = _dataSource.GetSummaryConfig(configurable.Name);
            }
        }

        public override void ShowConfigurationView()
        {
        }

        public override void EditConfigurable(Configurable configurable)
        {
        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {

        }

        public override UserControl GetView()
        {
            return _viewContainer;
        }

        public override List<Configurable> PopulateReportBrowser()
        {
            return _dataSource.GetSummaryNames().Select(name => new SummaryConfigurable(name, this, _dataSource)).Cast<Configurable>().ToList();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}