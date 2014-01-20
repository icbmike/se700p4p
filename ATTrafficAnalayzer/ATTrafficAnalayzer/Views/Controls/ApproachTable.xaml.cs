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
        private DateSettings _dateSettings;

        public ApproachTable(Approach approach, int intersection, DateSettings dateSettings)
        {
            _approach = approach;
            _intersection = intersection;
            _dateSettings = dateSettings;

            InitializeComponent();
            Render();
        }

        private void Render()
        {
            //Display the table
            var grid = new GridView();
            var dataTable = _approach.GetDataTable(_dateSettings, _intersection, 0);
            
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
            stringBuilder.AppendLine("Peak Volume: [b]" + _approach.GetPeak(_dateSettings, _intersection, 0) + "[/b] Peak time: [b]" +
                                     _approach.GetPeakTime(_dateSettings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.AppendLine("AM Peak: [b]" + _approach.GetAmPeak(_dateSettings, _intersection, 0) + "[/b] AM Peak time: [b]" + _approach.GetAmPeakTime(_dateSettings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.AppendLine("PM Peak: [b]" + _approach.GetPmPeak(_dateSettings, _intersection, 0) + "[/b] PM Peak time: [b]" + _approach.GetPmPeakTime(_dateSettings, _intersection, 0).ToShortTimeString() + "[/b]");
            stringBuilder.Append("Total volume: [b]" + _approach.GetTotal(_dateSettings, _intersection, 0) + "[/b]");
            ApproachSummary.Html = stringBuilder.ToString();
        }

        public void DateSettingsChanged(DateSettings newDateSettings)
        {
           //Don't do anything, approach tables aren't recycled
        }

        public void SelectedReportChanged(string newSelection)
        {
            //Don't do anything, approach tables aren't recycled
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
