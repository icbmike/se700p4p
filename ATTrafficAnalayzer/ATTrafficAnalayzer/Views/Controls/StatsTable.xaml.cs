using System;
using System.Collections.Generic;
using System.Data;
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
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Screens;

namespace ATTrafficAnalayzer.Views.Controls
{

    /// <summary>
    /// Interaction logic for StatsTable.xaml
    /// </summary>
    public partial class StatsTable :IView
    {
        private readonly IDataSource _dataSource;
        private readonly DateSettings _dateSettings;
        private readonly IEnumerable<SummaryRow> _summaryConfig;
        private readonly string _title;
        private readonly Func<DateTime, SummaryRow, int> _calculate;

        public StatsTable(IDataSource dataSource, DateSettings dateSettings, IEnumerable<SummaryRow> summaryConfig, string title, Func<DateTime, SummaryRow, int> calculate)
        {
            _dataSource = dataSource;
            _dateSettings = dateSettings;
            _summaryConfig = summaryConfig;
            _title = title;
            _calculate = calculate;
            InitializeComponent();

            Render();
        }

        private void Render()
        {
            StatsSummary.Html = "[b]" + _title + "[/b]";

            foreach (var summaryRow in _summaryConfig)
            {
                for (var day = _dateSettings.StartDate; day < _dateSettings.EndDate; day = day.AddDays(1))
                {
                    Console.Write(_calculate(day, summaryRow));
                    
                }
                Console.WriteLine();
            }
        }


        public void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }

        public void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args)
        {
            throw new NotImplementedException();
        }

        public event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }


}
