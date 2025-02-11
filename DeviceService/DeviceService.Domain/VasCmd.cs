namespace DeviceService.Domain
{
    public enum VasCmd
    {
        SetEnable = 1,
        SetMode = 3,
        GetData = 13, // 0x0000000D
        ClearAppleMerchantList = 500, // 0x000001F4
        AddAppleMerchantId = 501, // 0x000001F5
        GetAppleMerchantCount = 502, // 0x000001F6
        ClearGoogleMerchantList = 510, // 0x000001FE
        AddGoogleMerchantId = 511, // 0x000001FF
        GetGoogleMerchantCount = 512, // 0x00000200
        Unknown = 513 // 0x00000201
    }
}