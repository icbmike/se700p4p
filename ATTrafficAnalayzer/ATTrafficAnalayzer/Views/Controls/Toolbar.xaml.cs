using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar : UserControl
    {

        public delegate void ScreenChangeEventHandler(object sender, ScreenChangeEventHandlerArgs args);
        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventHandlerArgs args);

        public event ScreenChangeEventHandler ScreenChanged;
        public event DateRangeChangedEventHandler DateRangeChanged;

        public class ScreenChangeEventHandlerArgs
        {
            public enum ScreenButton
            {
                Graph, Table, Faults
            }

            public ScreenChangeEventHandlerArgs(ScreenButton button)
            {
                this.button = button;
            }

            public ScreenButton button { get; set; }
        }

        public class DateRangeChangedEventHandlerArgs
        {
            public DateRangeChangedEventHandlerArgs(DateTime startDate, DateTime endDate, int interval)
            {
                this.startDate = startDate;
                this.endDate = endDate;
                this.interval = interval;
            }

            public DateTime startDate { get; set; }
            public DateTime endDate { get; set; }
            public int interval { get; set; }
        }

        public Toolbar()
        {
            InitializeComponent();
            StartDatePicker.SelectedDate = new DateTime(2013, 3, 11);
            EndDatePicker.SelectedDate = new DateTime(2013, 3, 12);

        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {
            if (sender.Equals(GraphButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Graph));
            }
            else if (sender.Equals(TableButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Table));
            }
            else
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Faults));
            }
        }

        private void DateOrInverval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender.Equals(StartDatePicker))
                if (EndDatePicker != null)
                    EndDatePicker.SelectedDate = StartDatePicker.SelectedDate.Value.AddDays(1);
            if (DateRangeChanged != null)
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(StartDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value, (toolbarPanel.DataContext as SettingsTray).Interval));
        }

        private void HomeImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

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

        public DateTime StartDate { get { return StartDatePicker.SelectedDate.Value; } }

        public DateTime EndDate { get { return EndDatePicker.SelectedDate.Value; } }

        public SettingsTray SettingsTray { get { return toolbarPanel.DataContext as SettingsTray; } }
    }
}
