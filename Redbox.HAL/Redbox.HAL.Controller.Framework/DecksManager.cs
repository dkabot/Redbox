using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework
{
    internal sealed class DecksManager : IDecksService
    {
        private readonly List<IDeck> Decks = new List<IDeck>();

        public IDeck GetByNumber(int number)
        {
            return Decks.Find(each => each.Number == number);
        }

        public IDeck GetFrom(ILocation location)
        {
            return Decks.Find(each => each.Number == location.Deck);
        }

        public bool IsValidLocation(ILocation loc)
        {
            var byNumber = GetByNumber(loc.Deck) as Deck;
            return !(byNumber == null) && byNumber.IsSlotValid(loc.Slot);
        }

        public void Add(IDeck deck)
        {
            if (deck.IsQlm && QlmDeck != null)
                throw new ArgumentException("Only one QLM deck is allowed per configuration.");
            Decks.Add(deck);
        }

        public void ForAllDecksDo(Predicate<IDeck> predicate)
        {
            foreach (Deck deck in Decks)
                if (!predicate(deck))
                    break;
        }

        public void ForAllReverseDecksDo(Predicate<IDeck> predicate)
        {
            for (var count = Decks.Count; count >= 1; --count)
            {
                var deck = Decks[count - 1];
                if (!predicate(deck))
                    break;
            }
        }

        public IDeck First => Decks[0];

        public IDeck Last => Decks[Decks.Count - 1];

        public int DeckCount => Decks.Count;

        public IDeck QlmDeck
        {
            get
            {
                var qlmDeck = (IDeck)null;
                ForAllDecksDo(d =>
                {
                    if (!d.IsQlm)
                        return true;
                    qlmDeck = d;
                    return false;
                });
                return qlmDeck;
            }
        }

        internal void Clear()
        {
            Decks.Clear();
        }

        internal void Initialize(XmlNode decksNode)
        {
            Decks.Clear();
            foreach (XmlNode childNode in decksNode.ChildNodes)
                Decks.Add(Deck.FromXmlNode(childNode));
            Decks.Sort((x, y) => x.Number.CompareTo(y.Number));
            LogHelper.Instance.Log("[DecksManager] Kiosk configured for {0} decks.", Decks.Count);
        }

        internal void SaveConfiguration(XmlDocument document)
        {
            using (var stringWriter = new StringWriter())
            {
                var writer = new XmlTextWriter(stringWriter)
                {
                    Formatting = Formatting.Indented
                };
                foreach (var deck in Decks)
                    (deck as Deck).ToXmlWriter(writer);
                writer.Flush();
                document.DocumentElement.SelectSingleNodeAndSetInnerXml("Controller/Decks", stringWriter);
            }

            LogHelper.Instance.Log("Completed decks save.", LogEntryType.Info);
        }

        internal void ToPropertyXml(XmlWriter writer)
        {
            foreach (Deck deck in Decks)
                deck.ToXmlWriter(writer);
        }

        internal void UpdateFromPropertyXml(XmlNode propertyNode)
        {
            var xmlNodeList = propertyNode.SelectNodes("Deck");
            if (xmlNodeList == null)
                return;
            var list = new List<Deck>();
            using (new DisposeableList<Deck>(list))
            {
                if (xmlNodeList.Count != Decks.Count)
                {
                    LogHelper.Instance.Log("The deck configuration has changed in count; this is unsupported.");
                }
                else
                {
                    foreach (XmlNode node in xmlNodeList)
                    {
                        var deck = Deck.FromXmlNode(node);
                        var byNumber = GetByNumber(deck.Number) as Deck;
                        if (deck != byNumber)
                            list.Add(deck);
                    }

                    if (list.Count == 0)
                        LogHelper.Instance.Log("Update decks: there were no changes.");
                    else
                        try
                        {
                            LogHelper.Instance.Log("Update decks info:");
                            foreach (var newProperties in list)
                            {
                                var byNumber = GetByNumber(newProperties.Number) as Deck;
                                LogHelper.Instance.Log(" Deck Number {0} changes", byNumber.Number);
                                LogHelper.Instance.Log("  Y-offset: new = {0} old = {1}", newProperties.YOffset,
                                    byNumber.YOffset);
                                if (newProperties.Quadrants.Count == byNumber.Quadrants.Count)
                                {
                                    for (var index = 0; index < newProperties.Quadrants.Count; ++index)
                                        LogHelper.Instance.Log(
                                            "   Quadrant {0} offsets: new = {1} ( excluded = {2} ) old = {3} ( excluded = {4} )",
                                            index + 1, newProperties.Quadrants[index].Offset,
                                            newProperties.Quadrants[index].IsExcluded, byNumber.Quadrants[index].Offset,
                                            byNumber.Quadrants[index].IsExcluded);
                                    byNumber.UpdateFrom(newProperties);
                                }
                                else
                                {
                                    LogHelper.Instance.Log(LogEntryType.Error,
                                        "[DecksManager] Unsupported configuration change: slot count ( old count = {0} new count = {1}",
                                        byNumber.NumberOfSlots, newProperties.NumberOfSlots);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log("There was an exception during deck update.", ex);
                        }
                }
            }
        }
    }
}