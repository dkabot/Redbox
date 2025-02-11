namespace Redbox.HAL.Component.Model;

public sealed class RedboxHardwareErrorThreshold
{
    private readonly int Threshold;

    public RedboxHardwareErrorThreshold(int threshold, int current)
    {
        Threshold = threshold;
        CurrentCount = current;
    }

    public RedboxHardwareErrorThreshold(int threshold)
        : this(threshold, 0)
    {
    }

    public bool Exceeded => CurrentCount >= Threshold;

    public int CurrentCount { get; private set; }

    public int Increment()
    {
        return ++CurrentCount;
    }

    public void Reset()
    {
        CurrentCount = 0;
    }
}