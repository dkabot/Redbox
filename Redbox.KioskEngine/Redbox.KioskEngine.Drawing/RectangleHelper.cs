using System.Collections.Generic;
using System.Drawing;

namespace Redbox.KioskEngine.Drawing
{
    public static class RectangleHelper
    {
        public static List<Rectangle> Merged(this IEnumerable<Rectangle> rects)
        {
            var rectangleList1 = new List<Rectangle>();
            var rectangleList2 = new List<Rectangle>(rects);
            if (rectangleList2.Count == 0)
                return rectangleList1;
            rectangleList2.Sort((lhs, rhs) =>
            {
                var num1 = lhs.Left.CompareTo(rhs.Left);
                var num2 = lhs.Top.CompareTo(rhs.Top);
                return num1 <= num2 ? num1 : num2;
            });
            var a = rectangleList2[0];
            for (var index = 1; index < rectangleList2.Count; ++index)
            {
                var rectangle = rectangleList2[index];
                if (a.IntersectsWith(rectangle))
                {
                    a = Rectangle.Union(a, rectangle);
                }
                else
                {
                    rectangleList1.Add(a);
                    a = rectangle;
                }
            }

            rectangleList1.Add(a);
            return rectangleList1;
        }
    }
}