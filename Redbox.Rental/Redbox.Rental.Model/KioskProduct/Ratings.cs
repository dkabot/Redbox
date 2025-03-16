using System.ComponentModel;

namespace Redbox.Rental.Model.KioskProduct
{
    public enum Ratings : long
    {
        None = 0,

        [Description("General Audiences. All Ages Admitted.")]
        G = 5,

        [Description("Parental Guidance Suggested. Some Material May Not Be Suitable For Children.")]
        PG = 6,

        [Description("Parents Strongly Cautioned. Some Material May Be Inappropriate For Children Under 13.")]
        PG13 = 7,

        [Description("Restricted. Children Under 17 Require Accompanying Parent or Adult Guardian.")]
        R = 8,
        [Description("Not Rated")] NR = 10, // 0x000000000000000A
        [Description("ALL AGES")] ALLAGES = 13, // 0x000000000000000D

        [Description("Early Childhood, 6 and older")]
        EC = 14, // 0x000000000000000E
        [Description("Everyone")] E = 15, // 0x000000000000000F
        [Description("Everyone 10 and older")] E10PLUS = 16, // 0x0000000000000010

        [Description("Teen, ages 13 and older")]
        T = 17, // 0x0000000000000011
        [Description("Mature, 17 and older")] M17PLUS = 18, // 0x0000000000000012
        [Description("Rating Pending")] RP = 19, // 0x0000000000000013
        [Description("Adult Only")] AO = 20, // 0x0000000000000014
        [Description("Mature Audience Only")] TVMA = 21, // 0x0000000000000015

        [Description("Parental Guidance Suggested")]
        TVPG = 22, // 0x0000000000000016

        [Description("Parents Strongly Cautioned")]
        TV14 = 23, // 0x0000000000000017
        [Description("General Audience")] TVG = 24, // 0x0000000000000018
        [Description("All Children")] TVY = 25, // 0x0000000000000019

        [Description("Directed to Older Children")]
        TVY7 = 26, // 0x000000000000001A
        Max = 999 // 0x00000000000003E7
    }
}