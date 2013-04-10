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

        private void changeScreen(UserControl screen)
        {
            this.mainContentControl.Content = screen;
        }

        private void switchDisplay(object sender, RoutedEventArgs e)
        {
            RadioButton button = (RadioButton)sender;
            String value = (String)button.Content.ToString();

            switch (value)
            {
                case "Graph":
                    display = true;

                    if (mode)
                    {
                        changeScreen(new VSGraph());
                    }
                    else
                    {
                        changeScreen(new SMGraph());
                    }
                    break;
                case "Table":
                    display = false;

                    if (mode)
                    {
                        changeScreen(new VSTable());
                    }
                    else
                    {
                        changeScreen(new SMTable());
                    }
                    break;
            }
        }

        private void switchMode(object sender, RoutedEventArgs e)
        {
            RadioButton button = (RadioButton)sender;
            String value = (String)button.Content.ToString();

            switch (value)
            {
                case "VS":
                    mode = false;

                    if (display)
                    {
                        changeScreen(new VSGraph());
                    }
                    else
                    {
                        changeScreen(new VSTable());
                    }
                    break;

                case "SM":
                    display = true;

                    if (display)
                    {
                        changeScreen(new SMGraph());
                    }
                    else
                    {
                        changeScreen(new SMTable());
                    }
                    break;
            }
        }
    }
}
