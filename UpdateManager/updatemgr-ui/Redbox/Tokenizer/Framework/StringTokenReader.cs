using System;

namespace Redbox.Tokenizer.Framework
{
    internal class StringTokenReader : ITokenReader, IDisposable
    {
        private readonly string m_statement;

        public StringTokenReader(int row, string statement)
        {
            this.m_statement = statement;
            this.Row = (int)(ushort)row;
            this.Column = 0;
        }

        public char GetCurrentToken() => this.m_statement[this.Column];

        public bool MoveToNextToken()
        {
            ++this.Column;
            return this.Column < this.m_statement.Length;
        }

        public void Reset() => this.Column = 0;

        public char? PeekNextToken(int i)
        {
            return this.Column + i >= this.m_statement.Length ? new char?() : new char?(this.m_statement[this.Column + i]);
        }

        public char? PeekNextToken() => this.PeekNextToken(1);

        public bool IsEmpty() => string.IsNullOrEmpty(this.m_statement);

        public void IncrementRowCount() => ++this.Row;

        public void Dispose()
        {
        }

        public int Column { get; private set; }

        public int Row { get; private set; }

        public string RemainingTokens => this.m_statement.Substring(this.Column);
    }
}
