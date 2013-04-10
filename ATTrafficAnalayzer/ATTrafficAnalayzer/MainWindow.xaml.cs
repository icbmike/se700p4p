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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool mode = false;       // sm when true, vs when false
        bool display = false;    // graph when true, table when false

        public MainWindow()
        {
            InitializeComponent();

            List<string> StandardReports = new List<string>();
            StandardReports.Add("Standard Report 1");
            StandardReports.Add("Standard Report 2");
            StandardReports.Add("Standard Report 3");
            StandardReports.Add("Standard Report 4");
            StandardReports.Add("Standard Report 5");
            standardReportsListBox.ItemsSource = StandardReports;

            List<string> SpecialReports = new List<string>();
            SpecialReports.Add("Special Report 1");
            SpecialReports.Add("Special Report 2");
            SpecialReports.Add("Special Report 3");
            SpecialReports.Add("Special Report 4");
            SpecialReports.Add("Special Report 5");
            specialReportsListBox.ItemsSource = SpecialReports;

            this.mainContentControl.Content = new WelcomeScreen();
        }

        private void MenuItemImportClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("SICK");
        }

        private void switchDisplay(object sender, RoutedEventArgs e)
        {
            RadioButton button = (RadioButton) sender;

            if ((String) button.Content.ToString() == "Graph")
            {
                this.mainContentControl.Content = new SMGraph();
                display = true;

                if (mode)
                {
                    this.mainContentControl.Content = new SMGraph();
                }
                else
                {
                    this.mainContentControl.Content = new VSGraph();
                }
            }
            else if ((String)button.Content.ToString() == "Table")
            {
                this.mainContentControl.Content = new VSGraph();
                display = false;

                if (mode)
                {
                    this.mainContentControl.Content = new SMTable();
                }
                else
                {
                    this.mainContentControl.Content = new VSTable();
                }
            }
            else
            {
                Console.WriteLine(button.Content.ToString());
            }
        }

        private void vs(object sender, RoutedEventArgs e)
        {
            mode = false;
            if (display)
            {
                this.mainContentControl.Content = new VSGraph();
            }
            else
            {
                this.mainContentControl.Content = new VSTable();
            }
        }

        private void sm(object sender, RoutedEventArgs e)
        {
            mode = true;
            if (display)
            {
                this.mainContentControl.Content = new SMGraph();
            }
            else
            {
                this.mainContentControl.Content = new SMTable();
            }
        }

    }
}
