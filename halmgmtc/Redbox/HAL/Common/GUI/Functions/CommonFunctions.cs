using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions
{
    public static class CommonFunctions
    {
        private const int SellThruSlotWidth = 466;

        public static ClientHelper Helper { get; private set; }

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
                if ((num1 < valueOrDefault ? nullable2.HasValue ? 1 : 0 : 0) != 0)
                {
                    deckNode.ChildNodes[i].SetAttributeValue("Offset", (int)nullable1.Value);
                    decimal? nullable3;
                    var num2 = 0M;
                    var num3 = num2;
                    decimal? nullable4;
                    decimal? nullable5;
                    decimal? nullable6;
                    if (sellThruSlots.HasValue && sellThruOffset.HasValue)
                    {
                        var nullable7 = slotsPerQuadrant;
                        var num4 = 1;
                        int? nullable8;
                        if (!nullable7.HasValue)
                        {
                            nullable2 = new int?();
                            nullable8 = nullable2;
                        }
                        else
                        {
                            nullable8 = nullable7.GetValueOrDefault() - num4;
                        }

                        nullable2 = nullable8;
                        num2 = nullable2.Value;
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

        public static HardwareJob ExecuteInstruction(HardwareService service, string instruction)
        {
            var job = (HardwareJob)null;
            var result = service.ExecuteImmediate(instruction, out job);
            DumpResult(result);
            return !result.Success ? null : job;
        }

        public static HardwareJob ExecuteInstruction(
            HardwareService service,
            string instruction,
            int timeout)
        {
            var job = (HardwareJob)null;
            var result = service.ExecuteImmediate(instruction, timeout, out job);
            DumpResult(result);
            return !result.Success ? null : job;
        }

        public static void DumpResult(HardwareCommandResult result)
        {
            if (!result.Success && result.Errors.Count > 0)
            {
                LogHelper.Instance.Log("\nClient Errors:");
                foreach (var error in result.Errors)
                {
                    LogHelper.Instance.Log(error.ToString());
                    LogHelper.Instance.Log("Details: {0}\n", error.Details);
                }
            }

            if (result.CommandMessages.Count > 0)
            {
                LogHelper.Instance.Log("Command Messages:");
                for (var index = 0; index < result.CommandMessages.Count; ++index)
                    LogHelper.Instance.Log(result.CommandMessages[index]);
            }

            LogHelper.Instance.Log("Command {0}, execution time = {1}",
                result.Success ? "successfully executed" : (object)"failed during execution", result.ExecutionTime);
        }
    }
}