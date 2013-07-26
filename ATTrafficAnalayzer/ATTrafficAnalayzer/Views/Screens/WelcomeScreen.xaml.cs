using System.Collections.Generic;
using System.Windows.Controls;
using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for WelcomeScreen.xaml
    /// </summary>
    public partial class WelcomeScreen : UserControl
    {
        public WelcomeScreen()
        {
            InitializeComponent();

            DbHelper helper = DbHelper.GetDbHelper();

            var importedDates = helper.GetImportedDates();
            recentStandardReportsListBox.ItemsSource = importedDates;

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
