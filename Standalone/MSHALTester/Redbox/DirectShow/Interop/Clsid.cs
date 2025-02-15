using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal static class Clsid
{
    public static readonly Guid SystemDeviceEnum = new(1656642832, 24811, 4560, 189, 59, 0, 160, 201, 17, 206, 134);
    public static readonly Guid FilterGraph = new(3828804531U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid SampleGrabber = new(3253993632U, 16136, 4563, 159, 11, 0, 96, 8, 3, 158, 55);
    public static readonly Guid CaptureGraphBuilder2 = new(3213342433U, 35879, 4560, 179, 240, 0, 170, 0, 55, 97, 197);
    public static readonly Guid AsyncReader = new(3828804533U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
}