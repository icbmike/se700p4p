using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Modes;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views
{
    public class HomeMode : BaseMode
    {
        private readonly Home _homeView;

        public HomeMode(Action<BaseMode> action, IDataSource dataSource) : base(action)
        {
            ModeName = "Home";
            try
            {
                Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_020_home.png", UriKind.Relative));
            }
            catch (Exception e)
            {
                
            }
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

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }

   
}