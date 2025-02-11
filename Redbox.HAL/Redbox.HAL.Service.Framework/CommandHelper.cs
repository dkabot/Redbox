using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;

namespace Redbox.HAL.Service.Framework
{
    public static class CommandHelper
    {
        public static void FormatJob(IExecutionContext context, CommandContext result)
        {
            var messages = result.Messages;
            var objArray = new object[8]
            {
                context.ID,
                context.Label ?? "(not labeled)",
                context.ProgramName,
                context.Priority,
                context.StartTime.HasValue ? context.StartTime.ToString() : (object)"N/A",
                context.GetStatus(),
                null,
                null
            };
            var executionTime = context.Result.ExecutionTime;
            string str1;
            if (!executionTime.HasValue)
            {
                str1 = "Not Started";
            }
            else
            {
                executionTime = context.Result.ExecutionTime;
                str1 = executionTime.ToString();
            }

            objArray[6] = str1;
            objArray[7] = (ConnectionState)(context.IsConnected ? 0 : 1);
            var str2 = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|0|0", objArray);
            messages.Add(str2);
        }

        public static IExecutionContext GetJob(string jobId, ErrorList errors)
        {
            if (string.IsNullOrEmpty(jobId))
            {
                errors.Add(Error.NewError("S001", "The job: parameter is required.",
                    "Correct your command and resubmit."));
                return null;
            }

            var job = ServiceLocator.Instance.GetService<IExecutionService>().GetJob(jobId);
            if (job == null)
                errors.Add(Error.NewError("S001", string.Format("The job for ID '{0}' does not exist.", jobId),
                    "Correct your command and resubmit."));
            return job;
        }
    }
}