using System;

namespace Redbox.HAL.Management.Console
{
    public class CommandEnteredEventArgs : EventArgs
    {
        public CommandEnteredEventArgs(string command)
        {
            Command = command;
        }

        public string Command { get; private set; }
    }
}