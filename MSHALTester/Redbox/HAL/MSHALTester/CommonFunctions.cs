using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.MSHALTester;

public static class CommonFunctions
{
    private const int SellThruSlotWidth = 466;

    public static bool ComputeQuadrants(
        decimal? startOffset,
        int? numberOfQuadrants,
        int? sellThruSlots,
        int? sellThruOffset,
        int? slotsPerQuadrant,
        decimal? slotWidth,
        XmlNode deckNode)
    {
        var nullable1 = startOffset;
        var i = 0;
        while (true)
        {
            var num1 = i;
            var nullable2 = numberOfQuadrants;
            var valueOrDefault = nullable2.GetValueOrDefault();
            if ((num1 < valueOrDefault) & nullable2.HasValue)
            {
                deckNode.ChildNodes[i].SetAttributeValue("Offset", (int)nullable1.Value);
                decimal? nullable3;
                var num2 = 0M;
                var num3 = num2;
                nullable3 = num3;
                decimal? nullable4;
                decimal? nullable5;
                decimal? nullable6;
                if (sellThruSlots.HasValue && sellThruOffset.HasValue)
                {
                    nullable2 = slotsPerQuadrant;
                    int? nullable7;
                    int? nullable8;
                    if (!nullable2.HasValue)
                    {
                        nullable7 = new int?();
                        nullable8 = nullable7;
                    }
                    else
                    {
                        nullable8 = nullable2.GetValueOrDefault() - 1;
                    }

                    nullable7 = nullable8;
                    num2 = nullable7.Value;
                    nullable4 = slotWidth;
                    decimal? nullable9;
                    if (!nullable4.HasValue)
                    {
                        nullable5 = new decimal?();
                        nullable9 = nullable5;
                    }
                    else
                    {
                        nullable9 = num2 * nullable4.GetValueOrDefault();
                    }

                    nullable4 = nullable9;
                    num2 = 0.5M;
                    decimal? nullable10;
                    if (!nullable4.HasValue)
                    {
                        nullable5 = new decimal?();
                        nullable10 = nullable5;
                    }
                    else
                    {
                        nullable10 = nullable4.GetValueOrDefault() + num2;
                    }

                    nullable4 = nullable10;
                    num2 = 466;
                    decimal? nullable11;
                    if (!nullable4.HasValue)
                    {
                        nullable5 = new decimal?();
                        nullable11 = nullable5;
                    }
                    else
                    {
                        nullable11 = nullable4.GetValueOrDefault() + num2;
                    }

                    nullable3 = nullable11;
                }
                else
                {
                    num2 = slotsPerQuadrant.Value;
                    nullable6 = slotWidth;
                    nullable4 = nullable6.HasValue ? num2 * nullable6.GetValueOrDefault() : new decimal?();
                    nullable5 = slotWidth;
                    decimal? nullable12;
                    if (!(nullable4.HasValue & nullable5.HasValue))
                    {
                        nullable6 = new decimal?();
                        nullable12 = nullable6;
                    }
                    else
                    {
                        nullable12 = nullable4.GetValueOrDefault() + nullable5.GetValueOrDefault();
                    }

                    nullable3 = nullable12;
                }

                nullable5 = nullable1;
                nullable4 = nullable3;
                decimal? nullable13;
                if (!(nullable5.HasValue & nullable4.HasValue))
                {
                    nullable6 = new decimal?();
                    nullable13 = nullable6;
                }
                else
                {
                    nullable13 = nullable5.GetValueOrDefault() + nullable4.GetValueOrDefault();
                }

                nullable1 = nullable13;
                ++i;
            }
            else
            {
                break;
            }
        }

        return true;
    }
}