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
        private bool startSettingEnd;
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

        }

        private void StartDatePickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (EndDatePicker != null)
            {
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate.Value.AddDays(1);
            }

            if (!StartDatePicker.IsDropDownOpen) return;

            RaiseDateRangeChanged();
        }

        private void RaiseDateRangeChanged()
        {
            if (DateRangeChanged != null)
                DateRangeChanged(this,
                    new DateRangeChangedEventArgs {StartDate = DateSettings.StartDate, EndDate = DateSettings.EndDate});
        }

        private void EndDatePickerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!EndDatePicker.IsDropDownOpen) return;

            RaiseDateRangeChanged();            
        }
    }

    public class DateRangeChangedEventArgs
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
