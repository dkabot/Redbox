using System;

namespace HALUtilities
{
  public class DefaultConsole : IConsole, IDisposable
  {
    private readonly bool m_attachedToParentConsole;

    public DefaultConsole()
    {
      this.m_attachedToParentConsole = ConsoleTool.AttachConsoleToParentProcess();
      if (!this.m_attachedToParentConsole || Console.CursorTop + 1 >= Console.BufferHeight)
        return;
      Console.SetCursorPosition(0, Console.CursorTop + 1);
    }

    public void Write(string value) => Console.Write(value);

    public void WriteLine() => Console.WriteLine();

    public void WriteLine(object value) => Console.WriteLine(value);

    public void WriteLine(string value) => Console.WriteLine(value);

    public void WriteLine(string format, params object[] parms) => Console.WriteLine(format, parms);

    public void Dispose()
    {
      if (!this.m_attachedToParentConsole)
        return;
      ConsoleTool.FreeAttachedConsole();
    }

    public bool IsAttachedToConsole => this.m_attachedToParentConsole;
  }
}
