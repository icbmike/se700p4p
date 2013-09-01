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
using System.Windows.Shapes;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for DuplicatePolicyPreferenceDialog.xaml
    /// </summary>
    public partial class DuplicatePolicyPreferenceDialog : Window
    {
        private DefaultDupicatePolicy CurrentOption { get; set; }

        public DuplicatePolicyPreferenceDialog()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            //Save preference
            

            //Exit the window
            Close();
        }
    }

    enum DefaultDupicatePolicy
    {
        Skip,
        Continue,
        Ask
    }
}
