using ATTrafficAnalayzer.Models;

namespace ATTrafficAnalayzer.Modes
{
    public class SummaryConfigurable : BaseConfigurable
    {
        private readonly IDataSource _dataSource;

        public SummaryConfigurable(string name, BaseMode mode, IDataSource dataSource) : base(name, mode)
        {
            _dataSource = dataSource;
        }

        public override void Delete()
        {
            _dataSource.RemoveSummary(Name);   
        }
    }
}