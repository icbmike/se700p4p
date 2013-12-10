﻿using System.Data;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;
using System.Windows;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ApproachTable.xaml
    /// </summary>
    public partial class ApproachTable : IView
    {
        private readonly Approach _approach;
        private readonly int _intersection;
        private readonly DateSettings _settings;

        public ApproachTable(Approach approach, int intersection, DateSettings settings)
        {
            _approach = approach;
            _intersection = intersection;
            _settings = settings;

            InitializeComponent();
            Render();
        }

        private void Render()
        {
            //Display the table
            var grid = new GridView();
            var dataTable = _approach.GetDataTable(_settings, _intersection, 0);
            
            foreach (DataColumn col in dataTable.Columns)
            {
                grid.Columns.Add(new GridViewColumn
                {
                    Header = col.ColumnName,
                    DisplayMemberBinding = new Binding(col.ColumnName),
                    Width = col.ColumnName.Equals("Time") ? 55 : 44
                });
            }
                     
            ApproachListView.View = grid;
            ApproachListView.ItemsSource = dataTable.DefaultView;
            
            //Display the statistics
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Peak Volume: " + _approach.GetPeak(_settings, _intersection, 0) + " Peak time: " +
                                     _approach.GetPeakTime(_settings, _intersection, 0));
            stringBuilder.AppendLine("AM Peak: " + _approach.GetAmPeak(_settings, _intersection, 0) + " Peak time: " + _approach.GetAmPeakTime(_settings, _intersection, 0));
            stringBuilder.AppendLine("Pm Peak: " + _approach.GetPmPeak(_settings, _intersection, 0) + " Peak time: " + _approach.GetPmPeakTime(_settings, _intersection, 0));
            ApproachSummary.Inlines.Add(stringBuilder.ToString());
        }

        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            throw new System.NotImplementedException();
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            throw new System.NotImplementedException();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
