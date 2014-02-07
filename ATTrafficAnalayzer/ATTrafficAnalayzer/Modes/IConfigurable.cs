using System.Linq;
using System.Text;
using System.Windows;

namespace ATTrafficAnalayzer.Modes
{
    public interface IConfigurable
    {
        void Delete(string name);
        void Edit(string name);
        void View(string name);
    }
}
