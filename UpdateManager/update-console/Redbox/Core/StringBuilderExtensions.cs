using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core
{
    internal static class StringBuilderExtensions
    {
        public static void ToLines(this StringBuilder builder, List<string> lines)
        {
            string str = builder.ToString();
            int startIndex = 0;
            for (int index = str.IndexOf(Environment.NewLine); index != -1; index = str.IndexOf(Environment.NewLine, startIndex))
            {
                lines.Add(str.Substring(startIndex, index - startIndex));
                startIndex = index + Environment.NewLine.Length;
            }
        }
    }
}
