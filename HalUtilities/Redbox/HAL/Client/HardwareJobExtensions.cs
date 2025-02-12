using System.Collections.Generic;

namespace Redbox.HAL.Client
{
  public static class HardwareJobExtensions
  {
    public static string[] GetStackEntries(this HardwareJob job, int depth)
    {
      return HardwareJobExtensions.GetStackEntriesInner(job, depth);
    }

    public static string GetTopOfStack(this HardwareJob job)
    {
      return HardwareJobExtensions.GetStackEntriesInner(job, 1)[0];
    }

    private static string[] GetStackEntriesInner(HardwareJob job, int depth)
    {
      string[] stackEntriesInner = new string[depth];
      for (int index = 0; index < depth; ++index)
        stackEntriesInner[index] = string.Empty;
      Stack<string> stack;
      if (job.GetStack(out stack).Success && stack.Count >= depth)
      {
        for (int index = 0; index < depth; ++index)
          stackEntriesInner[index] = stack.Pop();
      }
      return stackEntriesInner;
    }
  }
}
