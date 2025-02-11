using Redbox.Core;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.GetOpts
{
    internal class OptionValueList : LinkedList<OptionValue>
    {
        public void WriteOptions(IConsole console)
        {
            foreach (OptionValue optionValue in (LinkedList<OptionValue>)this)
            {
                console.WriteLine((object)optionValue);
                console.WriteLine();
            }
        }

        public OptionValue GetOptionValue(string name)
        {
            foreach (OptionValue optionValue in (LinkedList<OptionValue>)this)
            {
                if (optionValue.IsValidName(name))
                    return optionValue;
            }
            return (OptionValue)null;
        }

        public ReadOnlyCollection<string> GetRequiredOptions()
        {
            List<string> stringList = new List<string>();
            foreach (OptionValue optionValue in (LinkedList<OptionValue>)this)
            {
                if (optionValue.Required)
                    stringList.Add(optionValue.GetName());
            }
            return stringList.AsReadOnly();
        }
    }
}
