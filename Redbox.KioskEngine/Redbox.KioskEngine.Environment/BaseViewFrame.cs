using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public abstract class BaseViewFrame : IBaseViewFrame
  {
    protected readonly ErrorList m_errors = new ErrorList();
    private bool _clear = true;

    public string ViewName { get; set; }

    public string ActiveFlag { get; set; }

    public bool IsMaintenanceModeView { get; set; }

    public bool IsStartOverView { get; set; }

    public IScene Scene { get; set; }

    public bool Clear
    {
      get => this._clear;
      set => this._clear = value;
    }

    public abstract void UpdateScene(IViewFrameInstance viewFrameInstance);

    public abstract bool RaiseOnEnter(IViewFrameInstance viewFrameInstance);

    public abstract void RaiseOnLeave(IViewFrameInstance viewFrameInstance);

    public ErrorList Errors => this.m_errors;
  }
}
