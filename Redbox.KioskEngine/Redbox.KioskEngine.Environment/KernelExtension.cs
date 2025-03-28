using Redbox.KioskEngine.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Redbox.KioskEngine.Environment
{
  public class KernelExtension : IKernelExtension, IDisposable
  {
    private List<Guid> m_dependencies;

    public void Dispose()
    {
      if (this.Icon == null)
        return;
      this.Icon.Dispose();
    }

    public void RegisterCommunicationActivity(Action<object, EventArgs> action)
    {
      this.Host?.RegisterCommunicationActivity(action);
    }

    public Guid ID { get; internal set; }

    public string Name { get; set; }

    public Bitmap Icon { get; set; }

    public string Title { get; set; }

    public string Path { get; internal set; }

    public string Author { get; internal set; }

    public string Version { get; internal set; }

    public string Category { get; internal set; }

    public string Copyright { get; internal set; }

    public string Trademark { get; internal set; }

    public IKernelExtensionHost Host { get; set; }

    public string Description { get; internal set; }

    public bool IsUnloadSupported { get; internal set; }

    public ReadOnlyCollection<Guid> Dependencies => this.InnerDependencies.AsReadOnly();

    internal List<Guid> InnerDependencies
    {
      get
      {
        if (this.m_dependencies == null)
          this.m_dependencies = new List<Guid>();
        return this.m_dependencies;
      }
    }
  }
}
