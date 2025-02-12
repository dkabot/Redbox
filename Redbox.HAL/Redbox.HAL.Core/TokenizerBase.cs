using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core
{
    public abstract class TokenizerBase<T>
    {
        protected TokenizerBase(Stream tokenStream)
            : this()
        {
            this.m_tokenReader = new StreamTokenReader(tokenStream);
        }

        protected TokenizerBase(int lineNumber, string statement)
            : this()
        {
            this.m_tokenReader = new StringTokenReader(lineNumber, statement);
        }

        protected TokenizerBase()
        {
            this.BuildStateHandlerDictionary();
        }

        public void Tokenize()
        {
            this.Reset();
            this.StartStateMachine();
        }

        public ErrorList Errors
        {
            get
            {
                return this.m_errors;
            }
        }

        protected internal void StartStateMachine()
        {
            if (this.m_tokenReader.IsEmpty())
            {
                return;
            }
            for (; ; )
            {
                TokenizerBase<T>.StateHandler stateHandler = (this.m_handlers.ContainsKey(this.CurrentState) ? this.m_handlers[this.CurrentState] : null);
                if (stateHandler == null)
                {
                    break;
                }
                StateResult stateResult = stateHandler();
                if (stateResult != StateResult.Continue)
                {
                    if (stateResult == StateResult.Terminal)
                    {
                        goto Block_6;
                    }
                }
                else if (!this.MoveToNextToken())
                {
                    goto Block_7;
                }
            }
            string text = "No handler found where CurrentState = ";
            T currentState = this.CurrentState;
            throw new ArgumentException(text + ((currentState != null) ? currentState.ToString() : null));
        Block_6:
            this.StopMachine();
            return;
        Block_7:
            this.StopMachine();
        }

        protected internal string FormatError(string message)
        {
            return string.Format("(Line: {0} Column: {1}) {2}", this.m_tokenReader.Row, this.m_tokenReader.Column, message);
        }

        protected internal void Reset()
        {
            this.m_errors.Clear();
            this.ResetAccumulator();
            this.m_tokenReader.Reset();
            this.OnReset();
        }

        protected internal bool MoveToNextToken()
        {
            return this.m_tokenReader.MoveToNextToken();
        }

        protected internal char? PeekNextToken()
        {
            return this.m_tokenReader.PeekNextToken();
        }

        protected internal char? PeekNextToken(int i)
        {
            return this.m_tokenReader.PeekNextToken(i);
        }

        protected internal char GetCurrentToken()
        {
            return this.m_tokenReader.GetCurrentToken();
        }

        protected internal void ResetAccumulator()
        {
            this.Accumulator = new StringBuilder();
        }

        protected internal void AppendToAccumulator()
        {
            this.Accumulator.Append(this.GetCurrentToken());
        }

        protected internal string GetAccumulatedToken()
        {
            return this.Accumulator.ToString();
        }

        protected internal StringBuilder Accumulator { get; set; }

        protected internal T CurrentState { get; set; }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnEndOfStream()
        {
        }

        void BuildStateHandlerDictionary()
        {
            this.m_handlers.Clear();
            foreach (MethodInfo methodInfo in base.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                StateHandlerAttribute stateHandlerAttribute = (StateHandlerAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(StateHandlerAttribute));
                if (stateHandlerAttribute != null)
                {
                    this.m_handlers[(T)((object)stateHandlerAttribute.State)] = (TokenizerBase<T>.StateHandler)Delegate.CreateDelegate(typeof(TokenizerBase<T>.StateHandler), this, methodInfo.Name);
                }
            }
        }

        void StopMachine()
        {
            this.OnEndOfStream();
            if (this.Accumulator.Length > 0)
            {
                throw new Exception(string.Format("State Machine Terminated with [{0}] left in the buffer.", this.Accumulator));
            }
        }

        protected readonly ITokenReader m_tokenReader;

        readonly ErrorList m_errors = new ErrorList();

        readonly IDictionary<T, TokenizerBase<T>.StateHandler> m_handlers = new Dictionary<T, TokenizerBase<T>.StateHandler>();

        internal delegate StateResult StateHandler();
    }
}
