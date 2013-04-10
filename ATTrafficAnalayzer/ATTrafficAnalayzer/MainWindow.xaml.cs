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
        enum modes { vs, sm };             // sm when true, vs when false
        enum displays { graph, table };    // graph when true, table when false

        modes mode;
        displays display;

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
                    display = displays.graph;
                    if (mode == modes.sm)
                    {
                        changeScreen(new SMGraph());
                    }
                    else if (mode == modes.vs)
                    {
                        changeScreen(new VSGraph());
                    }
                    break;

                case "Table":
                    display = displays.table;
                    if (mode == modes.sm)
                    {
                        changeScreen(new SMTable());
                    }
                    else if (mode == modes.vs)
                    {
                        changeScreen(new VSTable());
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
                    mode = modes.vs;
                    if (display == displays.graph)
                    {
                        changeScreen(new VSGraph());
                    }
                    else if (display == displays.table)
                    {
                        changeScreen(new VSTable());
                    }
                    break;

                case "SM":
                    mode = modes.sm;
                    if (display == displays.graph)
                    {
                        changeScreen(new SMGraph());
                    }
                    else if (display == displays.table)
                    {
                        changeScreen(new SMTable());
                    }
                    break;
            }
        }
    }
}
