using System;
using System.IO;

namespace Redbox.Tokenizer.Framework
{
    internal class StreamTokenReader : ITokenReader, IDisposable
    {
        private int m_index;
        private int m_bufferSize;
        private readonly char[] m_buffer;
        private readonly StreamReader m_stream;
        private readonly int BufferSize;
        private readonly int MaxLookAhead;
        private bool m_resetColumnOnNextMove;
        private bool m_twoCharNewLine;

        public StreamTokenReader(Stream stream, int maxLookAhead)
        {
            this.MaxLookAhead = maxLookAhead;
            this.BufferSize = this.MaxLookAhead + 1;
            this.m_buffer = new char[this.BufferSize];
            this.m_stream = new StreamReader(stream);
            this.Row = 1;
            this.Column = 1;
            this.m_bufferSize = this.m_stream.ReadBlock(this.m_buffer, 0, this.m_buffer.Length);
            this.m_index = 0;
            this.m_twoCharNewLine = false;
            this.m_resetColumnOnNextMove = false;
            this.AdavnceRowCount();
        }

        public StreamTokenReader(Stream stream)
          : this(stream, 1024)
        {
        }

        public char GetCurrentToken() => this.m_buffer[this.m_index];

        public char? PeekNextToken() => this.PeekNextToken(1);

        public char? PeekNextToken(int i)
        {
            if (this.m_index + i < 0)
                return new char?();
            if (i > this.MaxLookAhead)
                throw new ArgumentException(string.Format("This StreamTokenReader can only look {0} charactes ahead.", (object)this.MaxLookAhead));
            if (this.m_index + i < this.m_bufferSize)
                return new char?(this.m_buffer[this.m_index + i]);
            Array.Copy((Array)this.m_buffer, this.m_index, (Array)this.m_buffer, 0, this.m_bufferSize - this.m_index);
            this.m_bufferSize = this.m_stream.ReadBlock(this.m_buffer, this.m_bufferSize - this.m_index, this.m_index) + (this.m_bufferSize - this.m_index);
            this.m_index = 0;
            return i >= this.m_bufferSize ? new char?() : new char?(this.m_buffer[i]);
        }

        public bool MoveToNextToken()
        {
            if (this.m_resetColumnOnNextMove)
            {
                this.Column = 1;
                this.m_resetColumnOnNextMove = false;
            }
            else
                ++this.Column;
            ++this.m_index;
            if (this.m_index < this.m_bufferSize)
            {
                this.AdavnceRowCount();
                return true;
            }
            this.m_index = 0;
            this.AdavnceRowCount();
            this.m_bufferSize = this.m_stream.ReadBlock(this.m_buffer, 0, this.m_buffer.Length);
            return this.m_bufferSize > 0;
        }

        public void Reset()
        {
        }

        public bool IsEmpty() => this.m_index >= this.m_bufferSize && this.m_stream.EndOfStream;

        public int Row { get; private set; }

        public int Column { get; private set; }

        public string RemainingTokens
        {
            get => throw new NotImplementedException("Not implementing for streams.");
        }

        private void AdavnceRowCount()
        {
            if (this.GetCurrentToken() == '\r')
            {
                char? nullable = this.PeekNextToken();
                if (nullable.HasValue)
                {
                    nullable = this.PeekNextToken();
                    if (nullable.Value == '\n')
                    {
                        this.m_twoCharNewLine = true;
                        this.m_resetColumnOnNextMove = false;
                        goto label_5;
                    }
                }
                this.m_resetColumnOnNextMove = true;
            label_5:
                ++this.Row;
            }
            else
            {
                if (this.GetCurrentToken() != '\n')
                    return;
                this.Column = 1;
                if (this.m_twoCharNewLine)
                {
                    this.m_resetColumnOnNextMove = true;
                    this.m_twoCharNewLine = false;
                }
                else
                {
                    this.m_resetColumnOnNextMove = true;
                    ++this.Row;
                }
            }
        }

        public void Dispose() => this.m_stream.Dispose();
    }
}
