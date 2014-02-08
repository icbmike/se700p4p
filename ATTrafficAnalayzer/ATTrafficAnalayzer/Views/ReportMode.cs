using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Screens;
using ToolBar = System.Windows.Controls.ToolBar;
using UserControl = System.Windows.Controls.UserControl;

namespace ATTrafficAnalayzer.Views
{
    public class ReportMode : BaseMode
    {
        private readonly IDataSource _dataSource;
        private ReportViews viewType;
        private UserControl view;
        private ReportTable tableView;
        private ReportGraph graphView;

        enum ReportViews
        {
            Table,
            Graph,
            Configuration
        }

        public ReportMode(Action<BaseMode> modeChange, IDataSource dataSource) : base(modeChange)
        {
            _dataSource = dataSource;
            viewType = ReportViews.Table;
            view = new UserControl();
        }

        public override List<IConfigurable> PopulateReportBrowser()
        {
            return null;
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
            var tableButton = new Button
            {
                Style = Application.Current.FindResource("AtToolbarButtonStyle") as Style,
                Content = new Image{Source = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_119_table.png", UriKind.Relative))}
            };
            tableButton.Click += (sender, args) =>
            {
                if (viewType != ReportViews.Graph) return;

                viewType = ReportViews.Table;
                //if(tableView == null) tableView = new ReportTable(_dataSource);
                view.Content = tableView;
            };

            var graphButton = new Button
            {
                Style = Application.Current.FindResource("AtToolbarButtonStyle") as Style,
                Content = new Image{Source =  new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_040_stats.png", UriKind.Relative))}
            };
            graphButton.Click += (sender, args) =>
            {
                if (viewType != ReportViews.Table) return;

                viewType = ReportViews.Graph;
               // if (graphView == null) graphView = new ReportGraph(_dataSource);
                view.Content = graphView;
            };

            toolbar.Items.Add(tableButton);
            toolbar.Items.Add(graphButton);

        }

        public override UserControl GetView()
        {
            return view;
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}