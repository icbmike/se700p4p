using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Modes
{
    public class RedLightRunningConfigurable : Configurable
    {
        private readonly IDataSource _dataSource;

        public RedLightRunningConfigurable(string name, BaseMode mode, IDataSource dataSource) : base(name, mode)
        {
            _dataSource = dataSource;
        }

        public override void Delete()
        {
            _dataSource.RemoveRedLightRunningConfiguration(Name);
        }
    }
}