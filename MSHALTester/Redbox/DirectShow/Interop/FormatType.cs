using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal static class FormatType
{
    public static readonly Guid VideoInfo = new(89694080U, 50006, 4558, 191, 1, 0, 170, 0, 85, 89, 90);
    public static readonly Guid VideoInfo2 = new(4146755232U, 60170, 4560, 172, 228, 0, 0, 192, 204, 22, 186);
}