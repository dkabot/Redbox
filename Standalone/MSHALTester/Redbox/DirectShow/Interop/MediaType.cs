using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal static class MediaType
{
    public static readonly Guid Video = new(1935960438, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid Interleaved = new(1937138025, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid Audio = new(1935963489, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid Text = new(1937012852, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid Stream = new(3828804483U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
}