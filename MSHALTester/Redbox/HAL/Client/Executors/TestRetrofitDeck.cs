namespace Redbox.HAL.Client.Executors;

public sealed class TestRetrofitDeck(HardwareService service) : JobExecutor(service)
{
    protected override string JobName => "test-retrofit-deck";
}