namespace Redbox.HAL.Component.Model;

public struct OffsetMoveData
{
    public int? XOffset { get; private set; }

    public int? YOffset { get; private set; }

    public OffsetMoveData(int? xoff, int? yoff)
        : this()
    {
        XOffset = xoff;
        YOffset = yoff;
    }
}