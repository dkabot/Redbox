using System.Collections.Generic;
using System.Linq;

namespace Redbox.KioskEngine.ComponentModel
{
  public static class ListExtensions
  {
    public static bool IsEqualTo<T>(this List<T> list1, List<T> list2)
    {
      if (list1 != null && list2 != null)
      {
        if (list1.Count != list2.Count)
          return false;
        IEnumerable<T> source1 = list1.Except<T>((IEnumerable<T>) list2);
        IEnumerable<T> source2 = list2.Except<T>((IEnumerable<T>) list1);
        return !source1.Any<T>() && !source2.Any<T>();
      }
      return list1 == null && list2 == null;
    }
  }
}
