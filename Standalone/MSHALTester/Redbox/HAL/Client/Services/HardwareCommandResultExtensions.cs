using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

public static class HardwareCommandResultExtensions
{
    public static void Dump(this HardwareCommandResult result)
    {
        if (!result.Success && result.Errors.Count > 0)
        {
            LogHelper.Instance.Log("\nClient Errors:");
            foreach (var error in result.Errors)
            {
                LogHelper.Instance.Log(error.ToString());
                LogHelper.Instance.Log("Details: {0}\n", error.Details);
            }
        }

        if (result.CommandMessages.Count > 0)
        {
            LogHelper.Instance.Log("Command Messages:");
            for (var index = 0; index < result.CommandMessages.Count; ++index)
                LogHelper.Instance.Log(result.CommandMessages[index]);
        }

        LogHelper.Instance.Log("Command {0}, execution time = {1}",
            result.Success ? "successfully executed" : (object)"failed during execution", result.ExecutionTime);
    }
}