using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Properties;
using ATTrafficAnalayzer.Views.Screens;

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

            // Set default dates
            StartDatePicker.SelectedDate = DateTime.Today;

            ModeChanged += Toolbar_ModeChanged;
        }

        #region Screen events

        public enum ScreenButton { Graph, Table, Faults, Summary, Home }

        public delegate void ScreenChangeEventHandler(object sender, ScreenChangeEventHandlerArgs args);
        public event ScreenChangeEventHandler ScreenChanged;
        public class ScreenChangeEventHandlerArgs
        {
            public ScreenButton Button { get; set; }

            public ScreenChangeEventHandlerArgs(ScreenButton button)
            {
                Button = button;
            }
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(GraphButton))
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Graph));
            else if (sender.Equals(TableButton))
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Table));
            else if (sender.Equals(HomeButton))
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Home));
            else
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenButton.Faults));
        }

        #endregion

        #region Mode events

        public delegate void ModeChangedEventHandler(object sender, ModeChangedEventHandlerArgs args);
        public event ModeChangedEventHandler ModeChanged;
        public class ModeChangedEventHandlerArgs
        {
            private readonly Mode _mode;
            public ModeChangedEventHandlerArgs(Mode mode) { _mode = mode; }
            public Mode SelectedMode { get { return _mode; } }
        }

        private void SwitchMode(object sender, RoutedEventArgs e) { ModeChanged(this, new ModeChangedEventHandlerArgs(sender.Equals(MonthlySummaryButton) ? Mode.MonthlySummary : Mode.RegularReports)); }

        private void Toolbar_ModeChanged(object sender, ModeChangedEventHandlerArgs args)
        {
            if (args.SelectedMode.Equals(Mode.MonthlySummary))
            {
                GraphButton.Visibility = Visibility.Collapsed;
                StartDateLabel.Visibility = Visibility.Collapsed;
                StartDatePicker.Visibility = Visibility.Collapsed;
                EndDateLabel.Visibility = Visibility.Collapsed;
                EndDatePicker.Visibility = Visibility.Collapsed;
                IntervalLabel.Visibility = Visibility.Collapsed;
                IntervalComboBox.Visibility = Visibility.Collapsed;
                SummaryDateLabel.Visibility = Visibility.Visible;
                SummaryMonthComboBox.Visibility = Visibility.Visible;
                SummaryYearComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                GraphButton.Visibility = Visibility.Visible;
                StartDateLabel.Visibility = Visibility.Visible;
                StartDatePicker.Visibility = Visibility.Visible;
                EndDateLabel.Visibility = Visibility.Visible;
                IntervalLabel.Visibility = Visibility.Visible;
                EndDatePicker.Visibility = Visibility.Visible;
                IntervalComboBox.Visibility = Visibility.Visible;
                SummaryDateLabel.Visibility = Visibility.Collapsed;
                SummaryMonthComboBox.Visibility = Visibility.Collapsed;
                SummaryYearComboBox.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region Date events

        public DateTime StartDate { get { return StartDatePicker.SelectedDate.Value; } }
        public DateTime EndDate { get { return EndDatePicker.SelectedDate.Value; } }
        public int Month { get { return StartDatePicker.SelectedDate.Value.Month; } }

        private Boolean _startModifyingEnd;
        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventHandlerArgs args);
        public event DateRangeChangedEventHandler DateRangeChanged;
        public class DateRangeChangedEventHandlerArgs
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int Interval { get; set; }

            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate, int interval)
            {
                StartDate = startDate;
                EndDate = endDate;
                Interval = interval;
            }
        }

        private void DateOrInverval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.Equals(StartDatePicker))
            {
                if (EndDatePicker != null)
                {
                    _startModifyingEnd = true;
                    var newDate = StartDatePicker.SelectedDate.Value.AddDays(1);
                    EndDatePicker.SelectedDate = newDate;
                }
            }
            else if (sender.Equals(EndDatePicker))
            {
                if (_startModifyingEnd)
                {
                    _startModifyingEnd = false;
                    return;
                }
            }

            if (DateRangeChanged != null)
            {
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(StartDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value, (ToolbarPanel.DataContext as SettingsTray).Interval));
            }
        }

        #endregion

        #region Summary events

        public delegate void SummaryDateChangedEventHandler(object sender, SummaryDateChangedEventArgs args);
        public event SummaryDateChangedEventHandler SummaryDateChanged;
        public class SummaryDateChangedEventArgs
        {
            public int SummaryMonth { get; set; }
            public int SummaryYear { get; set; }

            public SummaryDateChangedEventArgs(int year, int month)
            {
                SummaryMonth = month;
                SummaryYear = year;
            }
        }

        private void SummaryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SummaryDateChanged(this, new SummaryDateChangedEventArgs(SummaryMonthComboBox.SelectedIndex, SummaryYearComboBox.SelectedIndex));
        }

        #endregion

        /// <summary>
        /// Removes overflow tab from the toolbar's right-end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainToolbar_OnLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }
        }
    }
}
