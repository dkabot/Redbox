using System.Collections.Generic;
using Redbox.HAL.Client;

namespace HALUtilities
{
    internal class CounterHelper
    {
        private const string DuplicateCounterName = "DUPLICATE-COUNT";

        internal bool ResetDuplicateCounterValue(HardwareService service)
        {
            ResetCounter(service, "DUPLICATE-COUNT");
            return true;
        }

        internal bool GetDuplicateCounterValue(HardwareService service, out int val)
        {
            return GetCounterValue(service, "DUPLICATE-COUNT", out val);
        }

        internal bool GetCounterValue(HardwareService service, string name, out int v)
        {
            v = 0;
            HardwareJob job;
            if (service == null || !service.ExecuteImmediate(string.Format("COUNTER NAME=\"{0}\" GET ", name), out job)
                    .Success)
                return false;
            var stack = GetStack(service, job);
            if (stack.Count > 0 && stack.Peek() == "SUCCESS")
            {
                stack.Pop();
                v = int.Parse(stack.Peek());
            }

            service.ExecuteImmediate("CLEAR", out job);
            return true;
        }

        internal void ResetCounter(HardwareService service, string name)
        {
            service?.ExecuteImmediate(string.Format("COUNTER NAME=\"{0}\" RESET", name), out var _);
        }

        private Stack<string> GetStack(HardwareService service, HardwareJob job)
        {
            if (service == null)
                return new Stack<string>();
            Stack<string> stack;
            return !job.GetStack(out stack).Success ? new Stack<string>() : stack;
        }
    }
}