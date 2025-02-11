using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.UpdateService.Model
{
  public class RepositoryDiffWrapper
  {
    public string O { get; set; }

    public string I { get; set; }

    public List<Dictionary<string, string>> U { get; set; }

    public List<ClientRepositoryDataDTO> A { get; set; }

    public List<string> D { get; set; }

    public RepositoryDiffWrapper Copy()
    {
      return new RepositoryDiffWrapper()
      {
        O = string.Copy(this.O),
        I = string.Copy(this.I),
        A = this.A.Copy(),
        D = this.D.ToList<string>(),
        U = this.U.Select<Dictionary<string, string>, Dictionary<string, string>>((Func<Dictionary<string, string>, Dictionary<string, string>>) (each => new Dictionary<string, string>((IDictionary<string, string>) each))).ToList<Dictionary<string, string>>()
      };
    }
  }
}
