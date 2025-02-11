using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.Configuration
{
    public sealed class PlatterData
    {
        internal PlatterData(List<int> data)
            : this(data, true)
        {
        }

        internal PlatterData(List<int> data, bool removeZeroes)
        {
            YOffset = data[0];
            data.RemoveAt(0);
            if (removeZeroes)
                data.RemoveAll(each => each == 0);
            SegmentOffsets = data.ToArray();
        }

        public int YOffset { get; }

        public int[] SegmentOffsets { get; internal set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(YOffset);
            foreach (var segmentOffset in SegmentOffsets)
                stringBuilder.Append(string.Format(",{0}", segmentOffset));
            return stringBuilder.ToString();
        }
    }
}