using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

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
        public virtual List<Configurable> PopulateReportBrowser()
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

        public virtual void EditConfigurable(Configurable configurable)
        {
            //Some views won't even have a configuration view.
            
        }
        public virtual void ShowConfigurable(Configurable configurable)
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

        public  ICommand ModeChange { get; protected set; }

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
        
    }
    
}