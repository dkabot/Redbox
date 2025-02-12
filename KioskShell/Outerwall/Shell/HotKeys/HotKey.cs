using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace Outerwall.Shell.HotKeys
{
    internal class HotKey
    {
        private readonly Action _action;

        public HotKey(string name, string sequence, Action action)
            : this(name, ParseModifierKeys(sequence), ParseKeys(sequence), action)
        {
        }

        public HotKey(string name, ModifierKeys modifierKeys, Key key, Action action)
            : this(name, modifierKeys, new Key[1]
            {
                key
            }, action)
        {
        }

        public HotKey(string name, ModifierKeys modifierKeys, IEnumerable<Key> keys, Action action)
        {
            if (modifierKeys == ModifierKeys.None || keys == null || !keys.Any())
                throw new ArgumentException(
                    "The specified hot key is invalid. A combination of modifier keys and keys is required.");
            if (action == null)
                throw new ArgumentNullException(nameof(action),
                    "No action was supplied that this hot key should invoke.");
            Name = name ?? string.Empty;
            ModifierKeys = modifierKeys;
            Keys = new List<Key>(keys);
            Keys.Sort();
            _action = action;
        }

        public string Name { get; }

        public ModifierKeys ModifierKeys { get; }

        public List<Key> Keys { get; }

        internal void Execute()
        {
            _action.BeginInvoke(null, new object());
        }

        public override string ToString()
        {
            return string.Format("{0}: {1} + {2}", Name, ModifierKeys,
                string.Join(",", Keys.ConvertAll(k => k.ToString()).ToArray()));
        }

        private static ModifierKeys ParseModifierKeys(string sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            try
            {
                return (ModifierKeys)Enum.Parse(typeof(ModifierKeys), sequence.Substring(0, sequence.IndexOf('+')));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("The hot key sequence \"{0}\" is invalid.", sequence), ex);
            }
        }

        private static IEnumerable<Key> ParseKeys(string sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));
            try
            {
                return sequence.Substring(sequence.IndexOf('+') + 1).Split(',')
                    .Select(s => (Key)Enum.Parse(typeof(Key), s.Trim())).ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("The hot key sequence \"{0}\" is invalid.", sequence), ex);
            }
        }
    }
}