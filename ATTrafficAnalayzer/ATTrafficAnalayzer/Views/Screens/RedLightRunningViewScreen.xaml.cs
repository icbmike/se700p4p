using System.Data;
using System.Windows.Controls;
using System.Windows.Data;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for RedLightRunningViewScreen.xaml
    /// </summary>
    public partial class RedLightRunningViewScreen
    {
        private readonly DateSettings _dateSettings;
        private readonly IDataSource _dataSource;
        private RedLightRunningConfiguration _configuration;

        public RedLightRunningViewScreen(DateSettings dateSettings, IDataSource dataSource)
        {
            _dateSettings = dateSettings;
            _dataSource = dataSource;
            InitializeComponent();
        }

        public RedLightRunningConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value;
                Render();
            }
        }

        private void Render()
        {
            ScreenTitle.Content = Configuration.Name;

            var dataTable = Configuration.GetDataTable(_dateSettings, _dataSource);

            var grid = new GridView();
            foreach (DataColumn col in dataTable.Columns)
            {
                grid.Columns.Add(new GridViewColumn
                {
                    Header = col.ColumnName,
                    DisplayMemberBinding = new Binding(col.ColumnName),
                });
            }

            Grid.View = grid;
            Grid.ItemsSource = dataTable.DefaultView;
        }

        public void DateRangeChanged()
        {
            if(Configuration != null) Render();
        }
    }
}
