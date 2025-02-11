using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    public sealed class Deck : IDeck
    {
        internal const int DefaultSlot1Offset = 780;
        private const int SlotsPerQuadrantDenseDeck = 15;
        private const int SlotsPerQuadrantSparseDeck = 6;

        internal Deck(
            int number,
            int yoffset,
            bool isQlm,
            int numberOfSlots,
            decimal slotWidth,
            int? sellThruSlots,
            int? sellThruOffset,
            Quadrant[] quadrantOffsets)
        {
            Number = number;
            YOffset = yoffset;
            IsQlm = isQlm;
            SlotWidth = slotWidth;
            SellThruSlots = sellThruSlots;
            SellThruOffset = sellThruOffset;
            NumberOfSlots = numberOfSlots;
            if (quadrantOffsets != null)
                Quadrants.AddRange(quadrantOffsets);
            else
                Quadrants.Add(new Quadrant(780));
            SlotsPerQuadrant = NumberOfSlots / Quadrants.Count;
        }

        public decimal SlotWidth { get; private set; }

        public int? SellThruSlots { get; }

        public int? SellThruOffset { get; private set; }

        public bool IsSlotSellThru(int slot)
        {
            return SellThruSlots.HasValue && slot % SellThruSlots.Value == 0;
        }

        public int GetSlotOffset(int slot)
        {
            if (!IsSlotValid(slot))
                throw new ArgumentException(string.Format("The slot parameter must be between 1 and {0}.",
                    NumberOfSlots));
            var num1 = slot - 1;
            var index = num1 / SlotsPerQuadrant;
            if (index >= Quadrants.Count)
                throw new ArgumentException(string.Format(
                    "The computed quadrant offset {0} is outside the range of defined quadrants.  This usuallys means the deck is incorrectly configured.",
                    index));
            var offset = Quadrants[index].Offset;
            var thatContainsSlot = FindQuadrantThatContainsSlot(slot);
            var num2 = num1 % SlotsPerQuadrant;
            if (thatContainsSlot != null)
            {
                num2 = slot - thatContainsSlot.Slots.Start;
                offset = thatContainsSlot.Offset;
            }

            var num3 = num2 * SlotWidth;
            if (IsSlotSellThru(slot))
                num3 = SellThruOffset ?? 915;
            return (int)(offset + num3);
        }

        public bool IsSlotValid(int slot)
        {
            return slot >= 1 && slot <= NumberOfSlots;
        }

        public bool IsSparse => NumberOfSlots == 72;

        public int Number { get; }

        public int YOffset { get; private set; }

        public bool IsQlm { get; private set; }

        public int NumberOfSlots { get; private set; }

        public int SlotsPerQuadrant { get; }

        public List<IQuadrant> Quadrants { get; } = new List<IQuadrant>();

        public override int GetHashCode()
        {
            return Number;
        }

        public override bool Equals(object obj)
        {
            var deck = obj as Deck;
            return (object)deck != null && deck == this;
        }

        public static bool operator !=(Deck lhs, Deck rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator ==(Deck lhs, Deck rhs)
        {
            if (lhs == (object)rhs)
                return true;
            if ((object)lhs == null || (object)rhs == null || lhs.YOffset != rhs.YOffset ||
                lhs.NumberOfSlots != rhs.NumberOfSlots || lhs.IsQlm != rhs.IsQlm || lhs.SlotWidth != rhs.SlotWidth)
                return false;
            var sellThruOffset1 = lhs.SellThruOffset;
            var sellThruOffset2 = rhs.SellThruOffset;
            if (!((sellThruOffset1.GetValueOrDefault() == sellThruOffset2.GetValueOrDefault()) &
                  (sellThruOffset1.HasValue == sellThruOffset2.HasValue)) || lhs.Quadrants.Count != rhs.Quadrants.Count)
                return false;
            for (var index = 0; index < lhs.Quadrants.Count; ++index)
            {
                var quadrant1 = lhs.Quadrants[index];
                var quadrant2 = rhs.Quadrants[index];
                if (quadrant1.Offset != quadrant2.Offset || quadrant1.IsExcluded != quadrant2.IsExcluded)
                    return false;
            }

            return true;
        }

        internal static Deck FromXmlNode(XmlNode node)
        {
            var attributeValue1 = node.GetAttributeValue("Number", 1);
            var attributeValue2 = node.GetAttributeValue("Offset", -18760);
            var attributeValue3 = node.GetAttributeValue("IsQlm", false);
            var attributeValue4 = node.GetAttributeValue("SlotWidth", 166.6667M);
            var attributeValue5 = node.GetAttributeValue("NumberOfSlots", 90);
            var attributeValue6 = node.GetAttributeValue("SellThruSlots", new int?());
            var attributeValue7 = node.GetAttributeValue("SellThruOffset", new int?());
            var quadrantList = new List<Quadrant>();
            var num1 = 0;
            foreach (XmlNode childNode in node.ChildNodes)
            {
                var attributeValue8 = childNode.GetAttributeValue("Offset", 780);
                var attributeValue9 = childNode.GetAttributeValue("StartSlot", new int?());
                var attributeValue10 = childNode.GetAttributeValue("EndSlot", new int?());
                var attributeValue11 = childNode.GetAttributeValue("IsExcluded", false);
                Range slots;
                if (attributeValue9.HasValue && attributeValue10.HasValue)
                {
                    slots = new Range(attributeValue9.Value, attributeValue10.Value);
                }
                else
                {
                    var num2 = 90 == attributeValue5 ? 15 : 6;
                    var start = num1 * num2 + 1;
                    slots = new Range(start, start + num2 - 1);
                }

                var quadrant = new Quadrant(attributeValue8, slots);
                quadrantList.Add(quadrant);
                quadrant.IsExcluded = attributeValue11;
                ++num1;
            }

            return new Deck(attributeValue1, attributeValue2, attributeValue3, attributeValue5, attributeValue4,
                attributeValue6, attributeValue7, quadrantList.ToArray());
        }

        internal void ToXmlWriter(XmlWriter writer)
        {
            writer.WriteStartElement(nameof(Deck));
            writer.WriteAttributeString("Number", XmlConvert.ToString(Number));
            writer.WriteAttributeString("Offset", XmlConvert.ToString(YOffset));
            writer.WriteAttributeString("IsQlm", XmlConvert.ToString(IsQlm));
            writer.WriteAttributeString("SlotWidth", XmlConvert.ToString(SlotWidth));
            writer.WriteAttributeString("NumberOfSlots", XmlConvert.ToString(NumberOfSlots));
            if (SellThruSlots.HasValue)
                writer.WriteAttributeString("SellThruSlots", XmlConvert.ToString(SellThruSlots.Value));
            var sellThruOffset = SellThruOffset;
            if (sellThruOffset.HasValue)
            {
                var xmlWriter = writer;
                sellThruOffset = SellThruOffset;
                var str = XmlConvert.ToString(sellThruOffset.Value);
                xmlWriter.WriteAttributeString("SellThruOffset", str);
            }

            foreach (var quadrant in Quadrants)
            {
                writer.WriteStartElement("Quadrant");
                writer.WriteAttributeString("Offset", XmlConvert.ToString(quadrant.Offset));
                if (quadrant.Slots != null)
                {
                    writer.WriteAttributeString("StartSlot", XmlConvert.ToString(quadrant.Slots.Start));
                    writer.WriteAttributeString("EndSlot", XmlConvert.ToString(quadrant.Slots.End));
                }

                if (quadrant.IsExcluded)
                    writer.WriteAttributeString("IsExcluded", XmlConvert.ToString(true));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        internal void UpdateFrom(Deck newProperties)
        {
            YOffset = newProperties.YOffset;
            IsQlm = newProperties.IsQlm;
            SlotWidth = newProperties.SlotWidth;
            SellThruOffset = newProperties.SellThruOffset;
            var numberOfSlots = NumberOfSlots;
            NumberOfSlots = newProperties.NumberOfSlots;
            Quadrants.Clear();
            Quadrants.AddRange(newProperties.Quadrants);
        }

        private IQuadrant FindQuadrantThatContainsSlot(int slot)
        {
            foreach (var quadrant in Quadrants)
            {
                if (quadrant.Slots == null)
                    return null;
                if (quadrant.Slots.Includes(slot))
                    return quadrant;
            }

            return null;
        }
    }
}