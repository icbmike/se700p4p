using System;
using System.Windows.Input;

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
            CanExport = false;
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
