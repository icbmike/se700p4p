using System.Windows;
using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for DuplicatePolicyDialog.xaml
    /// </summary>
    public partial class DuplicatePolicyDialog : Window
    {
        public DuplicatePolicy SelectedPolicy { get; set; }

        public DuplicatePolicyDialog()
        {
            InitializeComponent();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender == SkipButton )
            {
                SelectedPolicy = DuplicatePolicy.Skip;
            }else if (sender == SkipAllButton)
            {
                SelectedPolicy = DuplicatePolicy.SkipAll;
            }
            else
            {
                SelectedPolicy = DuplicatePolicy.Continue;
            }
            Close();
        }
    }
}
