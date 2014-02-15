using System.Linq;
using System.Text;
using System.Windows;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Modes
{
    public abstract class BaseConfigurable
    {
        public string Name { get; protected set; }
        protected readonly BaseMode Mode;

        protected BaseConfigurable(string name, BaseMode mode)
        {
            Name = name;
            Mode = mode;
            CanExport = true;
        }

        public bool CanExport { get; set; }

        public abstract void Delete();


        public virtual void Export()
        {
            Mode.ExportConfigurable(this);
        }

        public virtual void Edit()
        {
            Mode.EditConfigurable(this);
        }

        public virtual void View()
        {
            Mode.ShowConfigurable(this);
        }
    }
}
