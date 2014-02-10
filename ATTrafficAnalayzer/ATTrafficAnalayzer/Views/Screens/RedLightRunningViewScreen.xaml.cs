using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for RedLightRunningViewScreen.xaml
    /// </summary>
    public partial class RedLightRunningViewScreen
    {
        private RedLightRunningConfiguration _configuration;

        public RedLightRunningViewScreen(DateSettings dateSettings, IDataSource dataSource)
        {
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
            Content = Configuration.Name;
        }
    }
}
