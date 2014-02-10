namespace ATTrafficAnalayzer.Modes
{

    public delegate void ConfigurationSavedEventHandler(object sender, ConfigurationSavedEventArgs args);

    /// <summary>
    /// Event args for a ConfigurationSaved event. args have only a name : String.
    /// </summary>
    public class ConfigurationSavedEventArgs
    {
        public string Name { get; set; }
        public BaseMode Mode { get; set; }

        public ConfigurationSavedEventArgs(string name, BaseMode mode)
        {
            Name = name;
            Mode = mode;
        }
    }
}