using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Modes
{
    class ReportConfigurable : Configurable
    {
        private readonly IDataSource _dataSource;

        public ReportConfigurable(string name, BaseMode mode, IDataSource dataSource) : base(name, mode)
        {
            _dataSource = dataSource;
        }

        public override void Delete()
        {
            _dataSource.RemoveConfiguration(Name);   
        }
    }
}