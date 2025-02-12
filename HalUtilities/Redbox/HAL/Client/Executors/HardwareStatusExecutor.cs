namespace Redbox.HAL.Client.Executors
{
  public sealed class HardwareStatusExecutor : JobExecutor
  {
    public bool InventoryError { get; private set; }

    public bool VendDoorError { get; private set; }

    public bool HardwareOk { get; private set; }

    public bool MotionControlError { get; private set; }

    public bool PickerObstructed { get; private set; }

    protected override string JobName => "hardware-status";

        public HardwareStatusExecutor(HardwareService service) : base(service)
        {
        }

        protected override void OnJobCompleted()
    {
      foreach (ProgramResult result in this.Results)
      {
        if (result.Code == "MotionControllerCommunicationError")
          this.MotionControlError = true;
        else if (result.Code == "GripperObstructed")
          this.PickerObstructed = true;
        else if (result.Code == "VendDoorOpen")
          this.VendDoorError = true;
        else if (result.Code == "InventoryStoreError")
          this.InventoryError = true;
        else if (result.Code == "HardwareStatusInError")
          this.HardwareOk = false;
        else if (result.Code == "HardwareStatusOk")
          this.HardwareOk = true;
      }
    }
  }
}
