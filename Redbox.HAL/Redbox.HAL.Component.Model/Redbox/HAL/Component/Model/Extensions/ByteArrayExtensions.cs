using System;
using System.Text;

namespace Redbox.HAL.Component.Model.Extensions
{
    public static class ByteArrayExtensions
    {
        public static void Dump(this byte[] array)
        {
            LogHelper.Instance.Log(" -- Byte array dump -- ");
            if (array.Length == 0)
            {
                LogHelper.Instance.Log(" Array is empty");
            }
            else
            {
                var stringBuilder = new StringBuilder();
                var index = 0;
                var num = 0;
                while (index < array.Length)
                {
                    if (16 == num)
                    {
                        num = 0;
                        stringBuilder.AppendLine();
                    }

                    stringBuilder.AppendFormat("{0:x2} ", array[index]);
                    ++index;
                    ++num;
                }

                LogHelper.Instance.Log(stringBuilder.ToString());
            }
        }

        public static string AsString(this byte[] array)
        {
            var hex = new StringBuilder(array.Length * 2);
            Array.ForEach(array, b => hex.AppendFormat("{0:x2} ", b));
            return hex.ToString();
        }
    }
}