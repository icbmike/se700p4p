﻿using ATTrafficAnalayzer.Models.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for Toolbar.xaml
    /// </summary>
    public partial class Toolbar
    {

        public delegate void ScreenChangeEventHandler(object sender, ScreenChangeEventHandlerArgs args);
        public delegate void DateRangeChangedEventHandler(object sender, DateRangeChangedEventHandlerArgs args);

        public event ScreenChangeEventHandler ScreenChanged;
        public event DateRangeChangedEventHandler DateRangeChanged;

        private Boolean _startModifyingEnd;

        public class ScreenChangeEventHandlerArgs
        {
            public enum ScreenButton
            {
                Graph, Table, Faults, Home
            }

            public ScreenChangeEventHandlerArgs(ScreenButton button)
            {
                Button = button;
            }

            public ScreenButton Button { get; set; }
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
        }

        private void SwitchScreen(object sender, RoutedEventArgs e)
        {

            Console.WriteLine(ToolbarPanel.DataContext);

            if (sender.Equals(GraphButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Graph));
            }
            else if (sender.Equals(TableButton))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Table));
            }
            else if (sender.Equals(HomeImage))
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Home));
            }
            else
            {
                ScreenChanged(this, new ScreenChangeEventHandlerArgs(ScreenChangeEventHandlerArgs.ScreenButton.Faults));
            }
        }

        private void DateOrInverval_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (sender.Equals(StartDatePicker))
            {
                Console.WriteLine("Start date picker...");
                if (EndDatePicker != null)
                {
                    Console.WriteLine("true");
                    _startModifyingEnd = true;
                    var newDate = StartDatePicker.SelectedDate.Value.AddDays(1);
                    EndDatePicker.SelectedDate = newDate;
                    
                }

            }
            else if (sender.Equals(EndDatePicker))
            {
                Console.WriteLine("End date picker...");
                if (_startModifyingEnd)
                {
                    Console.WriteLine("false");
                    _startModifyingEnd = false;
                    return;
                }
            }

            if (DateRangeChanged != null)
            {
                Console.WriteLine("Firing the actual event");
                DateRangeChanged(this, new DateRangeChangedEventHandlerArgs(StartDatePicker.SelectedDate.Value, EndDatePicker.SelectedDate.Value, (ToolbarPanel.DataContext as SettingsTray).Interval));
            }
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

        public SettingsTray SettingsTray { get { return ToolbarPanel.DataContext as SettingsTray; } }
    }
}
