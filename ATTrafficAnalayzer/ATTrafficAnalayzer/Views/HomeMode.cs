using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Properties;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views
{
    public class HomeMode : BaseMode
    {
        private readonly Home _homeView;

        public HomeMode(Action action, IDataSource dataSource) : base(action)
        {
            _homeView = new Home(dataSource);
        }

        public override List<IConfigurable> PopulateReportBrowser()
        {
            //Home mode doesn't display anything in the report browser
            return null;
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
            //Home mode doesn't do anything to the toolbar
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override UserControl GetView()
        {
            return _homeView;
        }

        public override Button ModeButton { get; protected set; }
    }
}