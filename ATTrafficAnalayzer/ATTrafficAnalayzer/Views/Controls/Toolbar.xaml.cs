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
    public partial class Toolbar : INotifyPropertyChanged
    {
        public SettingsTray SettingsTray { get { return ToolbarPanel.DataContext as SettingsTray; } }

        public enum View { Table, Graph }
        private static View _view = View.Table;
        private static Mode _mode = Mode.Home;

        public Toolbar()
        {
            InitializeComponent();

            StartDate = new DateTime(2013, 3, 11);

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

            // Report controls
            var isReportMode = e.Mode.Equals(Mode.Report);
            GraphButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            TableButton.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalLabel.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;
            IntervalComboBox.Visibility = isReportMode ? Visibility.Visible : Visibility.Collapsed;

            // Summary controls
            var isSummaryMode = e.Mode.Equals(Mode.Summary);
            SummaryAmPeakLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryAmPeakComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed ;
            SummaryPmPeakLabel.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
            SummaryPmPeakComboBox.Visibility = isSummaryMode ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Toolbar Event Handlers

        private DateTime _startDate;
        private DateTime _endDate;

        public DateTime StartDate { get {return _startDate;} set { _startDate = value; OnPropertyChanged("StartDate");} }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; OnPropertyChanged("EndDate"); } } 
        public int Month { get { return StartDatePicker.SelectedDate.Value.Month; }}

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

        private void DateAndInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
             System.Windows.Forms.MessageBox.Show(string.Format("Test - Sender: {0}", sender.ToString()));

             //if (sender.Equals(StartDatePicker))
             //{
             //    if (EndDatePicker != null)
             //    {
             //        _startModifyingEnd = true;
             //        EndDatePicker.SelectedDate = StartDatePicker.SelectedDate.Value.AddDays(1);
             //    }
             //}
             //else if (sender.Equals(EndDatePicker))
             //{
             //    if (_startModifyingEnd)
             //    {
             //        _startModifyingEnd = false;
             //        return;
             //    }
             //}

             if (DateRangeChanged != null)
             {
                 System.Windows.Forms.MessageBox.Show("Thowing event");
                 DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(StartDate, EndDate, (ToolbarPanel.DataContext as SettingsTray).Interval));
             }
             else
             {
                 System.Windows.Forms.MessageBox.Show("It's null");
             }
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
    
public event PropertyChangedEventHandler PropertyChanged;
}
}
