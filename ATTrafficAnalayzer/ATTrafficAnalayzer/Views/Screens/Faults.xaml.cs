using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Annotations;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Faults.xaml
    /// </summary>
    public partial class Faults : IView, INotifyPropertyChanged
    {
        private readonly IDataSource _dataSource;
        private DateTime _endDate;
        private DateTime _startDate;
        private int _faultThreshold;

        /// <summary>
        /// Bound Property to the Fault threshold control in XAML
        /// </summary>
        public int FaultThreshold
        {
            get { return _faultThreshold; }
            set
            {
                _faultThreshold = value;
                OnPropertyChanged("FaultThreshold");
            }
        }

        /// <summary>
        /// Constructor for this screen.
        /// </summary>
        /// <param name="settings">A settings tray so that start date and end date can be retrieved on creation 
        /// TODO: replace with DateTimes
        /// 
        ///  </param>
        public Faults(DateSettings settings, IDataSource dataSource)
        {
            _startDate = settings.StartDate;
            _endDate = settings.EndDate;
            _dataSource = dataSource;
            DataContext = this;
            InitializeComponent();
            FaultThreshold = 150;
            Render();
        }

        /// <summary>
        /// Fetches data and displays it in the screen's data grid
        /// </summary>
        private void Render()
        {
            if (!DbHelper.GetDbHelper().VolumesExist(_startDate, _endDate))
                MessageBox.Show("You haven't imported volume data for the selected date range");

            var faults = _dataSource.GetSuspectedFaults(_startDate, _endDate, FaultThreshold);
            var transformedFaults = faults.Keys.ToDictionary(key => key, key => String.Join(", ", faults[key].Distinct().OrderBy(x => x)));
            FaultsDataGrid.ItemsSource = transformedFaults;
        }

        #region Event Handlers

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;


        /// <summary>
        /// Handler for when the date range in the Toolbar is changed
        /// </summary>
        /// <param name="sender">The Toolbar</param>
        /// <param name="args">Event args containing the new start and end datetime objects</param>
        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            if (!args.StartDate.Equals(_startDate) || !args.EndDate.Equals(_endDate))
            {
                _startDate = args.StartDate;
                _endDate = args.EndDate;

                Render();
            }
        }
        /// <summary>
        /// Not needed to be implemented for this screen, as it does not rely on reports
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }

        #endregion

        /// <summary>
        /// Click handler for the refresh button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshFaults_OnClick(object sender, RoutedEventArgs e)
        {
            Render();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
