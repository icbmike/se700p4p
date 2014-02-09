using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Modes
{
    public class HomeMode : BaseMode
    {
        private readonly Home _homeView;

        public HomeMode(Action<BaseMode> action, IDataSource dataSource) : base(action, null)
        {
            ModeName = "Home";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_020_home.png", UriKind.Relative));
            _homeView = new Home(dataSource);
            
        }

        public event EventHandler ImportRequested
        {
            add { _homeView.ImportRequested += value; }
            remove { _homeView.ImportRequested -= value; }
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