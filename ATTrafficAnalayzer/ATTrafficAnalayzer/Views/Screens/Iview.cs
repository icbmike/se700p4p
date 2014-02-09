using System;
using ATTrafficAnalayzer.Models.Settings;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{

    /// <summary>
    /// Interface that lets implementors respond to changes in the ReportBrowser and Toolbar components. 
    /// Also lets other componenets know when volume and date counts don't match.
    /// </summary>
    public interface IView
    {
        void DateSettingsChanged(DateSettings newDateSettings);
        
        event EventHandler VolumeDateCountsDontMatch;
    }
}
