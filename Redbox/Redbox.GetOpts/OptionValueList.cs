using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;

namespace Redbox.GetOpts
{
    public class OptionValueList : LinkedList<OptionValue>
    {
        public void WriteOptions(IConsole console)
        {
            foreach (var optionValue in this)
            {
                console.WriteLine(optionValue);
                console.WriteLine();
            }
        }

        public OptionValue GetOptionValue(string name)
        {
            foreach (var optionValue in this)
                if (optionValue.IsValidName(name))
                    return optionValue;
            return null;
        }

        public ReadOnlyCollection<string> GetRequiredOptions()
        {
            var stringList = new List<string>();
            foreach (var optionValue in this)
                if (optionValue.Required)
                    stringList.Add(optionValue.GetName());
            return stringList.AsReadOnly();
        }
    }
}