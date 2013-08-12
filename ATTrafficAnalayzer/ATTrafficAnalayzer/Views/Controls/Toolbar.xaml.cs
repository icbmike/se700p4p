using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar
    {
        public SettingsTray SettingsTray { get { return ToolbarPanel.DataContext as SettingsTray; } }

        public Toolbar()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            ModeChanged += Toolbar_ModeChanged;
        }

        private void Toolbar_ModeChanged(object sender, ModeChangedEventHandlerArgs args)
        {
            if (args.SelectedMode.Equals(Mode.MonthlySummary))
            {
                //Remove the view Buttons
                GraphButton.Visibility = Visibility.Collapsed;
                TableButton.Visibility = Visibility.Collapsed;
                FaultsButton.Visibility = Visibility.Collapsed;
                
                //Add summary Button
                
                //Remove End Date and Interval
                EndDatePicker.Visibility = Visibility.Collapsed;
                IntervalComboBox.Visibility = Visibility.Collapsed;
            }
            else
            {
                //Add the view Buttons
                GraphButton.Visibility = Visibility.Collapsed;
                TableButton.Visibility = Visibility.Collapsed;
                FaultsButton.Visibility = Visibility.Collapsed;

                //Add End Date and Interval
                EndDatePicker.Visibility = Visibility.Collapsed;
                IntervalComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private void MainToolbar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }

        #region Screen refreshing

        private ScreenButton currentScreen = ScreenButton.Home;
        private Mode currentMode = Mode.RegularReports;

        public enum ScreenButton
        {
            Graph,
            Table,
            Faults,
            Summary,
            Home
        }

        public delegate void ScreenChangeEventHandler(object sender, ScreenChangeEventHandlerArgs args);
        public event ScreenChangeEventHandler ScreenChanged;

        public class ScreenChangeEventHandlerArgs
        {
            public ScreenChangeEventHandlerArgs(ScreenButton button)
            {
                Button = button;
            }

            public ScreenButton Button { get; set; }
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(GraphButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Graph));
            }
            else if (sender.Equals(TableButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Table));
            }
            else if (sender.Equals(HomeButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Home));
            }
            else if (sender.Equals(MonthlySummaryButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Summary));
            }
            else
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Faults));
            }
        }

        private void SwitchMode(object sender, RoutedEventArgs e)
        {
             ModeChanged(this, new ModeChangedEventHandlerArgs(sender.Equals(MonthlySummaryButton) ? Mode.MonthlySummary : Mode.RegularReports));
        }

        #endregion


        public delegate void ModeChangedEventHandler(object sender, ModeChangedEventHandlerArgs args);

        public event ModeChangedEventHandler ModeChanged;

        public class ModeChangedEventHandlerArgs
        {
            private readonly Mode _mode;

            public ModeChangedEventHandlerArgs(Mode mode)
            {
                _mode = mode;
            }

            public Mode SelectedMode
            {
                get { return _mode; }
            }
        }

        #region Date refreshing

        private Boolean _startModifyingEnd;

        public DateTime StartDate { get { return StartDatePicker.SelectedDate.Value; } }
        public DateTime EndDate { get { return EndDatePicker.SelectedDate.Value; } }

        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventHandlerArgs args);
        public event DateRangeChangedEventHandler DateRangeChanged;

        public class DateRangeChangedEventHandlerArgs
        {
            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public int interval { get; set; }

            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate, int interval)
            {
                this.startDate = startDate;
                this.endDate = endDate;
                this.interval = interval;
            }
        }

        private void DateOrInverval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.Equals(StartDatePicker))
            {
                Console.WriteLine("Start date picker...");
                if (EndDatePicker != null)
                {
                    Console.WriteLine("true");
                    _startModifyingEnd = true;
                    var newDate = StartDatePicker.SelectedDate.Value.AddDays(1);
                    EndDatePicker.SelectedDate = newDate;
                }
            }
            else if (sender.Equals(EndDatePicker))
            {
                Console.WriteLine("End date picker...");
                if (_startModifyingEnd)
                {
                    Console.WriteLine("false");
                    _startModifyingEnd = false;
                    return;
                }
            }

            if (DateRangeChanged != null)
            {
                Console.WriteLine("Firing the actual event");
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(StartDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value, (ToolbarPanel.DataContext as SettingsTray).Interval));
            }
        }

        #endregion

        public enum Mode
        {
            MonthlySummary, RegularReports
        }
    }
}
