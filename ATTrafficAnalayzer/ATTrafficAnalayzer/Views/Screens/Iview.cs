using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    /// <summary>
    /// Delegate that lets subscribers know when volume counts and date counts dont match.
    /// </summary>
    /// <param name="sender">The IView that tried to display volume data for some date range</param>
    public delegate void VolumeAndDateCountsDontMatchHandler(IView sender);

    /// <summary>
    /// Interface that lets implementors respond to changes in the ReportBrowser and Toolbar components. 
    /// Also lets other componenets know when volume and date counts don't match.
    /// </summary>
    public interface IView
    {
        void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args);
        void ReportChangedHandler(object sender, ReportBrowser.SelectedReportChangeEventHandlerArgs args);
        

        event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
