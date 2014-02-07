using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ATTrafficAnalayzer.Modes
{
    public interface IMode
    {
        /// <summary>
        /// Template method to set the contents of the report browser
        /// Return null to indicate that the report browser should not be displayed
        /// </summary>
        /// <returns></returns>
        List<IConfigurable> PopulateReportBrowser();

        /// <summary>
        /// Method to add controls to the toolbar specific to the implementing mode
        /// </summary>
        /// <param name="toolbar"></param>
        void PopulateToolbar(ToolBar toolbar);

        /// <summary>
        /// Button that will be displayed in the toolbar to switch to this mode
        /// </summary>
        Button ModeButton { get; }

    }

    public interface IConfigurable
    {
        void Delete(string name);
        void Edit(string name);
        void View(string name);
    }
}
