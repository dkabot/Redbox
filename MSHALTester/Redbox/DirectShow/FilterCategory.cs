using System;
using System.Runtime.InteropServices;

namespace Redbox.DirectShow;

[ComVisible(false)]
public static class FilterCategory
{
    public static readonly Guid AudioInputDevice = new(869902178U, 37064, 4560, 189, 67, 0, 160, 201, 17, 206, 134);
    public static readonly Guid VideoInputDevice = new(2248913680U, 23809, 4560, 189, 59, 0, 160, 201, 17, 206, 134);

    public static readonly Guid VideoCompressorCategory =
        new(869902176U, 37064, 4560, 189, 67, 0, 160, 201, 17, 206, 134);

    public static readonly Guid AudioCompressorCategory =
        new(869902177U, 37064, 4560, 189, 67, 0, 160, 201, 17, 206, 134);
}