using Redbox.HAL.Common.GUI.Functions;

namespace Redbox.HAL.MSHALTester;

internal sealed class TesterSessionImplemtation : ISessionUserService
{
    private readonly ISessionUser CurrentSession;

    internal TesterSessionImplemtation(ISessionUser user)
    {
        CurrentSession = user;
    }

    public ISessionUser GetCurrentSession()
    {
        return CurrentSession;
    }
}