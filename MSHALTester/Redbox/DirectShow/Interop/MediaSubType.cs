using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow.Interop;

[ComVisible(false)]
internal static class MediaSubType
{
    public static readonly Guid YUYV = new(1448695129, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid IYUV = new(1448433993, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid DVSD = new(1146312260, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
    public static readonly Guid RGB1 = new(3828804472U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB4 = new(3828804473U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB8 = new(3828804474U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB565 = new(3828804475U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB555 = new(3828804476U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB24 = new(3828804477U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid RGB32 = new(3828804478U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid Avi = new(3828804488U, 21071, 4558, 159, 83, 0, 32, 175, 11, 167, 112);
    public static readonly Guid Asf = new(1035472784U, 37906, 4561, 173, 237, 0, 0, 248, 117, 75, 153);
}