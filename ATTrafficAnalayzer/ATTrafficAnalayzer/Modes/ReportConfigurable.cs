using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Modes
{
    class ReportConfigurable : BaseConfigurable
    {
        private readonly IDataSource _dataSource;

        public ReportConfigurable(string name, BaseMode mode, IDataSource dataSource) : base(name, mode)
        {
            _dataSource = dataSource;
            CanExport = true;
        }

        public override void Delete()
        {
            _dataSource.RemoveConfiguration(Name);   
        }
    }
}