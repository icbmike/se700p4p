using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Modes
{
    public abstract class BaseMode
    {
        /// <summary>
        /// Base constructor creates the toolbar button and sets it's click 
        /// action to execute the passed in action.
        /// </summary>
        /// <param name="action"></param>
        protected BaseMode(Action<BaseMode> action)
        {
            ModeChange = new ModeChangeCommand(action, this);
        }

        /// <summary>
        /// Template method to set the contents of the report browser
        /// Return null to indicate that the report browser should not be displayed
        /// </summary>
        /// <returns></returns>
        public abstract List<IConfigurable> PopulateReportBrowser();

        /// <summary>
        /// Method to add controls to the toolbar specific to the implementing mode
        /// </summary>
        /// <param name="toolbar"></param>
        public abstract void PopulateToolbar(ToolBar toolbar);

        public abstract UserControl GetView();
        public abstract UserControl GetConfigurationView();


        public event EventHandler ConfigurationCreated;

        protected virtual void OnConfigurationCreated()
        {
            var handler = ConfigurationCreated;
            if (handler != null) handler(this, EventArgs.Empty);
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