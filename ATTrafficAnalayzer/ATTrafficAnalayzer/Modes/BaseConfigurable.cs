using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
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
            CanExport = false;

            ConfigurableCommand = new ConfigurableCommand(this);
        }

        public bool CanExport { get; set; }

        public abstract void Delete();

        public ICommand ConfigurableCommand { get; set; }

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

    public class ConfigurableCommand : ICommand
    {
        private readonly BaseConfigurable _configurable;

        public ConfigurableCommand(BaseConfigurable configurable)
        {
            _configurable = configurable;
        }

        public void Execute(object parameter)
        {
            var action = parameter as string;
            switch (action)
            {
                case "export":
                    _configurable.Export();
                    break;
                case "edit":
                    _configurable.Edit();
                    break;
                case "delete":
                    _configurable.Delete();
                    break;
            }
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}
