using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.HAL.Common.GUI.Functions;

public struct ButtonAspects : IDisposable
{
    public readonly Button Button;
    private readonly Color RunningColor;
    private readonly Color CompleteColor;

    public void Dispose()
    {
        if (Button == null)
            return;
        Button.BackColor = CompleteColor;
        Application.DoEvents();
    }

    public string GetTagInstruction()
    {
        return Button != null ? Button.Tag as string : string.Empty;
    }

    internal ButtonAspects(Button b, Color running)
    {
        Button = b;
        RunningColor = running;
        if (Button == null)
        {
            CompleteColor = Color.LightGray;
        }
        else
        {
            CompleteColor = Button.BackColor;
            Button.BackColor = RunningColor;
        }

        Application.DoEvents();
    }
}