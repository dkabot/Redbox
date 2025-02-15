using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console
{
    public class ProgramEvent : INotifyPropertyChanged, IComparable
    {
        private DateTime m_eventTime;
        private string m_message;

        public ProgramEvent(DateTime time, string msg)
        {
            EventTime = time;
            Message = msg;
        }

        public DateTime EventTime
        {
            get => m_eventTime;
            set
            {
                m_eventTime = value;
                NotifyPropertyChanged(nameof(EventTime));
            }
        }

        public string Message
        {
            get => m_message;
            set
            {
                m_message = value;
                NotifyPropertyChanged(nameof(Message));
            }
        }

        public int CompareTo(object p_Target)
        {
            return EventTime.CompareTo((p_Target as ProgramEvent).EventTime);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(ProgramEvent item)
        {
            return item != null && (!(item.EventTime != EventTime) || !(item.Message != Message));
        }

        private void NotifyPropertyChanged(string name)
        {
            if (PropertyChanged == null)
                return;
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}