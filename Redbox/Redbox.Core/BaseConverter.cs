using System;
using System.Text;

namespace Redbox.Core
{
    public class BaseConverter
    {
        private readonly string _fromNumberingScheme;
        private readonly int _maxFromSchemeCharacter;
        private readonly string _toNumberingScheme;

        private BaseConverter(
            int fromRadix,
            NumberingSchemes fromScheme,
            int toRadix,
            NumberingSchemes toScheme)
        {
            if (fromRadix < 2 || fromRadix > 36)
                throw new ArgumentOutOfRangeException(nameof(fromRadix), "Radix can be from 2 to 36 inclusive");
            if (toRadix < 2 || toRadix > 36)
                throw new ArgumentOutOfRangeException(nameof(toRadix), "Radix can be from 2 to 36 inclusive");
            if (fromRadix > 26 && fromScheme == NumberingSchemes.AToZ)
                throw new ArgumentOutOfRangeException(nameof(fromRadix),
                    "Invalid numbering scheme for specified number base");
            if (toRadix > 26 && fromScheme == NumberingSchemes.AToZ)
                throw new ArgumentOutOfRangeException(nameof(toRadix),
                    "Invalid numbering scheme for specified number base");
            From = fromRadix;
            _fromNumberingScheme = GetCharactersForNumberingScheme(fromScheme);
            To = toRadix;
            _toNumberingScheme = GetCharactersForNumberingScheme(toScheme);
            _maxFromSchemeCharacter = fromScheme == NumberingSchemes.ZeroToZ ? fromRadix : fromRadix + 1;
        }

        public static BaseConverter BinToOct { get; } =
            new BaseConverter(2, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

        public static BaseConverter BinToDec { get; } =
            new BaseConverter(2, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

        public static BaseConverter BinToHex { get; } =
            new BaseConverter(2, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

        public static BaseConverter OctToBin { get; } =
            new BaseConverter(8, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

        public static BaseConverter OctToDec { get; } =
            new BaseConverter(8, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

        public static BaseConverter OctToHex { get; } =
            new BaseConverter(8, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

        public static BaseConverter DecToBin { get; } =
            new BaseConverter(10, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

        public static BaseConverter DecToOct { get; } =
            new BaseConverter(10, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

        public static BaseConverter DecToHex { get; } =
            new BaseConverter(10, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);

        public static BaseConverter HexToBin { get; } =
            new BaseConverter(16, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);

        public static BaseConverter HexToOct { get; } =
            new BaseConverter(16, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);

        public static BaseConverter HexToDec { get; } =
            new BaseConverter(16, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

        public int From { get; }

        public int To { get; }

        public static BaseConverter Create(int fromRadix)
        {
            return new BaseConverter(fromRadix, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
        }

        public static BaseConverter Create(NumberBases fromRadix)
        {
            return new BaseConverter((int)fromRadix, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
        }

        public static BaseConverter Create(NumberBases fromRadix, NumberBases toRadix)
        {
            return new BaseConverter((int)fromRadix, NumberingSchemes.ZeroToZ, (int)toRadix, NumberingSchemes.ZeroToZ);
        }

        public static BaseConverter Create(int fromRadix, int toRadix)
        {
            return new BaseConverter(fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ);
        }

        public static BaseConverter Create(
            NumberBases fromRadix,
            NumberingSchemes fromScheme,
            NumberBases toRadix,
            NumberingSchemes toScheme)
        {
            return new BaseConverter((int)fromRadix, fromScheme, (int)toRadix, toScheme);
        }

        public static BaseConverter Create(
            int fromRadix,
            NumberingSchemes fromScheme,
            int toRadix,
            NumberingSchemes toScheme)
        {
            return new BaseConverter(fromRadix, fromScheme, toRadix, toScheme);
        }

        public static BaseConverter Create(
            int fromRadix,
            NumberingSchemes fromScheme,
            NumberBases toRadix,
            NumberingSchemes toScheme)
        {
            return new BaseConverter(fromRadix, fromScheme, (int)toRadix, toScheme);
        }

        public static BaseConverter Create(
            NumberBases fromRadix,
            NumberingSchemes fromScheme,
            int toRadix,
            NumberingSchemes toScheme)
        {
            return new BaseConverter((int)fromRadix, fromScheme, toRadix, toScheme);
        }

        public static string Convert(NumberBases fromRadix, string value)
        {
            return Convert((int)fromRadix, 10, value);
        }

        public static string Convert(int fromRadix, string value)
        {
            return Convert(fromRadix, 10, value);
        }

        public static string Convert(NumberBases fromRadix, NumberBases toRadix, string value)
        {
            return Convert((int)fromRadix, NumberingSchemes.ZeroToZ, (int)toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(NumberBases fromRadix, int toRadix, string value)
        {
            return Convert((int)fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(int fromRadix, int toRadix, string value)
        {
            return Convert(fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(
            NumberBases fromRadix,
            NumberingSchemes fromScheme,
            NumberBases toRadix,
            NumberingSchemes toScheme,
            string value)
        {
            return Convert((int)fromRadix, fromScheme, (int)toRadix, toScheme, value);
        }

        public static string Convert(
            NumberBases fromRadix,
            NumberingSchemes fromScheme,
            int toRadix,
            NumberingSchemes toScheme,
            string value)
        {
            return Convert((int)fromRadix, fromScheme, toRadix, toScheme, value);
        }

        public static string Convert(
            int fromRadix,
            NumberingSchemes fromScheme,
            NumberBases toRadix,
            NumberingSchemes toScheme,
            string value)
        {
            return Convert(fromRadix, fromScheme, (int)toRadix, toScheme, value);
        }

        public static string Convert(
            int fromRadix,
            NumberingSchemes fromScheme,
            int toRadix,
            NumberingSchemes toScheme,
            string value)
        {
            return new BaseConverter(fromRadix, fromScheme, toRadix, toScheme).Convert(value);
        }

        public string Convert(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            var base10 = ConvertToBase10(value, From, _fromNumberingScheme, _maxFromSchemeCharacter);
            return To != 10 ? ConvertFromBase10(base10, To, _toNumberingScheme) : base10.ToString();
        }

        private static string GetCharactersForNumberingScheme(NumberingSchemes scheme)
        {
            if (scheme == NumberingSchemes.AToZ)
                return "_ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (scheme == NumberingSchemes.ZeroToZ)
                return "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            throw new ArgumentOutOfRangeException(nameof(scheme));
        }

        private static long ConvertToBase10(
            string value,
            int fromBase,
            string characters,
            int maxFromSchemeCharacter)
        {
            var stringBuilder = new StringBuilder(value);
            var y = 0;
            long base10 = 0;
            while (stringBuilder.Length > 0)
            {
                var num = Array.IndexOf(characters.ToCharArray(), stringBuilder[stringBuilder.Length - 1]);
                if (num < 0)
                    throw new FormatException("Unsupported character in value string");
                if (num >= maxFromSchemeCharacter)
                    throw new FormatException("Value contains character not valid for number base");
                base10 += num * (long)Math.Pow(fromBase, y);
                if (base10 < 0L)
                    throw new OverflowException();
                --stringBuilder.Length;
                ++y;
            }

            return base10;
        }

        private static string ConvertFromBase10(long value, int toBase, string characters)
        {
            var stringBuilder = new StringBuilder();
            for (; value > 0L; value /= toBase)
            {
                var index = (int)(value % toBase);
                stringBuilder.Insert(0, characters[index]);
            }

            return stringBuilder.ToString();
        }
    }
}