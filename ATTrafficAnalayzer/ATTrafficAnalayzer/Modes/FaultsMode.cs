using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Modes
{
    internal class FaultsMode : BaseMode
    {
        private readonly Faults faultsView;

        public FaultsMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange, dateSettings)
        {
            faultsView = new Faults(dateSettings, dataSource);

            ModeName = "Faults";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_078_warning_sign.png", UriKind.Relative));

        }

        public override UserControl GetView()
        {
            return faultsView;
        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            faultsView.DateSettingsChanged();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}