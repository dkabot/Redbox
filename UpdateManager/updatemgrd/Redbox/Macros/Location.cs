using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Redbox.Macros
{
    [Serializable]
    internal class Location
    {
        public static readonly Location UnknownLocation = new Location();

        public Location(string fileName, int lineNumber, int columnNumber)
        {
            this.Init(fileName, lineNumber, columnNumber);
        }

        public Location(string fileName) => this.Init(fileName, 0, 0);

        private Location() => this.Init((string)null, 0, 0);

        private void Init(string fileName, int lineNumber, int columnNumber)
        {
            if (fileName != null)
            {
                try
                {
                    fileName = new Uri(fileName).LocalPath;
                }
                catch
                {
                    try
                    {
                        fileName = Path.GetFullPath(fileName);
                    }
                    catch (ArgumentException ex)
                    {
                    }
                }
            }
            this.FileName = fileName;
            this.LineNumber = lineNumber;
            this.ColumnNumber = columnNumber;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("");
            if (this.FileName != null)
            {
                stringBuilder.Append(this.FileName);
                if (this.LineNumber != 0)
                    stringBuilder.Append(string.Format((IFormatProvider)CultureInfo.InvariantCulture, "({0},{1})", (object)this.LineNumber, (object)this.ColumnNumber));
                stringBuilder.Append(":");
            }
            return stringBuilder.ToString();
        }

        public string FileName { get; private set; }

        public int LineNumber { get; private set; }

        public int ColumnNumber { get; private set; }
    }
}
