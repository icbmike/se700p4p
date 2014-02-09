using System.Collections.ObjectModel;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Modes;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar
    {
        public DateSettings DateSettings { get { return DateRangeToolBar.DataContext as DateSettings; } }

        public ObservableCollection<BaseMode> Modes { get; set; }
        public int Month { get { return DateSettings.StartDate.Month; }  }

        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args);
        public event DateRangeChangedEventHandler DateRangeChanged;

        public Toolbar()
        {
            
            Modes = new ObservableCollection<BaseMode>();
            
            InitializeComponent();
            
            DateSettings.StartDate = DataSourceFactory.GetDataSource().GetMostRecentImportedDate();
            DateSettings.EndDate = DateSettings.StartDate.AddDays(1);

        }

        private void DateAndInterval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Toolbar_OnLoaded(object sender, RoutedEventArgs e)
        {
            
        }
    }

    public class DateRangeChangedEventArgs
    {
    }
}
