using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Screens;
using ToolBar = System.Windows.Controls.ToolBar;
using UserControl = System.Windows.Controls.UserControl;

namespace ATTrafficAnalayzer.Views
{
    public class ReportMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private ReportViews _viewType;
        private readonly UserControl _view;

        private ReportTable tableView;
        private ReportGraph graphView;
        private UserControl _configView;

        public int Interval { get; set; }

        enum ReportViews
        {
            Table,
            Graph,
            Configuration
        }

        public ReportMode(Action<BaseMode> modeChange, IDataSource dataSource) : base(modeChange)
        {
            _dataSource = dataSource;
            _viewType = ReportViews.Table;
            _view = new UserControl();
            Interval = 5;
        }

        public override List<IConfigurable> PopulateReportBrowser()
        {
            return null;
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
                if (_viewType != ReportViews.Table) return;

                _viewType = ReportViews.Graph;
                // if (graphView == null) graphView = new ReportGraph(_dataSource);
                _view.Content = graphView;
            };
            return graphButton;
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
                if (_viewType != ReportViews.Graph) return;

                _viewType = ReportViews.Table;
                //if(tableView == null) tableView = new ReportTable(_dataSource);
                _view.Content = tableView;
            };
            return tableButton;
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

        public override UserControl GetConfigurationView()
        {
            if (_configView != null) return _configView;

            _configView = new ReportConfig(_dataSource);
            return _configView;
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}