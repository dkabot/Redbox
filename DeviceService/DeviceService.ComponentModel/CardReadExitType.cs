namespace DeviceService.ComponentModel
{
    public enum CardReadExitType
    {
        GoodRead = 0,
        BadRead = 1,
        Cancelled = 2,
        ButtonPressed = 3,
        ClessCardFloorLimitExceeded = 4,
        MaxClessFloorLimitExceeded = 5,
        InvalidPrompt = 6,
        EncryptionFailed = 7,
        BadKeyCard = 8,
        BadFormatOf23 = 9,
        AmountWasNotSetAndContactlessReaderWasNotEnabled = 10, // 0x0000000A
        AtLeastOneSpecifiedReaderIsDisabled = 11, // 0x0000000B
        InvalidExitCode = 998, // 0x000003E6
        NotPerformed = 999 // 0x000003E7
    }
}