using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.Environment
{
  public class Project : IProject
  {
    public Project()
    {
      this.Id = Guid.NewGuid();
      this.Version = "1.0";
      this.CreatedOn = new DateTime?(DateTime.Now);
      this.InnerBundles = new List<IBundleRef>();
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList Save() => new Redbox.KioskEngine.ComponentModel.ErrorList();

    public Redbox.KioskEngine.ComponentModel.ErrorList Build() => new Redbox.KioskEngine.ComponentModel.ErrorList();

    public Redbox.KioskEngine.ComponentModel.ErrorList AddBundleRef(string name, string path)
    {
      return new Redbox.KioskEngine.ComponentModel.ErrorList();
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList AddBundleRef(
      string name,
      IResourceBundle bundle)
    {
      return new Redbox.KioskEngine.ComponentModel.ErrorList();
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList RemoveBundleRef(string name)
    {
      return new Redbox.KioskEngine.ComponentModel.ErrorList();
    }

    public Redbox.KioskEngine.ComponentModel.ErrorList RemoveBundleRef(IBundleRef bundleRef)
    {
      return new Redbox.KioskEngine.ComponentModel.ErrorList();
    }

    public Guid Id { get; set; }

    public string Version { get; set; }

    public string FileName { get; set; }

    public DateTime? CreatedOn { get; set; }

    public DateTime? ModifiedOn { get; set; }

    public ReadOnlyCollection<IBundleRef> Bundles => this.InnerBundles.AsReadOnly();

    public List<IBundleRef> InnerBundles { get; set; }
  }
}
