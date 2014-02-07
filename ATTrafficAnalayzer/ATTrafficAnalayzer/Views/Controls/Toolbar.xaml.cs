using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
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
        public DateSettings DateSettings { get { return DateRangeToolBar.DataContext as DateSettings; } }

        public ObservableCollection<ToolbarButton> Modes { get; set; }
        public int Month { get { return DateSettings.StartDate.Month; }  }

        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args);
        public event DateRangeChangedEventHandler DateRangeChanged;

        public Toolbar()
        {
            InitializeComponent();
            Modes = new ObservableCollection<ToolbarButton>();
            DateSettings.StartDate = DataSourceFactory.GetDataSource().GetMostRecentImportedDate();

        }

        private void DateAndInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }

    public class DateRangeChangedEventArgs
    {
    }
}
