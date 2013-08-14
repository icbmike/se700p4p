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
using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for DuplicatePolicyDialog.xaml
    /// </summary>
    public partial class DuplicatePolicyDialog : Window
    {
        public DbHelper.DuplicatePolicy SelectedPolicy { get; set; }

        public DuplicatePolicyDialog()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender == SkipButton )
            {
                SelectedPolicy = DbHelper.DuplicatePolicy.Skip;
            }else if (sender == SkipAllButton)
            {
                SelectedPolicy = DbHelper.DuplicatePolicy.SkipAll;
            }
            else
            {
                SelectedPolicy = DbHelper.DuplicatePolicy.Continue;
            }
            Close();
        }
    }
}
