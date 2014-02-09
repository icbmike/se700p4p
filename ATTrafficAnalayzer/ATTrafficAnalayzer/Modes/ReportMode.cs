using System;
using System.Collections.Generic;
using System.Linq;
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
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace ATTrafficAnalayzer.Modes
{
    public class ReportMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private ReportViews _currentView;
        private readonly UserControl _view;

        private ReportTable _tableView;
        private ReportGraph _graphView;
        private UserControl _configView;
        private Configuration _configuration;
        private int _interval;

        public int Interval
        {
            get { return _interval; }
            set { _interval = value;
                if (_tableView != null) _tableView.Interval = value;
                if (_graphView != null) _graphView.Interval = value;
            }
        }

        //Do some sweet event bubbling
        public event EventHandler DateVolumeCountsDontMatch
        {
            add
            {
                _tableView.VolumeDateCountsDontMatch += value;
                _graphView.VolumeDateCountsDontMatch += value;
            }
            remove
            {
                _tableView.VolumeDateCountsDontMatch -= value;
                _graphView.VolumeDateCountsDontMatch -= value;
            }
        }

        enum ReportViews
        {
            Table,
            Graph,
            Configuration
        }

        public ReportMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange, dateSettings)
        {
            //Base Mode stuff
            ModeName = "Report";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_029_notes_2.png", UriKind.Relative));


            _dataSource = dataSource;
            
            //Make the starting view the configuration screen
            _currentView = ReportViews.Configuration;
            _view = new UserControl(); // A container that we give to the main window but populate ourselves

            ShowConfigurationView();
            
            Interval = 5;

            //Create the views
            _graphView = CreateGraphView();
            _tableView = CreateTableView();

        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            switch (_currentView)
            {
                case ReportViews.Table:
                    _tableView.DateSettingsChanged();
                    break;
                case ReportViews.Graph:
                    _graphView.DateSettingsChanged();
                    break;
            }
        }

        public override List<Configurable> PopulateReportBrowser()
        {
            return _dataSource.GetConfigurationNames().Select(name => new ReportConfigurable(name, this, _dataSource)).Cast<Configurable>().ToList();
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
            var label = CreateIntervalLabel();

            var intervals = CreateComboBox();

            var tableButton = CreateTableButton();

            var graphButton = CreateGraphButton();

            //Add our controls to the toolbar
            toolbar.Items.Add(label);
            toolbar.Items.Add(intervals);
            toolbar.Items.Add(tableButton);
            toolbar.Items.Add(graphButton);

        }
        #region Toolbar items construction

        private Button CreateGraphButton()
        {
            var graphButton = new Button
            {
                Style = Application.Current.FindResource("AtToolbarButtonStyle") as Style,
                Content =
                    new Image
                    {
                        Source =
                            new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_040_stats.png", UriKind.Relative))
                    }
            };
            graphButton.Click += (sender, args) =>
            {
                if (_currentView == ReportViews.Graph) return;

                _currentView = ReportViews.Graph;
                 if (_graphView == null) _graphView = CreateGraphView();
                if (_configuration != null) _graphView.Configuration = _configuration;
                _view.Content = _graphView;
            };
            return graphButton;
        }

        private ReportGraph CreateGraphView()
        {
            var graphView = new ReportGraph(DateSettings, _dataSource){Interval = Interval};
            graphView.VolumeDateCountsDontMatch +=
                (sender, args) => MessageBox.Show("You don't have volume data imported for the range you specified");
            return graphView;
        }

        private Button CreateTableButton()
        {
            var tableButton = new Button
            {
                Style = Application.Current.FindResource("AtToolbarButtonStyle") as Style,
                Content =
                    new Image
                    {
                        Source =
                            new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_119_table.png", UriKind.Relative))
                    }
            };
            tableButton.Click += (sender, args) =>
            {
                if (_currentView == ReportViews.Table) return;

                _currentView = ReportViews.Table;
                if(_tableView == null) _tableView = CreateTableView();
                if (_configuration != null) _tableView.Configuration = _configuration;

                _view.Content = _tableView;
            };
            return tableButton;
        }

        private ReportTable CreateTableView()
        {
            var tableView = new ReportTable(DateSettings,_dataSource) {Interval = Interval};
            tableView.VolumeDateCountsDontMatch +=
                (sender, args) => MessageBox.Show("You don't have volume data imported for the range you specified");
            
            return tableView;
        }

        private static Label CreateIntervalLabel()
        {
            var label = new Label
            {
                Content = "Interval: ",
                Margin = new Thickness(5, 0, 0, 0),
                Foreground = Brushes.White,
                FontSize = 16
            };
            return label;
        }

        private ComboBox CreateComboBox()
        {
            var intervals = new ComboBox()
            {
                Padding = new Thickness(6, 0, 6, 0),
                Foreground = Brushes.Black,
                FontSize = 14,
                DataContext = this,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Height = 23
            };
            intervals.Items.Add("5 min");
            intervals.Items.Add("15 min");
            intervals.Items.Add("30 min");
            intervals.Items.Add("60 min");
            intervals.SelectedIndex = 0;

            intervals.SetBinding(Selector.SelectedValueProperty, new Binding("Interval") {Converter = new IntervalConverter()});
            return intervals;
        }

        #endregion

        public override UserControl GetView()
        {
            return _view;
        }

        public override void ShowConfigurationView()
        {
            //Lazily instatiate config view
            if (_configView == null)
            {
                _configView = new ReportConfig(_dataSource);
            }
            _currentView = ReportViews.Configuration;
            _view.Content = _configView;
        }

        public override void ShowConfigurable(Configurable configurable)
        {
            //Need to check if it's the same configuration before we go off and do a whole database call
            _configuration = _dataSource.GetConfiguration(configurable.Name);

            if (_currentView == ReportViews.Table || _currentView == ReportViews.Configuration) //If we're coming from config view
            {
                if (_tableView == null) _tableView = CreateTableView();
                _tableView.Configuration = _configuration;
                _view.Content = _tableView;
                _currentView = ReportViews.Table;
            }
            else if (_currentView == ReportViews.Graph)
            {
                //Graph view should never be null at this point because it is not the default view, but....
                if (_graphView == null) _graphView = CreateGraphView();
                _graphView.Configuration = _configuration;
                _view.Content = _graphView;
                _currentView = ReportViews.Graph;
            }
        }

        public override void EditConfigurable(Configurable configurable)
        {
            //Lazily instatiate config view
            if (_configView == null)
            {
                _configView = new ReportConfig(configurable.Name, _dataSource);
            }
            _currentView = ReportViews.Configuration;
            _view.Content = _configView;
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}