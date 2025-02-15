using System;
using System.ComponentModel;

namespace Redbox.HAL.Management.Console
{
    public class SortableBindingList<T> : BindingList<T>
    {
        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            var num = direction == ListSortDirection.Ascending ? 1 : -1;
            if (prop.PropertyType.GetInterface("IComparable") != null)
                for (var index1 = 1; index1 < Items.Count; ++index1)
                {
                    var flag = true;
                    for (var index2 = 1; index2 < Items.Count; ++index2)
                        if ((prop.GetValue(Items[index2 - 1]) as IComparable).CompareTo(
                                prop.GetValue(Items[index2]) as IComparable) * num > 0)
                        {
                            var obj = Items[index2 - 1];
                            Items[index2 - 1] = Items[index2];
                            Items[index2] = obj;
                            flag = false;
                        }

                    if (flag)
                        break;
                }
            else
                for (var index3 = 1; index3 < Items.Count; ++index3)
                {
                    var flag = true;
                    for (var index4 = 1; index4 < Items.Count; ++index4)
                        if (string.Compare(
                                prop.GetValue(Items[index4 - 1]) != null
                                    ? prop.GetValue(Items[index4 - 1]).ToString()
                                    : string.Empty,
                                prop.GetValue(Items[index4]) != null
                                    ? prop.GetValue(Items[index4]).ToString()
                                    : string.Empty) * num > 0)
                        {
                            var obj = Items[index4 - 1];
                            Items[index4 - 1] = Items[index4];
                            Items[index4] = obj;
                            flag = false;
                        }

                    if (flag)
                        break;
                }
        }
    }
}