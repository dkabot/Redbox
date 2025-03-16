namespace Redbox.Rental.Model.KioskClientService.Transactions
{
    public enum VendStatus
    {
        Vended = 0,
        HardwareError = 7,
        DiskNotTaken = 8,
        CriticalHardwareError = 9,
        EmptyOrStuck = 11, // 0x0000000B
        KioskFull = 14, // 0x0000000E
        CustomerDisabled = 15, // 0x0000000F
        NotVended = 28 // 0x0000001C
    }
}