using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console
{
    public class StringWrapper : INotifyPropertyChanged, IComparable
    {
        private string m_value;

        public StringWrapper(string value)
        {
            Value = value;
        }

        public string Value
        {
            get => m_value;
            set
            {
                m_value = value;
                NotifyPropertyChanged(nameof(Value));
            }
        }

        public int CompareTo(object p_Target)
        {
            return Value.CompareTo((p_Target as StringWrapper).Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}