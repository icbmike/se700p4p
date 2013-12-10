using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using ATTrafficAnalayzer.Annotations;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar
    {
        public DateSettings SettingsTray { get { return ToolbarPanel.DataContext as DateSettings; } }

        public enum View { Table, Graph }
        private static View _view = View.Table;
        private static Mode _mode = Mode.Home;

        public Toolbar()
        {
            InitializeComponent();

            SettingsTray.StartDate = DbHelper.GetDbHelper().GetMostRecentImportedDate();

            ModeChanged += SwitchToolbar;
        }

        #region Mode/View Switching Events Handlers

        public delegate void ModeChangedEventHandler(object sender, ModeChangedEventHandlerArgs args);
        public event ModeChangedEventHandler ModeChanged;
        public class ModeChangedEventHandlerArgs
        {
            public ModeChangedEventHandlerArgs(Mode mode, View view)
            {
                _mode = mode;
                _view = view;
            }

            public ModeChangedEventHandlerArgs(View view)
            {
                _view = view;
            }

            public ModeChangedEventHandlerArgs(Mode mode)
            {
                _mode = mode;
            }

            public Mode Mode { get { return _mode; } }
            public View View { get { return _view; } }
        }

        private void SwitchMode(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(HomeButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(Mode.Home));
            else if (sender.Equals(RegularReportsButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(Mode.Report));
            else if (sender.Equals(MonthlySummaryButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(Mode.Summary));
            else if (sender.Equals(FaultsButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(Mode.Faults));
        }

        private void SwitchView(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(GraphButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(View.Graph));
            else if (sender.Equals(TableButton))
                ModeChanged(this, new ModeChangedEventHandlerArgs(View.Table));
        }

        private void SwitchToolbar(object sender, ModeChangedEventHandlerArgs e)
        {
            // Date pickers
            var isHomeMode = e.Mode.Equals(Mode.Home);
            StartDateLabel.Visibility = isHomeMode ? Visibility.Collapsed : Visibility.Visible;
            StartDatePicker.Visibility = isHomeMode ? Visibility.Collapsed : Visibility.Visible;
            EndDateLabel.Visibility = isHomeMode ? Visibility.Collapsed : Visibility.Visible;
            EndDatePicker.Visibility = isHomeMode ? Visibility.Collapsed : Visibility.Visible;

            // Configuration controls
            var isReportMode = e.Mode.Equals(Mode.Report);
            GraphButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            TableButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalLabel.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalComboBox.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;

            // Summary controls
            var isSummaryMode = e.Mode.Equals(Mode.Summary);
            SummaryAmPeakLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryAmPeakComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryPmPeakLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryPmPeakComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Toolbar Event Handlers

        public DateTime StartDate { get { return SettingsTray.StartDate; } }
        public DateTime EndDate { get { return SettingsTray.EndDate; } }
        public int Month { get { return StartDate.Month; } }

        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventHandlerArgs args);
        public event DateRangeChangedEventHandler DateRangeChanged;
        public class DateRangeChangedEventHandlerArgs
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int AmPeakHour { get; set; }
            public int PmPeakHour { get; set; }
            public int Interval { get; set; }

            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate, int amPeakHour, int pmPeakHour)
            {
                StartDate = startDate;
                EndDate = endDate;
                AmPeakHour = amPeakHour;
                PmPeakHour = pmPeakHour;
            }

            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate, int interval)
            {
                StartDate = startDate;
                EndDate = endDate;
                Interval = interval;
            }

            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate)
            {
                StartDate = startDate;
                EndDate = endDate;
            }
        }

        private Boolean _startModifyingEnd;
        private DateTime _prevStartDate = new DateTime();
        private DateTime _prevEndDate = new DateTime();
        private int _prevInterval = 0;

        private void DateAndInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsTray.StartDate.Equals(_prevStartDate) && SettingsTray.EndDate.Equals(_prevEndDate) && SettingsTray.Interval.Equals(_prevInterval))
            {
                return;
            }
            else
            {
                _prevStartDate = SettingsTray.StartDate;
                _prevEndDate = SettingsTray.EndDate;
                _prevInterval = SettingsTray.Interval;
            }

            if (sender.Equals(StartDatePicker))
            {
                if (EndDatePicker != null)
                {
                    _startModifyingEnd = true;
                    EndDatePicker.SelectedDate = StartDatePicker.SelectedDate.Value.AddDays(1);
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
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(SettingsTray.StartDate, SettingsTray.EndDate, SettingsTray.Interval));
        }

        private void SummaryControls_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateRangeChanged != null)
            {
                DateRangeChanged(this,
                    new DateRangeChangedEventHandlerArgs(StartDatePicker.SelectedDate.Value,
                        EndDatePicker.SelectedDate.Value, SummaryAmPeakComboBox.SelectedIndex,
                        SummaryPmPeakComboBox.SelectedIndex));
            }
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
