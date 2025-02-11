using System;
using System.ComponentModel;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

[Command("ipctest")]
public sealed class TestCommand
{
    [CommandForm(Name = "test-ipc-xfer")]
    [Usage("ipctest test-ipc-xfer size: payloadSize")]
    [Description("")]
    public void TestIPCTransfer(CommandContext context,
        [CommandKeyValue(IsRequired = true, KeyName = "size")] int payloadSize)
    {
        var random = new Random((int)DateTime.Now.Ticks);
        var stringBuilder = new StringBuilder();
        for (var index = 0; index < payloadSize; ++index)
        {
            var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26.0 * random.NextDouble() + 65.0)));
            stringBuilder.Append(ch);
        }

        var msg = stringBuilder.ToString();
        LogHelper.Instance.Log("String: ");
        LogHelper.Instance.Log(msg);
        context.Messages.Add(msg);
    }

    [CommandForm(Name = "test-comm")]
    [Usage("ipctest test-comm")]
    [Description("")]
    public void TestIPCTransfer(CommandContext context)
    {
        context.Messages.Add("ACK");
    }
}