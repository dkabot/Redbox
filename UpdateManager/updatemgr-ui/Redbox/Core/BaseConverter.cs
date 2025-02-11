using System;
using System.Text;

namespace Redbox.Core
{
    internal class BaseConverter
    {
        private readonly string _toNumberingScheme;
        private readonly string _fromNumberingScheme;
        private readonly int _maxFromSchemeCharacter;
        private static readonly BaseConverter _binToOct = new BaseConverter(2, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _binToDec = new BaseConverter(2, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _binToHex = new BaseConverter(2, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _octToBin = new BaseConverter(8, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _octToDec = new BaseConverter(8, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _octToHex = new BaseConverter(8, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _decToBin = new BaseConverter(10, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _decToOct = new BaseConverter(10, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _decToHex = new BaseConverter(10, NumberingSchemes.ZeroToZ, 16, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _hexToBin = new BaseConverter(16, NumberingSchemes.ZeroToZ, 2, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _hexToOct = new BaseConverter(16, NumberingSchemes.ZeroToZ, 8, NumberingSchemes.ZeroToZ);
        private static readonly BaseConverter _hexToDec = new BaseConverter(16, NumberingSchemes.ZeroToZ, 10, NumberingSchemes.ZeroToZ);

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
            return BaseConverter.Convert((int)fromRadix, 10, value);
        }

        public static string Convert(int fromRadix, string value)
        {
            return BaseConverter.Convert(fromRadix, 10, value);
        }

        public static string Convert(NumberBases fromRadix, NumberBases toRadix, string value)
        {
            return BaseConverter.Convert((int)fromRadix, NumberingSchemes.ZeroToZ, (int)toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(NumberBases fromRadix, int toRadix, string value)
        {
            return BaseConverter.Convert((int)fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(int fromRadix, int toRadix, string value)
        {
            return BaseConverter.Convert(fromRadix, NumberingSchemes.ZeroToZ, toRadix, NumberingSchemes.ZeroToZ, value);
        }

        public static string Convert(
          NumberBases fromRadix,
          NumberingSchemes fromScheme,
          NumberBases toRadix,
          NumberingSchemes toScheme,
          string value)
        {
            return BaseConverter.Convert((int)fromRadix, fromScheme, (int)toRadix, toScheme, value);
        }

        public static string Convert(
          NumberBases fromRadix,
          NumberingSchemes fromScheme,
          int toRadix,
          NumberingSchemes toScheme,
          string value)
        {
            return BaseConverter.Convert((int)fromRadix, fromScheme, toRadix, toScheme, value);
        }

        public static string Convert(
          int fromRadix,
          NumberingSchemes fromScheme,
          NumberBases toRadix,
          NumberingSchemes toScheme,
          string value)
        {
            return BaseConverter.Convert(fromRadix, fromScheme, (int)toRadix, toScheme, value);
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
            long base10 = BaseConverter.ConvertToBase10(value, this.From, this._fromNumberingScheme, this._maxFromSchemeCharacter);
            return this.To != 10 ? BaseConverter.ConvertFromBase10(base10, this.To, this._toNumberingScheme) : base10.ToString();
        }

        public static BaseConverter BinToOct => BaseConverter._binToOct;

        public static BaseConverter BinToDec => BaseConverter._binToDec;

        public static BaseConverter BinToHex => BaseConverter._binToHex;

        public static BaseConverter OctToBin => BaseConverter._octToBin;

        public static BaseConverter OctToDec => BaseConverter._octToDec;

        public static BaseConverter OctToHex => BaseConverter._octToHex;

        public static BaseConverter DecToBin => BaseConverter._decToBin;

        public static BaseConverter DecToOct => BaseConverter._decToOct;

        public static BaseConverter DecToHex => BaseConverter._decToHex;

        public static BaseConverter HexToBin => BaseConverter._hexToBin;

        public static BaseConverter HexToOct => BaseConverter._hexToOct;

        public static BaseConverter HexToDec => BaseConverter._hexToDec;

        public int From { get; private set; }

        public int To { get; private set; }

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
                throw new ArgumentOutOfRangeException(nameof(fromRadix), "Invalid numbering scheme for specified number base");
            if (toRadix > 26 && fromScheme == NumberingSchemes.AToZ)
                throw new ArgumentOutOfRangeException(nameof(toRadix), "Invalid numbering scheme for specified number base");
            this.From = fromRadix;
            this._fromNumberingScheme = BaseConverter.GetCharactersForNumberingScheme(fromScheme);
            this.To = toRadix;
            this._toNumberingScheme = BaseConverter.GetCharactersForNumberingScheme(toScheme);
            this._maxFromSchemeCharacter = fromScheme == NumberingSchemes.ZeroToZ ? fromRadix : fromRadix + 1;
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
            StringBuilder stringBuilder1 = new StringBuilder(value);
            int y = 0;
            long base10 = 0;
            while (stringBuilder1.Length > 0)
            {
                char[] charArray = characters.ToCharArray();
                StringBuilder stringBuilder2 = stringBuilder1;
                int num1 = (int)stringBuilder2[stringBuilder2.Length - 1];
                int num2 = Array.IndexOf<char>(charArray, (char)num1);
                if (num2 < 0)
                    throw new FormatException("Unsupported character in value string");
                if (num2 >= maxFromSchemeCharacter)
                    throw new FormatException("Value contains character not valid for number base");
                base10 += (long)num2 * (long)Math.Pow((double)fromBase, (double)y);
                if (base10 < 0L)
                    throw new OverflowException();
                --stringBuilder1.Length;
                ++y;
            }
            return base10;
        }

        private static string ConvertFromBase10(long value, int toBase, string characters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (; value > 0L; value /= (long)toBase)
            {
                int index = (int)(value % (long)toBase);
                stringBuilder.Insert(0, characters[index]);
            }
            return stringBuilder.ToString();
        }
    }
}
