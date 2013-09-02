using System;
using System.ComponentModel;
using System.Windows;
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

            DefaultDupicatePolicy defaultDuplicatePolicy;
            Enum.TryParse(Properties.Settings.Default.DefaultDuplicatePolicy, out defaultDuplicatePolicy);
            DefaultDuplicatePolicy = defaultDuplicatePolicy;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            //Save preference
            Properties.Settings.Default.DefaultDuplicatePolicy = DefaultDuplicatePolicy.ToString();
            Properties.Settings.Default.Save();

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
