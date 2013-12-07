using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models.Configuration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for ApproachTable.xaml
    /// </summary>
    public partial class ApproachTable : IView
    {
        private readonly Approach _approach;
        private readonly int _intersection;
        private readonly SettingsTray _settings;

        public ApproachTable(Approach approach, int intersection, SettingsTray settings)
        {
            _approach = approach;
            _intersection = intersection;
            _settings = settings;
            InitializeComponent();
            Render();
        }

        private void Render()
        {
            var grid = new GridView();
            var dataTable = _approach.GetDataTable(_settings, _intersection, 24, 0, 0);
            foreach (DataColumn col in dataTable.Columns)
            {
                grid.Columns.Add(new GridViewColumn
                {
                    Header = col.ColumnName,
                    DisplayMemberBinding = new Binding(col.ColumnName)
                });
            }
            
            ApproachListView.View = grid;
            ApproachListView.ItemsSource = dataTable.Rows;
            
            DataContext = dataTable;

            SetBinding(ListView.ItemsSourceProperty, new Binding());
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
