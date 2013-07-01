using System.Collections.Generic;
using System.Windows.Controls;

namespace ATTrafficAnalayzer
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {
            InitializeComponent();

            var recentStandardReports = new List<string>
                {
                    "Recent Standard Report 1",
                    "Recent Standard Report 2",
                    "Recent Standard Report 3",
                    "Recent Standard Report 4",
                    "Recent Standard Report 5"
                };
            recentStandardReportsListBox.ItemsSource = recentStandardReports;

            var recentSpecialReports = new List<string>
                {
                    "Recent Special Report 1",
                    "Recent Special Report 2",
                    "Recent Special Report 3",
                    "Recent Special Report 4",
                    "Recent Special Report 5"
                };
            recentSpecialReportsListBox.ItemsSource = recentSpecialReports;

            Logger.Info("constructed view", "homescreen");
        }
    }
}
