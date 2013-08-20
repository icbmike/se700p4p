using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar
    {
        public SettingsTray SettingsTray { get { return ToolbarPanel.DataContext as SettingsTray; } }

        public enum View { Table, Graph }
        private static View _view = View.Table;
        private static Mode _mode = Mode.Home;


        public Toolbar()
        {
            InitializeComponent();

            // Set default values
            StartDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            SummaryMonthComboBox.SelectedIndex = DateTime.Today.Month;
            SummaryYearComboBox.SelectedIndex = DateTime.Today.Year - 2012;

            ModeChanged += SwitchToolbar;
        }

        #region Mode/view switching events

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
            // Report & faults date pickers
            var isReportsOrFaults = e.Mode.Equals(Mode.Report) || e.Mode.Equals(Mode.Faults);
            StartDateLabel.Visibility = isReportsOrFaults ? Visibility.Visible : Visibility.Collapsed;
            StartDatePicker.Visibility = isReportsOrFaults ? Visibility.Visible : Visibility.Collapsed;
            EndDateLabel.Visibility = isReportsOrFaults ? Visibility.Visible : Visibility.Collapsed;
            EndDatePicker.Visibility = isReportsOrFaults ? Visibility.Visible : Visibility.Collapsed;

            // Report controls
            var isReportMode = e.Mode.Equals(Mode.Report);
            GraphButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            TableButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalLabel.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalComboBox.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;

            // Summary controls
            var isSummaryMode = e.Mode.Equals(Mode.Summary);
            SummaryMonthLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryMonthComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryYearLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryYearComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed; ;
        }

        #endregion

        #region View parameter events

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

            public DateRangeChangedEventHandlerArgs(int year, int month)
            {
                StartDate = new DateTime(year + 2012, month + 1, 1);
            }

            public DateTime GetStartDate()
            {
                return StartDate;
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

        private void SummaryComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DateRangeChanged != null)
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(SummaryYearComboBox.SelectedIndex, SummaryMonthComboBox.SelectedIndex));
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
