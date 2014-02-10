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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ATTrafficAnalayzer.Annotations;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Models.ReportConfiguration;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for RedLightRunningConfigScreen.xaml
    /// </summary>
    public partial class RedLightRunningConfigScreen
    {
        private readonly IDataSource _dataSource;

        public RedLightRunningConfigScreen(IDataSource dataSource)
        {
            _dataSource = dataSource;
            Configuration = new RedLightRunningConfiguration();
            ReportConfigurations = _dataSource.GetConfigurationNames().Select(name => new ReportConfigSelectedModel {Name = name, Selected = false}).ToList();
            InitializeComponent();
        }

        public RedLightRunningConfiguration Configuration { get; set; } // What we will be saving eventually

        public List<ReportConfigSelectedModel> ReportConfigurations { get; set; }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            
        }

        private void CheckboxHeaderOnChange(object sender, RoutedEventArgs e)
        {
            var isChecked = (sender as CheckBox).IsChecked;
            ReportConfigurations.ForEach(model => model.Selected = isChecked.Value);
            
        }
    }

    public class ReportConfigSelectedModel : INotifyPropertyChanged
    {
        private bool _selected;
        public string Name { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value;OnPropertyChanged("Selected"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
