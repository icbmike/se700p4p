namespace ATTrafficAnalayzer.Views.Screens
{

    /// <summary>
    /// Delegate to handle when a configuration of some kind is saved
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    public delegate void ConfigurationSavedEventHander(object sender, ConfigurationSavedEventArgs args);
    
    /// <summary>
    /// Interface that lets implementors respond to ConfigurationSaved events.
    /// </summary>
    interface IConfigScreen
    {
        event ConfigurationSavedEventHander ConfigurationSaved;
    }

    /// <summary>
    /// Event args for a ConfigurationSaved event. args have only a name : String.
    /// </summary>
    public class ConfigurationSavedEventArgs
    {
        public string Name { get; set; }

        public ConfigurationSavedEventArgs(string name)
        {
            Name = name;
        }
    }
}
