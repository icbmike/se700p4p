using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views;
using ATTrafficAnalayzer.Views.Controls;
using Microsoft.Win32;

namespace ATTrafficAnalayzer.Modes
{
    public abstract class BaseMode
    {
        protected readonly DateSettings DateSettings;

        /// <summary>
        /// Base constructor creates the toolbar button and sets it's click 
        /// action to execute the passed in action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="dateSettings"></param>
        protected BaseMode(Action<BaseMode> action, DateSettings dateSettings)
        {
            DateSettings = dateSettings;
            ModeChange = new ModeChangeCommand(action, this);
        }

        /// <summary>
        /// Template method to set the contents of the report browser
        /// Return null to indicate that the report browser should not be displayed
        /// </summary>
        /// <returns></returns>
        public virtual List<BaseConfigurable> PopulateReportBrowser()
        {
            //Default implementation won't populate the report browser
            return null;
        }

        /// <summary>
        /// Method to add controls to the toolbar specific to the implementing mode
        /// </summary>
        /// <param name="toolbar"></param>
        public virtual void PopulateToolbar(ToolBar toolbar)
        {
            //Do nothing
        }

        public virtual void DateRangeChangedEventHandler(object sender, DateRangeChangedEventArgs args)
        {
            //Don't even care G
        }

        /// <summary>
        /// All modes should have a view
        /// </summary>
        /// <returns></returns>
        public abstract UserControl GetView();

        public virtual void ShowConfigurationView()
        {
            //Some views won't even have a configuration view.
        }

        public virtual void EditConfigurable(BaseConfigurable configurable)
        {
            //Some views won't even have a configuration view.
        }

        public virtual void ShowConfigurable(BaseConfigurable configurable)
        {
            //Default, don't do anything
        }

        public event ConfigurationSavedEventHandler ConfigurationSaved;

        protected virtual void OnConfigurationSaved(ConfigurationSavedEventArgs args)
        {
            var handler = ConfigurationSaved;
            if (handler != null) handler(this, args);
        }

        public abstract ImageSource Image { get; protected set; }
        public abstract String ModeName { get; protected set; }

        public ICommand ModeChange { get; protected set; }

        public class ModeChangeCommand : ICommand
        {
            private readonly Action<BaseMode> _a;
            private readonly BaseMode _b;

            public ModeChangeCommand(Action<BaseMode> a, BaseMode b)
            {
                _a = a;
                _b = b;
            }

            public void Execute(object parameter)
            {
                _a(_b);
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }

        public void ExportConfigurable(BaseConfigurable baseConfigurable)
        {
            var dlg = new SaveFileDialog
            {
                FileName = "",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                DefaultExt = ".csv",
                Filter = "CSV Files (.csv)|*.csv"
            };

            if (dlg.ShowDialog() == true) //They clicked okay
            {
                WriteToFile(GetExportLines(), dlg.FileName);
            }
        }

        protected abstract IEnumerable<string> GetExportLines();

        /// <summary>
        /// private method to write the prepared lines to the filename specified.
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="filename"></param>
        private static void WriteToFile(IEnumerable<string> lines, string filename)
        {
            while (true)
            {
                try
                {
                    File.WriteAllLines(filename, lines);
                    System.Windows.Forms.MessageBox.Show("Successfully exported");
                    break;
                }
                catch (IOException)
                {
                    var messageBoxText = "Cannot write to file. Please ensure the file is not open. Try again?";
                    const string caption = "Export failed";
                    const MessageBoxButton button = MessageBoxButton.OKCancel;
                    const MessageBoxImage icon = MessageBoxImage.Error;

                    var result = MessageBox.Show(messageBoxText, caption, button, icon);

                    if (!result.Equals(MessageBoxResult.OK))  //They don't want to try again
                        break;
                }
            }
        }
    }
}