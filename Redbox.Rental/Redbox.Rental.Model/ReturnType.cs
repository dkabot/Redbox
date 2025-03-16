namespace Redbox.Rental.Model
{
    public enum ReturnType
    {
        Unknown = 1,
        ServiceMenu = 2,
        InventorySync = 3,
        Qlm = 4,
        ExceptionReturn = 5,
        KioskReturn = 6,
        QuickReturn = 7,
        BackwardsDiscReturn = 8,
        UnplayableReturn = 9,
        CrossBorderReturn = 10, // 0x0000000A
        FailedIRSecurity = 11, // 0x0000000B
        FailedLabelAudit = 12 // 0x0000000C
    }
}