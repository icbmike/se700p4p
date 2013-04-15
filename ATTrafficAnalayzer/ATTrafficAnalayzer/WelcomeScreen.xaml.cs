using System;
using System.Collections.Generic;
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

            List<string> recentStandardReports = new List<string>();
            recentStandardReports.Add("Recent Standard Report 1");
            recentStandardReports.Add("Recent Standard Report 2");
            recentStandardReports.Add("Recent Standard Report 3");
            recentStandardReports.Add("Recent Standard Report 4");
            recentStandardReports.Add("Recent Standard Report 5");
            recentStandardReportsListBox.ItemsSource = recentStandardReports;

            List<string> recentSpecialReports = new List<string>();
            recentSpecialReports.Add("Recent Standard Report 1");
            recentSpecialReports.Add("Recent Standard Report 2");
            recentSpecialReports.Add("Recent Standard Report 3");
            recentSpecialReports.Add("Recent Standard Report 4");
            recentSpecialReports.Add("Recent Standard Report 5");
            recentSpecialReportsListBox.ItemsSource = recentSpecialReports;
        }
    }
}
