using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using ATTrafficAnalayzer.Annotations;

namespace ATTrafficAnalayzer.Views.Controls
{
    /// <summary>
    /// Interaction logic for DuplicatePolicyPreferenceDialog.xaml
    /// </summary>
    public partial class DuplicatePolicyPreferenceDialog : INotifyPropertyChanged
    {
        private DefaultDupicatePolicy _defaultDuplicatePolicy;
        public DefaultDupicatePolicy DefaultDuplicatePolicy
        {
            get { return _defaultDuplicatePolicy; }
            set { _defaultDuplicatePolicy = value; OnPropertyChanged("DefaultDuplicatePolicy"); }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum DefaultDupicatePolicy
    {
        Skip,
        Continue,
        Ask
    }
}
