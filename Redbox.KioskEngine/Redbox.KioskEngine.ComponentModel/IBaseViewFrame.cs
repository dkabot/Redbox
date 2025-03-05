namespace Redbox.KioskEngine.ComponentModel
{
  public interface IBaseViewFrame
  {
    string ViewName { get; }

    void UpdateScene(IViewFrameInstance viewFrameInstance);

    IScene Scene { get; }

    bool RaiseOnEnter(IViewFrameInstance viewFrameInstance);

    void RaiseOnLeave(IViewFrameInstance viewFrameInstance);

    string ActiveFlag { get; }

    ErrorList Errors { get; }

    bool Clear { get; }

    bool IsMaintenanceModeView { get; set; }

    bool IsStartOverView { get; set; }
  }
}
