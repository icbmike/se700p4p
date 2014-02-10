using System;
using System.Windows.Controls;
using System.Windows.Data;
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
        private readonly Faults _faultsView;
        private int _threshold;

        public int Threshold
        {
            get { return _threshold; }
            set { _threshold = value;
                _faultsView.FaultThreshold = value;
            }
        }

        public FaultsMode(Action<BaseMode> modeChange, IDataSource dataSource, DateSettings dateSettings) : base(modeChange, dateSettings)
        {
            _faultsView = new Faults(dateSettings, dataSource);

            ModeName = "Faults";
            Image = new BitmapImage(new Uri("/Resources\\Images\\Icons\\glyphicons_078_warning_sign.png", UriKind.Relative));

        }

        public override UserControl GetView()
        {
            return _faultsView;
        }

        public override void PopulateToolbar(ToolBar toolbar)
        {
            var label = new Label
            {
                Content = "Fault Threshold: ",
                Foreground = Brushes.White,
                FontSize = 16
            };
            var textbox = new TextBox
            {
                DataContext = this,
                Height = 20,
                Width = 60
            };
            textbox.SetBinding(TextBox.TextProperty, new Binding("Threshold"){UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged});

            toolbar.Items.Add(label);
            toolbar.Items.Add(textbox);
        }

        public override void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            _faultsView.DateSettingsChanged();
        }

        public override ImageSource Image { get; protected set; }
        public override string ModeName { get; protected set; }
    }
}