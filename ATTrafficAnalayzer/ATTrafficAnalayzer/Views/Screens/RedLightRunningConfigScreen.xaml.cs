using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ATTrafficAnalayzer.Annotations;
using ATTrafficAnalayzer.Models;
using ATTrafficAnalayzer.Modes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Interaction logic for RedLightRunningConfigScreen.xaml
    /// </summary>
    public partial class RedLightRunningConfigScreen : INotifyPropertyChanged
    {
        private readonly IDataSource _dataSource;
        private RedLightRunningConfiguration _configuration;
        private bool _isEditing;
        private string _oldName;

        public RedLightRunningConfigScreen(IDataSource dataSource)
        {
            _dataSource = dataSource;
            _configuration = new RedLightRunningConfiguration();
            
            RefreshReportConfigurations(); //Needs a better name
            _isEditing = false;
            _oldName = null;
            InitializeComponent();
        }

        public void RefreshReportConfigurations()
        {
            ReportConfigurations = new ObservableCollection<ReportConfigSelectedModel>(_dataSource.GetConfigurationNames().
                Select(name => _dataSource.GetConfiguration(name)).
                Select(config => new ReportConfigSelectedModel
                {
                    Name = config.Name,
                    Selected = false,
                    Intersection = config.Intersection,
                    Approaches = string.Join(", ", config.Approaches.Select(approach => approach.ApproachName))
                }));
            if(ReportConfigListView != null)
                ReportConfigListView.ItemsSource = ReportConfigurations;
        }

        public RedLightRunningConfiguration Configuration
        {
            get { return _configuration; }
            set { _configuration = value;
                _isEditing = true;
                _oldName = _configuration.Name;
                Render(); 
                OnPropertyChanged("Configuration");
            }
        }

        private void Render()
        {

            RefreshReportConfigurations();
            //We need to restore the selected checkbox
            foreach (var reportConfigSelectedModel in ReportConfigurations)
            {
                reportConfigSelectedModel.Selected = false;
            }
            foreach (var model in Configuration.Sites.SelectMany(configuration => ReportConfigurations
                .Where(model => model.Name.Equals(configuration.Name))))
            {
                model.Selected = true;
            }

        }

        public ObservableCollection<ReportConfigSelectedModel> ReportConfigurations { get; set; }

        private void SaveButtonOnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Configuration.Name))
            {
                MessageBox.Show("You need to give this configuration a name.");
                return;
            }

            Configuration.Sites = ReportConfigurations.Where(model => model.Selected)
                .Select(model => _dataSource.GetConfiguration(model.Name))
                .ToList();

            //If we're editing, we delete the old version
            if(_isEditing)
                _dataSource.RemoveRedLightRunningConfiguration(_oldName);

            _dataSource.SaveRedLightRunningConfiguration(Configuration);

            //Fire the saved event
            if (ConfigurationSaved != null)
                ConfigurationSaved(this, new ConfigurationSavedEventArgs(Configuration.Name, null)); //We don't have a reference to the containing mode right now
        }

        public event ConfigurationSavedEventHandler ConfigurationSaved;

        private void CheckboxHeaderOnChange(object sender, RoutedEventArgs e)
        {
            var isChecked = (sender as CheckBox).IsChecked;
            foreach (var reportConfigSelectedModel in ReportConfigurations)
            {
                reportConfigSelectedModel.Selected = isChecked.Value;
            }
            
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ReportConfigSelectedModel : INotifyPropertyChanged
    {
        private bool _selected;
        public string Name { get; set; }
        public string Approaches { get; set; }

        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; OnPropertyChanged("Selected"); }
        }

        public int Intersection { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
