using System;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;
using QuickZip.MiniHtml2;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ApproachTable.xaml
    /// </summary>
    public partial class ApproachTable
    {

        public ApproachTable()
        {
            InitializeComponent();

        }

        public static readonly DependencyProperty DateSettingsProperty =
            DependencyProperty.Register("DateSettings", typeof (DateSettings), typeof (ApproachTable));

        public static readonly DependencyProperty IntersectionProperty =
            DependencyProperty.Register("Intersection", typeof(int), typeof(ApproachTable));


        public DateSettings DateSettings { 
            get { return (DateSettings) GetValue(DateSettingsProperty); } 
            set{ SetValue(DateSettingsProperty, value);} 
        }

        public int Intersection
        {
            get { return (int) GetValue(IntersectionProperty); } 
            set { SetValue(IntersectionProperty, value); }
        }

        private void ApproachTableOnLoaded(object sender, RoutedEventArgs e)
        {
            //Display the table
            var grid = new GridView();

            var approach = DataContext as Approach;

            var dataTable = approach.GetDataTable(DateSettings, Intersection, 0);

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
        }
    }
}
