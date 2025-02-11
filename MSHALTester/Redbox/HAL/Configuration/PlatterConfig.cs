using System;

namespace Redbox.HAL.Configuration;

public sealed class PlatterConfig
{
    private const int SellThruSlotWidth = 466;

    private PlatterConfig(PlatterData data)
    {
        Data = data;
        switch (data.SegmentOffsets.Length)
        {
            case 4:
                Type = PlatterType.Qlm;
                break;
            case 6:
                Type = PlatterType.Dense;
                break;
            case 12:
                Type = PlatterType.Sparse;
                break;
            default:
                Type = PlatterType.None;
                break;
        }

        Configure();
    }

    private PlatterConfig(PlatterType t)
    {
        Type = t;
        Configure();
    }

    public PlatterType Type { get; }

    public int SlotsPerQuadrant { get; private set; }

    public int? SellThruOffset { get; private set; }

    public decimal SlotWidth { get; private set; }

    public int QuadrantCount { get; private set; }

    public int SlotCount { get; private set; }

    public PlatterData Data { get; private set; }

    public static PlatterConfig Get(PlatterData data)
    {
        return new PlatterConfig(data);
    }

    public int[] ComputeOffsets(decimal startOffset)
    {
        if (PlatterType.Qlm == Type || Type == PlatterType.None)
            throw new InvalidOperationException(string.Format("Can't compute segments on type {0}", Type));
        var offsets = new int[QuadrantCount];
        var num1 = startOffset;
        for (var index = 0; index < QuadrantCount; ++index)
        {
            offsets[index] = (int)num1;
            var num2 = PlatterType.Sparse != Type
                ? SlotsPerQuadrant * SlotWidth + SlotWidth
                : (SlotsPerQuadrant - 1) * SlotWidth + 0.5M + 466M;
            num1 += num2;
        }

        return offsets;
    }

    private void Configure()
    {
        switch (Type)
        {
            case PlatterType.Sparse:
                SlotWidth = 173.3M;
                SellThruOffset = 915;
                QuadrantCount = 12;
                SlotsPerQuadrant = 6;
                SlotCount = 72;
                break;
            case PlatterType.Dense:
                SlotWidth = 166.6667M;
                QuadrantCount = 6;
                SlotsPerQuadrant = 15;
                SlotCount = 90;
                break;
            case PlatterType.Qlm:
                SlotWidth = 177.7M;
                break;
        }
    }
}