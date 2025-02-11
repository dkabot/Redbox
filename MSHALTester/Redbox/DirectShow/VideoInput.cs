namespace Redbox.DirectShow;

public class VideoInput
{
    internal VideoInput(int index, PhysicalConnectorType type)
    {
        Index = index;
        Type = type;
    }

    public int Index { get; private set; }

    public PhysicalConnectorType Type { get; private set; }

    public static VideoInput Default => new(-1, PhysicalConnectorType.Default);
}