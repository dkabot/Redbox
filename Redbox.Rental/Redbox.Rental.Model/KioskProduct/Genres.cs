using System.ComponentModel;

namespace Redbox.Rental.Model.KioskProduct
{
    public enum Genres : long
    {
        [Description("Action And Adventure")] ActionAndAdventure = 1000, // 0x00000000000003E8
        [Description("Animation")] Animation = 1001, // 0x00000000000003E9
        [Description("Award Winners")] AwardWinners = 1002, // 0x00000000000003EA
        [Description("Crime")] Crime = 1003, // 0x00000000000003EB
        [Description("Comedy")] Comedy = 1004, // 0x00000000000003EC
        [Description("Drama")] Drama = 1005, // 0x00000000000003ED
        [Description("Family")] FamilyMovies = 1006, // 0x00000000000003EE
        [Description("Foreign")] Foreign = 1007, // 0x00000000000003EF
        [Description("Holiday")] Holiday = 1008, // 0x00000000000003F0
        [Description("Horror")] Horror = 1009, // 0x00000000000003F1
        [Description("Kids")] Kids = 1010, // 0x00000000000003F2
        [Description("Musical")] Musical = 1011, // 0x00000000000003F3
        [Description("Romance")] Romance = 1012, // 0x00000000000003F4
        [Description("SciFi And Fantasy")] SciFiAndFantasy = 1013, // 0x00000000000003F5
        [Description("Special Interest")] SpecialInterest = 1014, // 0x00000000000003F6
        [Description("Suspense")] Suspense = 1016, // 0x00000000000003F8
        [Description("Television")] Television = 1017, // 0x00000000000003F9
        [Description("War")] War = 1018, // 0x00000000000003FA
        [Description("Western")] Western = 1019, // 0x00000000000003FB
        [Description("Hit Movies")] HitMovies = 1020, // 0x00000000000003FC
        [Description("GAME")] GAME = 1021, // 0x00000000000003FD
        [Description("Bluray")] Bluray = 1022, // 0x00000000000003FE
        [Description("redbox Replay")] RedboxReplayMovies = 1025, // 0x0000000000000401
        [Description("Action Games")] ActionGames = 1026, // 0x0000000000000402
        [Description("Fighting")] Fighting = 1028, // 0x0000000000000404
        [Description("Music And Party")] MusicAndParty = 1030, // 0x0000000000000406
        [Description("Shooter")] Shooter = 1034, // 0x000000000000040A
        [Description("Sports")] Sports = 1036, // 0x000000000000040C
        [Description("Documentary")] Documentary = 1092, // 0x0000000000000444
        [Description("Top 20 Movies")] Top20Movies = 1093, // 0x0000000000000445
        [Description("Family")] FamilyGames = 1094, // 0x0000000000000446
        [Description("redbox Replay")] RedboxReplayGames = 1097, // 0x0000000000000449
        [Description("War And Western")] WarAndWestern = 1098, // 0x000000000000044A
        [Description("Martial Arts")] MartialArts = 1099, // 0x000000000000044B
        [Description("Independent")] Independent = 1100, // 0x000000000000044C

        [Description("Documentary And Special Interest")]
        DocumentaryAndSpecialInterest = 1101, // 0x000000000000044D
        [Description("Adventure")] Adventure = 1102, // 0x000000000000044E
        [Description("Action")] Action = 1103, // 0x000000000000044F
        [Description("Award Nominees")] AwardNominees = 1104 // 0x0000000000000450
    }
}