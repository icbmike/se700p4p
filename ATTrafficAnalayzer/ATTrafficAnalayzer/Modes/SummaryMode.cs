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
        private readonly UserControl _viewContainer;
        private SummaryConfigScreen _configView;
        private readonly SummaryTable _tableView;
        private SummaryViews _currentView;

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

        public override List<BaseConfigurable> PopulateReportBrowser()
        {
            return _dataSource.GetSummaryNames().Select(name => new SummaryConfigurable(name, this, _dataSource)).Cast<BaseConfigurable>().ToList();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}