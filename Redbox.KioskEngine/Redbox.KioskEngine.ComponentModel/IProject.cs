using Redbox.REDS.Framework;
using System;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel
{
  public interface IProject
  {
    ErrorList Save();

    ErrorList Build();

    ErrorList AddBundleRef(string name, string path);

    ErrorList AddBundleRef(string name, IResourceBundle bundle);

    ErrorList RemoveBundleRef(string name);

    ErrorList RemoveBundleRef(IBundleRef bundleRef);

    Guid Id { get; }

    string Version { get; }

    DateTime? CreatedOn { get; }

    DateTime? ModifiedOn { get; }

    ReadOnlyCollection<IBundleRef> Bundles { get; }
  }
}
