using Redbox.Macros;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment
{
    internal class MacroService : IMacroService
    {
        private readonly PropertyDictionary m_properties = new PropertyDictionary();

        public string this[string name]
        {
            get => this.m_properties[name];
            set => this.m_properties[name] = value;
        }

        public string ExpandProperties(string input)
        {
            return this.m_properties.ExpandProperties(input, Location.UnknownLocation);
        }
    }
}
