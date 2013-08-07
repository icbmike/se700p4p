namespace ATTrafficAnalayzer.Views.Screens
{
    public delegate void ConfigurationSavedEventHander(object sender, ConfigurationSavedEventArgs args);
    interface IConfigScreen
    {
        event ConfigurationSavedEventHander ConfigurationSaved;
    }

    public class ConfigurationSavedEventArgs
    {
        public string Name { get; set; }

        public ConfigurationSavedEventArgs(string name)
        {
            Name = name;
        }
    }
}
