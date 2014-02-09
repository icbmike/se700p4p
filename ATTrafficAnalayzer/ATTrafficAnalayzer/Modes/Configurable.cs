using System.Linq;
using System.Text;
using System.Windows;
using ATTrafficAnalayzer.Models.Settings;

namespace ATTrafficAnalayzer.Modes
{
    public abstract class Configurable
    {
        public string Name { get; protected set; }
        protected readonly BaseMode Mode;

        protected Configurable(string name, BaseMode mode)
        {
            Name = name;
            Mode = mode;
        }

        public abstract void Delete();
        

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
