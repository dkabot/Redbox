using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment
{
  public class Tween
  {
    public bool Update()
    {
      if (!this.Enabled)
        return true;
      ++this.Time;
      float newValue = TweenFunctions.ExecuteTween(this.Type, this.Time, this.Begin, this.Change, this.Duration);
      if (this.ChangeState != null)
        this.ChangeState(this.Name, this.Time, newValue);
      return (double) this.Time < (double) this.Duration;
    }

    public void ClearEnd() => this.End = (TweenEnd) null;

    public void ClearChangeState() => this.ChangeState = (TweenChange) null;

    public void RaiseEnd()
    {
      if (this.End == null)
        return;
      this.End(this.Name);
      this.Enabled = false;
      this.Time = 0.0f;
    }

    public string Name { get; set; }

    public bool Enabled { get; set; }

    public float Time { get; set; }

    public float Begin { get; set; }

    public float Finish { get; set; }

    public float Change => this.Finish - this.Begin;

    public TweenType Type { get; set; }

    public float Duration { get; set; }

    public event TweenEnd End;

    public event TweenChange ChangeState;
  }
}
