using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public class HardwareCommand(IPCProtocol protocol, string command) :
    ClientCommand<HardwareCommandResult>(protocol, command)
{
}