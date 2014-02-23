using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using ATTrafficAnalayzer.Annotations;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for Faults.xaml
    /// </summary>
    public partial class Faults
    {
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
                Render();
            }
        }

        /// <summary>
        /// Constructor for this screen.
        /// </summary>
        /// <param name="dateSettings">A dateSettings tray so that start date and end date can be retrieved on creation 
        /// TODO: replace with DateTimes
        /// 
        ///  </param>
        public Faults(DateSettings dateSettings, IDataSource dataSource) : base(dateSettings, dataSource)
        {
            DataContext = this;
            InitializeComponent();
            FaultThreshold = 150;
            Render();
        }

        /// <summary>
        /// Fetches data and displays it in the screen's data grid
        /// </summary>
        protected override void Render(bool reloadConfiguration = true)
        {
            if (!DataSource.VolumesExistForDateRange(DateSettings.StartDate, DateSettings.EndDate))
                MessageBox.Show("You haven't imported volume data for the selected date range");

            var faults = DataSource.GetSuspectedFaults(DateSettings.StartDate, DateSettings.EndDate, FaultThreshold);
            var transformedFaults = faults.Keys.ToDictionary(key => key, key => String.Join(", ", faults[key].Distinct().OrderBy(x => x)));
            FaultsDataGrid.ItemsSource = transformedFaults;
        }

    }
}
