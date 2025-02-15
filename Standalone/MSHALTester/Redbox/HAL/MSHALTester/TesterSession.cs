using System;
using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterSession : ISessionUser
{
    internal TesterSession(string uname)
    {
        SessionStart = DateTime.Now;
        User = uname;
    }

    public string User { get; }

    public DateTime SessionStart { get; }
}