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
    public class SummaryMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private UserControl _viewContainer;
        private SummaryConfig _configView;
        private SummaryTable _tableView;

        public SummaryMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange, dateSettings)
        {
            ModeName = "Summaries";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_057_history.png", UriKind.Relative));
            
            _dataSource = dataSource;
            _viewContainer = new UserControl();

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

    public class SummaryConfigurable : Configurable
    {
        private readonly IDataSource _dataSource;

        public SummaryConfigurable(string name, BaseMode mode, IDataSource dataSource) : base(name, mode)
        {
            _dataSource = dataSource;
        }

        public override void Delete()
        {
            _dataSource.RemoveSummary(Name);   
        }
    }
}