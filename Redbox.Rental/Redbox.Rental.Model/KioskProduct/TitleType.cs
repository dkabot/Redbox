using System.ComponentModel.DataAnnotations;

namespace Redbox.Rental.Model.KioskProduct
{
    public enum TitleType
    {
        [TitleFamily(TitleFamily = TitleFamily.All)] [Display(Name = "All", Description = "All")]
        All = 0,

        [TitleFamily(TitleFamily = TitleFamily.Movies)] [Display(Name = "DVD", Description = "DVD")]
        DVD = 1,

        [TitleFamily(TitleFamily = TitleFamily.Movies)] [Display(Name = "Bluray", Description = "Blu-ray")]
        Bluray = 2,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "Xbox360", Description = "Xbox 360")]
        Xbox360 = 3,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "PS2", Description = "PS2")]
        PS2 = 4,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "PS3", Description = "PS3")]
        PS3 = 5,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "PSP", Description = "PSP")]
        PSP = 6,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "Wii", Description = "Wii")]
        Wii = 7,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "DS", Description = "Nintendo DS")]
        DS = 8,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "PC", Description = "PC")]
        PC = 9,

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "WiiU", Description = "Wii U")]
        WiiU = 10, // 0x0000000A

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "PS4", Description = "PS4")]
        PS4 = 11, // 0x0000000B

        [TitleFamily(TitleFamily = TitleFamily.Games)] [Display(Name = "XboxOne", Description = "Xbox One")]
        XboxOne = 12, // 0x0000000C

        [TitleFamily(TitleFamily = TitleFamily.Movies)] [Display(Name = "DigitalCode", Description = "Digital Code")]
        DigitalCode = 17, // 0x00000011

        [TitleFamily(TitleFamily = TitleFamily.Games)]
        [Display(Name = "NintendoSwitch", Description = "Nintendo Switch")]
        NintendoSwitch = 18, // 0x00000012

        [TitleFamily(TitleFamily = TitleFamily.Movies)] [Display(Name = "4K UHD", Description = "4K UHD")]
        _4KUHD = 19 // 0x00000013
    }
}