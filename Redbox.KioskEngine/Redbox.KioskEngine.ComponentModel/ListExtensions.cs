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
                var source1 = list1.Except(list2);
                var source2 = list2.Except(list1);
                return !source1.Any() && !source2.Any();
            }

            return list1 == null && list2 == null;
        }
    }
}