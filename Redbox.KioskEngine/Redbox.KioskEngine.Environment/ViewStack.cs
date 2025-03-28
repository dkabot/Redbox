using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment
{
  internal class ViewStack : IEnumerable<IViewFrameInstance>, IEnumerable
  {
    private readonly Stack<IViewFrameInstance> m_stack = new Stack<IViewFrameInstance>();

    public IEnumerator<IViewFrameInstance> GetEnumerator()
    {
      return (IEnumerator<IViewFrameInstance>) new ViewStack.ViewStackEnumerator((IEnumerator<IViewFrameInstance>) this.m_stack.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator() => (IEnumerator) this.GetEnumerator();

    public void Clear() => this.m_stack.Clear();

    public IViewFrameInstance Pop()
    {
      return this.m_stack.Count != 0 ? this.m_stack.Pop() : (IViewFrameInstance) null;
    }

    public IViewFrameInstance Peek()
    {
      return this.m_stack.Count != 0 ? this.m_stack.Peek() : (IViewFrameInstance) null;
    }

    public void Push(Guid id, string viewName, object parameter = null)
    {
      IBaseViewFrame viewFrame = ViewService.Instance.GetViewFrame(viewName);
      this.m_stack.Push((IViewFrameInstance) new ViewFrameInstance()
      {
        Id = id,
        ViewFrame = viewFrame,
        Parameter = parameter
      });
    }

    public string PeekViewName()
    {
      if (this.m_stack.Count == 0)
        return (string) null;
      return this.m_stack.Peek()?.ViewFrame?.ViewName;
    }

    public int Count => this.m_stack.Count;

    private sealed class ViewStackEnumerator : 
      IEnumerator<IViewFrameInstance>,
      IDisposable,
      IEnumerator
    {
      private readonly IEnumerator<IViewFrameInstance> m_baseEnumerator;

      public ViewStackEnumerator(IEnumerator<IViewFrameInstance> baseEnumerator)
      {
        this.m_baseEnumerator = baseEnumerator;
      }

      public void Dispose() => this.m_baseEnumerator.Dispose();

      public bool MoveNext() => this.m_baseEnumerator.MoveNext();

      public void Reset() => this.m_baseEnumerator.Reset();

      public IViewFrameInstance Current => this.m_baseEnumerator?.Current;

      object IEnumerator.Current => (object) this.Current;
    }
  }
}
