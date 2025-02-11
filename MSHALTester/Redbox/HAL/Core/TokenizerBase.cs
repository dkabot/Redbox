using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public abstract class TokenizerBase<T>
{
    private readonly IDictionary<T, StateHandler> m_handlers = new Dictionary<T, StateHandler>();

    protected readonly ITokenReader m_tokenReader;

    protected TokenizerBase(Stream tokenStream)
        : this()
    {
        m_tokenReader = new StreamTokenReader(tokenStream);
    }

    protected TokenizerBase(int lineNumber, string statement)
        : this()
    {
        m_tokenReader = new StringTokenReader(lineNumber, statement);
    }

    protected TokenizerBase()
    {
        BuildStateHandlerDictionary();
    }

    public ErrorList Errors { get; } = new();

    protected internal StringBuilder Accumulator { get; set; }

    protected internal T CurrentState { get; set; }

    public void Tokenize()
    {
        Reset();
        StartStateMachine();
    }

    protected internal void StartStateMachine()
    {
        if (m_tokenReader.IsEmpty()) return;
        for (;;)
        {
            var stateHandler = m_handlers.ContainsKey(CurrentState) ? m_handlers[CurrentState] : null;
            if (stateHandler == null) break;
            var stateResult = stateHandler();
            if (stateResult != StateResult.Continue)
            {
                if (stateResult == StateResult.Terminal) goto Block_6;
            }
            else if (!MoveToNextToken())
            {
                goto Block_7;
            }
        }

        var text = "No handler found where CurrentState = ";
        var currentState = CurrentState;
        throw new ArgumentException(text + (currentState != null ? currentState.ToString() : null));
        Block_6:
        StopMachine();
        return;
        Block_7:
        StopMachine();
    }

    protected internal string FormatError(string message)
    {
        return string.Format("(Line: {0} Column: {1}) {2}", m_tokenReader.Row, m_tokenReader.Column, message);
    }

    protected internal void Reset()
    {
        Errors.Clear();
        ResetAccumulator();
        m_tokenReader.Reset();
        OnReset();
    }

    protected internal bool MoveToNextToken()
    {
        return m_tokenReader.MoveToNextToken();
    }

    protected internal char? PeekNextToken()
    {
        return m_tokenReader.PeekNextToken();
    }

    protected internal char? PeekNextToken(int i)
    {
        return m_tokenReader.PeekNextToken(i);
    }

    protected internal char GetCurrentToken()
    {
        return m_tokenReader.GetCurrentToken();
    }

    protected internal void ResetAccumulator()
    {
        Accumulator = new StringBuilder();
    }

    protected internal void AppendToAccumulator()
    {
        Accumulator.Append(GetCurrentToken());
    }

    protected internal string GetAccumulatedToken()
    {
        return Accumulator.ToString();
    }

    protected virtual void OnReset()
    {
    }

    protected virtual void OnEndOfStream()
    {
    }

    private void BuildStateHandlerDictionary()
    {
        m_handlers.Clear();
        foreach (var methodInfo in GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var stateHandlerAttribute =
                (StateHandlerAttribute)Attribute.GetCustomAttribute(methodInfo, typeof(StateHandlerAttribute));
            if (stateHandlerAttribute != null)
                m_handlers[(T)stateHandlerAttribute.State] =
                    (StateHandler)Delegate.CreateDelegate(typeof(StateHandler), this, methodInfo.Name);
        }
    }

    private void StopMachine()
    {
        OnEndOfStream();
        if (Accumulator.Length > 0)
            throw new Exception(string.Format("State Machine Terminated with [{0}] left in the buffer.", Accumulator));
    }

    internal delegate StateResult StateHandler();
}