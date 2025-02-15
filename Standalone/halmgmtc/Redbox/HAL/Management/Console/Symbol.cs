using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console
{
    public class Symbol : INotifyPropertyChanged, IComparable
    {
        private string m_name;
        private string m_value;

        public Symbol(string Name, string value)
        {
            this.Name = Name;
            Value = value;
        }

        public string Name
        {
            get => m_name;
            set
            {
                m_name = value;
                NotifyPropertyChanged(nameof(Name));
            }
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
            return Name.CompareTo((p_Target as Symbol).Name);
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