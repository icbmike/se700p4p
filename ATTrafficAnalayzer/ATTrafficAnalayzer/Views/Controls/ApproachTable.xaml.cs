using System.Data;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models.ReportConfiguration;
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
            stringBuilder.AppendLine("Approach: [b]" + _approach.Name + "[/b]");
            stringBuilder.AppendLine("Peak Volume: [b]" + _approach.GetPeak(_settings, _intersection, 0) + "[/b] Peak time: [b]" +
                                     _approach.GetPeakTime(_settings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.AppendLine("AM Peak: [b]" + _approach.GetAmPeak(_settings, _intersection, 0) + "[/b] AM Peak time: [b]" + _approach.GetAmPeakTime(_settings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.AppendLine("PM Peak: [b]" + _approach.GetPmPeak(_settings, _intersection, 0) + "[/b] PM Peak time: [b]" + _approach.GetPmPeakTime(_settings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.Append("Total volume: [b]" + _approach.GetTotal(_settings, _intersection, 0) + "[/b]");
            ApproachSummary.Html = stringBuilder.ToString();
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
