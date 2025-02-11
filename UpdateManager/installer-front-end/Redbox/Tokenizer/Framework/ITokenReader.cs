using System;

namespace Redbox.Tokenizer.Framework
{
    internal interface ITokenReader : IDisposable
    {
        void Reset();

        char? PeekNextToken();

        char? PeekNextToken(int i);

        char GetCurrentToken();

        bool MoveToNextToken();

        bool IsEmpty();

        int Row { get; }

        int Column { get; }

        string RemainingTokens { get; }
    }
}
