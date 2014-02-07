using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        protected BaseMode(Action action)
        {
            ModeButton = new ToolbarButton();
            ModeButton.Click += (sender, args) => action();
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
        /// <summary>
        /// Button that will be displayed in the toolbar to switch to this mode
        /// </summary>
        public abstract Button ModeButton { get; protected set; }

    }

    public interface IConfigurable
    {
        void Delete(string name);
        void Edit(string name);
        void View(string name);
    }
}
