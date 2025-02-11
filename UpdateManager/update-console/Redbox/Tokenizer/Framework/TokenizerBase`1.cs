using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Redbox.Tokenizer.Framework
{
    internal abstract class TokenizerBase<T>
    {
        protected readonly ITokenReader m_tokenReader;
        private readonly ErrorList m_errors = new ErrorList();
        private readonly IDictionary<T, TokenizerBase<T>.StateHandler> m_handlers = (IDictionary<T, TokenizerBase<T>.StateHandler>)new Dictionary<T, TokenizerBase<T>.StateHandler>();

        protected TokenizerBase(Stream tokenStream)
          : this()
        {
            this.m_tokenReader = (ITokenReader)new StreamTokenReader(tokenStream);
        }

        protected TokenizerBase(int lineNumber, string statement)
          : this()
        {
            this.m_tokenReader = (ITokenReader)new StringTokenReader(lineNumber, statement);
        }

        protected TokenizerBase() => this.BuildStateHandlerDictionary();

        public void Tokenize()
        {
            this.Reset();
            this.StartStateMachine();
        }

        public ErrorList Errors => this.m_errors;

        protected internal void StartStateMachine()
        {
            if (this.m_tokenReader.IsEmpty())
                return;
            do
            {
                TokenizerBase<T>.StateHandler handler = this.m_handlers.ContainsKey(this.CurrentState) ? this.m_handlers[this.CurrentState] : (TokenizerBase<T>.StateHandler)null;
                if (handler == null)
                    throw new ArgumentException("No handler found where CurrentState = " + (object)this.CurrentState);
                switch (handler())
                {
                    case StateResult.Continue:
                        continue;
                    case StateResult.Terminal:
                        this.StopMachine();
                        return;
                    default:
                        continue;
                }
            }
            while (this.MoveToNextToken());
            this.StopMachine();
        }

        protected internal string FormatError(string message)
        {
            return string.Format("(Line: {0} Column: {1}) {2}", (object)this.m_tokenReader.Row, (object)this.m_tokenReader.Column, (object)message);
        }

        protected internal void Reset()
        {
            this.m_errors.Clear();
            this.ResetAccumulator();
            this.m_tokenReader.Reset();
            this.OnReset();
        }

        protected internal bool MoveToNextToken() => this.m_tokenReader.MoveToNextToken();

        protected internal char? PeekNextToken() => this.m_tokenReader.PeekNextToken();

        protected internal char? PeekNextToken(int i) => this.m_tokenReader.PeekNextToken(i);

        protected internal char GetCurrentToken() => this.m_tokenReader.GetCurrentToken();

        protected internal void ResetAccumulator() => this.Accumulator = new StringBuilder();

        protected internal void AppendToAccumulator()
        {
            this.Accumulator.Append(this.GetCurrentToken());
        }

        protected internal string GetAccumulatedToken() => this.Accumulator.ToString();

        protected internal StringBuilder Accumulator { get; private set; }

        protected internal T CurrentState { get; set; }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnEndOfStream()
        {
        }

        private void BuildStateHandlerDictionary()
        {
            this.m_handlers.Clear();
            foreach (MethodInfo method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                StateHandlerAttribute customAttribute = (StateHandlerAttribute)Attribute.GetCustomAttribute((MemberInfo)method, typeof(StateHandlerAttribute));
                if (customAttribute != null)
                    this.m_handlers[(T)customAttribute.State] = (TokenizerBase<T>.StateHandler)Delegate.CreateDelegate(typeof(TokenizerBase<T>.StateHandler), (object)this, method.Name);
            }
        }

        private void StopMachine()
        {
            this.OnEndOfStream();
            if (this.Accumulator.Length > 0)
                throw new Exception(string.Format("State Machine Terminated with [{0}] left in the buffer.", (object)this.Accumulator));
        }

        internal delegate StateResult StateHandler();
    }
}
