using Redbox.KioskEngine.ComponentModel;
using System;

namespace Redbox.KioskEngine.Environment
{
  public class ViewFrameInstance : IViewFrameInstance
  {
    public Guid Id { get; set; }

    public IBaseViewFrame ViewFrame { get; set; }

    public object Parameter { get; set; }
  }
}
