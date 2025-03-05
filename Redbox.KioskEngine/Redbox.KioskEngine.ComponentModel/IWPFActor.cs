namespace Redbox.KioskEngine.ComponentModel
{
  public interface IWPFActor
  {
    event WPFHitHandler OnWPFHit;

    IActor Actor { get; set; }
  }
}
