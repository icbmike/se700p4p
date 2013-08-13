using System.Windows.Controls;
using ATTrafficAnalayzer.Views.Controls;

namespace ATTrafficAnalayzer.Views.Screens
{
    public delegate void VolumeAndDateCountsDontMatchHandler(IView sender);

    public interface IView
    {
        void DateRangeChangedHandler(object sender, Toolbar.DateRangeChangedEventHandlerArgs args);
        void ReportChangedHandler(object sender, ReportBrowser.SelectedReporChangeEventHandlerArgs args);

        event VolumeAndDateCountsDontMatchHandler VolumeDateCountsDontMatch;
    }
}
