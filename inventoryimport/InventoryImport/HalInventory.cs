using System;
using System.Collections.Generic;
using Microsoft.Win32;
using Redbox.Core;

namespace InventoryImport
{
    public class HalInventory
    {
        private static readonly HalInventory m_instance = new HalInventory();
        private readonly Dictionary<int, List<string>> m_decks = new Dictionary<int, List<string>>();

        private HalInventory()
        {
        }

        public static HalInventory Instance => m_instance;

        public static void Add(Disc disc)
        {
            var halBarcode = disc.Barcode.ToHalBarcode(disc.DiskType);
            LogHelper.Instance.Log("HAL barcode from legacy disc: {0}", halBarcode);
            int result;
            if (!int.TryParse(disc.Deck, out result))
            {
                LogHelper.Instance.Log("...An invalid deck number was specified: {0}", disc.Deck);
            }
            else
            {
                if (!Instance.m_decks.ContainsKey(result))
                    Instance.m_decks[result] = new List<string>();
                Instance.m_decks[result].Add(halBarcode);
            }
        }

        public static void SetInventory()
        {
            try
            {
                foreach (var deck in Instance.m_decks)
                {
                    var subkey = string.Format("SOFTWARE\\Redbox\\HAL\\State\\Live\\Decks\\{0}", deck.Key);
                    var subKey = Registry.LocalMachine.CreateSubKey(subkey);
                    if (subKey == null)
                    {
                        LogHelper.Instance.Log("...Unable to create registry sub key: {0}", subkey);
                    }
                    else
                    {
                        subKey.SetValue("Slots", deck.Value.ToArray(), RegistryValueKind.MultiString);
                        subKey.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised while creating the HAL inventory registry entries.", ex);
            }
        }
    }
}