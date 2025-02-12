using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace HALUtilities.KioskTest
{
  internal sealed class QlmKioskTest : AbstractKioskTest
  {
    private int QlmFrequency = 2;

    protected override void OnProcessArgument(string argument)
    {
      if (!argument.StartsWith("--qlmFrequency"))
        return;
      this.QlmFrequency = CommandLineOption.GetOptionVal<int>(argument, this.QlmFrequency);
    }

    protected override int OnPreTest() => 0;

    protected override bool CanVisit(TestLocation loc) => loc.Deck != 8;

    protected override JobExecutor GetTransferExecutor(
      TestLocation start,
      TestLocation end,
      bool vend,
      int iteration)
    {
      return (JobExecutor) new QlmKioskTestExecutor(this.Service, start, end, vend, iteration % this.QlmFrequency == 0);
    }

    protected override int OnCleanup(TestLocation diskSource) => 0;

    protected override bool SingleTestNeeded() => false;

    internal QlmKioskTest(KioskConfiguration config, HardwareService s)
      : base(s, config)
    {
    }
  }
}
