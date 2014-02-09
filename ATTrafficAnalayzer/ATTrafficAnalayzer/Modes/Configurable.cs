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

        public abstract void Delete(Configurable configurable);
        

        public virtual void Edit(Configurable configurable)
        {
            Mode.EditConfigurable(configurable);
        }

        public virtual void View(Configurable configurable)
        {
            Mode.ShowConfigurable(configurable);
        }
    }
}
