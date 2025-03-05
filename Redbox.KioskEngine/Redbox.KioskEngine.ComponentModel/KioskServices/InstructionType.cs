namespace Redbox.KioskEngine.ComponentModel.KioskServices
{
  public enum InstructionType : byte
  {
    Start = 1,
    Shutdown = 2,
    WaitAbsolute = 3,
    SetRequestsPerSecond = 4,
    WaitForSuccessfulPing = 5,
  }
}
