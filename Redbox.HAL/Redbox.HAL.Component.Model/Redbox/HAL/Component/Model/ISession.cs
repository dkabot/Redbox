using System;

namespace Redbox.HAL.Component.Model
{
    public interface ISession : IMessageSink
    {
        bool LogDetailedMessages { get; }
        void Start();

        event EventHandler Disconnect;
    }
}