using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DeviceService.ComponentModel;

namespace DeviceService.Domain
{
    public class ParseTags
    {
        private const string CDIV = "CDIV";
        private const string CNSUP = "CNSUP";

        private readonly IDictionary<string, FallbackStatusAction> _fallbackErrorStatusActions =
            new Dictionary<string, FallbackStatusAction>
            {
                {
                    nameof(CDIV),
                    FallbackStatusAction.IncrementFallbackCounter
                },
                {
                    nameof(CNSUP),
                    FallbackStatusAction.NoAction
                },
                {
                    "CRPRE",
                    FallbackStatusAction.NoAction
                },
                {
                    "APBLK",
                    FallbackStatusAction.ResetFallbackCounter
                },
                {
                    "CABLK",
                    FallbackStatusAction.ResetFallbackCounter
                },
                {
                    "CEXP",
                    FallbackStatusAction.ResetFallbackCounter
                }
            };

        public CardBrandEnum CardBrand;
        public FallbackType? FallbackReason;

        public string Last4 { get; set; }

        public string First6 { get; set; }

        public string AID { get; set; }

        public string PAN { get; set; }

        public string Track2 { get; set; }

        public string ApplicationLabel { get; set; }

        public string CardHolderName { get; set; }

        public bool IsMobileWallet { get; set; }

        public string ErrorCode { get; set; }

        public string FF1FOnGuardCardData { get; set; }

        public FallbackStatusAction Fallback { get; set; } = FallbackStatusAction.ResetFallbackCounter;

        public bool Tag9F27CryptogramDeclined { get; set; }

        public void ParseEMVTags(
            string Tag,
            int TagLength,
            string TagDataStr,
            byte[] TagDataByte,
            CardSourceType transactionType,
            Action<string> log,
            IList<Error> errors)
        {
            var generalSourceType = transactionType.GetGeneralSourceType();
            var str1 = "[Pii Data]";
            var tagDetails = TagConstants.Details.FirstOrDefault(x => x.Tag == Tag);
            IUC285Proxy.Log(string.Format("Tag {0}({1}) Length = {2} TagData = {3}", Tag,
                tagDetails?.Description ?? "Undefined", TagLength,
                tagDetails == null || !tagDetails.IsPii ? TagDataStr.ToUpper() : (object)str1));
            if (Tag == "4F" || Tag == "84")
            {
                AID = TagDataStr.ToUpper();
                CardBrand = AID.Contains("A000000003") || AID.Contains("A00000009") ? CardBrandEnum.VISA :
                    !AID.Contains("A00000002") ? !AID.Contains("A000000004")
                        ? AID.Contains("A0000001") || AID.Contains("A000000324") ? CardBrandEnum.Discover :
                        !AID.Contains("A000000333") ? CardBrandEnum.Unknown : CardBrandEnum.UnionPay
                        : CardBrandEnum.Mastercard : CardBrandEnum.Amex;
                IUC285Proxy.Log(string.Format("CardBrand: {0}", CardBrand));
            }
            else if (Tag == "50")
            {
                ApplicationLabel = ConvertHexToString(TagDataStr, errors).ToUpper();
            }
            else if (Tag == "57")
            {
                Track2 = TagDataStr.ToUpper();
                First6 = TagDataStr.Substring(0, 6);
                Last4 = TagDataStr.Substring(12, 4);
            }
            else if (Tag == "5A")
            {
                PAN = TagDataStr;
            }
            else
            {
                if (Tag == "8A")
                    return;
                if (Tag == "9B")
                {
                    IUC285Proxy.Log("--------Transaction Status Information (T9B)--------");
                    var str2 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str3 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str2);
                    IUC285Proxy.Log("  " + str2[0] + " - Offline data authentication was performed");
                    IUC285Proxy.Log("  " + str2[1] + " - Cardholder verification was performed");
                    IUC285Proxy.Log("  " + str2[2] + " - Card risk management was performed");
                    IUC285Proxy.Log("  " + str2[3] + " - Issuer authentication was performed");
                    var ch = str2[4];
                    IUC285Proxy.Log("  " + ch + " - Terminal risk management was performed");
                    ch = str2[5];
                    IUC285Proxy.Log("  " + ch + " - Script processing was performed");
                    ch = str2[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str2[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 2: " + str3);
                    ch = str3[0];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[1];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[2];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[3];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str3[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F10")
                {
                    if (CardBrand.Equals(CardBrandEnum.Amex))
                    {
                        IUC285Proxy.Log("--------Issuer Application Data(T9F10) " + CardBrand + " --------");
                        var startIndex1 = 0;
                        IUC285Proxy.Log("Byte 1 - Length Byte = " + TagDataStr.Substring(startIndex1, 2));
                        var startIndex2 = startIndex1 + 2;
                        IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + TagDataStr.Substring(startIndex2, 2));
                        var startIndex3 = startIndex2 + 2;
                        IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + TagDataStr.Substring(startIndex3, 2));
                        var startIndex4 = startIndex3 + 2;
                        var str4 = TagDataStr.Substring(startIndex4, 2);
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex4, 8).ToUpper());
                        IUC285Proxy.Log("CVR Byte 1 - Length = " + str4);
                        var startIndex5 = startIndex4 + 2;
                        var int32 = Convert.ToInt32(str4);
                        var index = 0;
                        var strArray1 = new string[int32];
                        var numArray = new int[int32];
                        var strArray2 = new string[int32];
                        for (; index < int32 && startIndex5 + 2 <= TagDataStr.Length; startIndex5 += 2)
                        {
                            strArray1[index] = TagDataStr.Substring(startIndex5, 2);
                            numArray[index] = Convert.ToInt32(strArray1[index], 16);
                            strArray2[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("-----");
                        if (int32 == 0)
                        {
                            IUC285Proxy.Log("No Additional CVR bits");
                        }
                        else
                        {
                            IUC285Proxy.Log("CVR Byte 2: " + strArray2[0]);
                            var str5 = strArray2[0].Substring(0, 2);
                            switch (str5)
                            {
                                case "00":
                                    IUC285Proxy.Log("    bit 8-7 = " + str5 + " - AC returned in 2nd GENERATE AC: AAC");
                                    break;
                                case "01":
                                    IUC285Proxy.Log("    bit 8-7 = " + str5 + " - AC returned in 2nd GENERATE AC: TC");
                                    break;
                                case "10":
                                    IUC285Proxy.Log("    bit 8-7 = " + str5 +
                                                    " - AC returned in 2nd GENERATE AC: Not requested");
                                    break;
                                case "11":
                                    IUC285Proxy.Log("    bit 8-7 = " + str5 + " - AC returned in 2nd GENERATE AC: RFU");
                                    break;
                            }

                            var str6 = strArray2[0].Substring(2, 2);
                            if (str6 == "00")
                                IUC285Proxy.Log("    bit 6-5 = " + str6 + " - AC returned in 1st GENERATE AC: AAC");
                            if (str6 == "01")
                                IUC285Proxy.Log("    bit 6-5 = " + str6 + " - AC returned in 1st GENERATE AC: TC");
                            if (str6 == "10")
                                IUC285Proxy.Log("    bit 6-5 = " + str6 + " - AC returned in 1st GENERATE AC: ARQC");
                            if (str6 == "11")
                                IUC285Proxy.Log("    bit 6-5 = " + str6 + " - AC returned in 1st GENERATE AC: RFU");
                            var str7 = strArray2[0].Substring(4, 1);
                            if (str7 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str7 + "  - Issuer Authentication did not fail");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str7 + "  - Issuer Authentication failed");
                            var str8 = strArray2[0].Substring(5, 1);
                            if (str8 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str8 + "  - Offline PIN verification not performed");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str8 + "  - Offline PIN verification performed");
                            var str9 = strArray2[0].Substring(6, 1);
                            if (str9 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str9 + "  - Offline PIN verification did not fail");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str9 + "  - Offline PIN verification failed");
                            var str10 = strArray2[0].Substring(7, 1);
                            if (str10 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str10 + "  - Not unable to go online");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str10 + "  - Unable to go online");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 3: " + strArray2[1]);
                            var str11 = strArray2[1].Substring(0, 1);
                            if (str11 == "0")
                                IUC285Proxy.Log("    bit 8   = " + str11 + " - Last online transaction completed");
                            else
                                IUC285Proxy.Log("    bit 8   = " + str11 + " - Last online transaction not completed");
                            var str12 = strArray2[1].Substring(1, 1);
                            if (str12 == "0")
                                IUC285Proxy.Log("    bit 7   = " + str12 + " - PIN try limit not exceeded");
                            else
                                IUC285Proxy.Log("    bit 7   = " + str12 + " - PIN try limit exceeded");
                            var str13 = strArray2[1].Substring(2, 1);
                            if (str13 == "0")
                                IUC285Proxy.Log("    bit 6   = " + str13 +
                                                " - Velocity checking counters not exceeded");
                            else
                                IUC285Proxy.Log("    bit 6   = " + str13 + " - Velocity checking counters exceeded");
                            var str14 = strArray2[1].Substring(3, 1);
                            if (str14 == "0")
                                IUC285Proxy.Log("    bit 5   = " + str14 + " - Not a new card");
                            else
                                IUC285Proxy.Log("    bit 5   = " + str14 + " - New card");
                            var str15 = strArray2[1].Substring(4, 1);
                            if (str15 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str15 +
                                                " - Issuer Authentication did not fail on last online transaction");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str15 +
                                                " - Issuer Authentication failed on last online transaction");
                            var str16 = strArray2[1].Substring(5, 1);
                            if (str16 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str16 +
                                                " - Issuer Authentication performed after online authorization");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str16 +
                                                " - Issuer Authentication not performed after online authorization");
                            var str17 = strArray2[1].Substring(6, 1);
                            if (str17 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str17 +
                                                " - Application not blocked by card because PIN Try Limit exceeded");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str17 +
                                                " - Application blocked by card because PIN Try Limit exceeded");
                            var str18 = strArray2[1].Substring(7, 1);
                            if (str18 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str18 +
                                                " - Offline static data authentication did not fail on last transaction or transaction not declined offline");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str18 +
                                                " - Offline static data authentication failed on last transaction or transaction declined offline");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 4: " + strArray2[2]);
                            IUC285Proxy.Log("    bit 8-5 = " + strArray2[2].Substring(0, 4) +
                                            " - Number of Issuer Script Commands received after the second GENERATE AC command");
                            var str19 = strArray2[2].Substring(4, 1);
                            if (str19 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str19 +
                                                "    - Issuer Script processing did not fail on last transaction");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str19 +
                                                "    - Issuer Script processing failed on last transaction");
                            var str20 = strArray2[2].Substring(5, 1);
                            if (str20 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str20 +
                                                "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str20 +
                                                "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
                            var str21 = strArray2[2].Substring(6, 1);
                            if (str21 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str21 +
                                                "    - Offline dynamic data authentication not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str21 +
                                                "    - Offline dynamic data authentication performed");
                            IUC285Proxy.Log("    bit 1   = " + strArray2[2].Substring(7, 1) + " -    RFU");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("----------------------------------------------------");
                        }
                    }
                    else if (CardBrand.Equals(CardBrandEnum.Amex) && generalSourceType == GeneralSourceType.Tap)
                    {
                        IUC285Proxy.Log("-----Issuer Application Data(T9F10) " + CardBrand + " -----");
                        var startIndex6 = 0;
                        IUC285Proxy.Log("Byte 1 - Length Byte = " + TagDataStr.Substring(startIndex6, 2));
                        var startIndex7 = startIndex6 + 2;
                        IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + TagDataStr.Substring(startIndex7, 2));
                        var startIndex8 = startIndex7 + 2;
                        IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " + TagDataStr.Substring(startIndex8, 2));
                        var startIndex9 = startIndex8 + 2;
                        var str22 = TagDataStr.Substring(startIndex9, 2);
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex9, 8).ToUpper());
                        IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + str22);
                        var startIndex10 = startIndex9 + 2;
                        var int32 = Convert.ToInt32(str22);
                        var index = 0;
                        var strArray3 = new string[int32];
                        var numArray = new int[int32];
                        var strArray4 = new string[int32];
                        for (; index < int32 && startIndex10 + 2 <= TagDataStr.Length; startIndex10 += 2)
                        {
                            strArray3[index] = TagDataStr.Substring(startIndex10, 2);
                            numArray[index] = Convert.ToInt32(strArray3[index], 16);
                            strArray4[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("-----");
                        if (int32 == 0)
                        {
                            IUC285Proxy.Log("No Additional CVR bits");
                        }
                        else
                        {
                            IUC285Proxy.Log("CVR Byte 2: " + strArray4[0]);
                            var str23 = strArray4[0].Substring(0, 2);
                            switch (str23)
                            {
                                case "00":
                                    IUC285Proxy.Log("    bit 8-7 = " + str23 +
                                                    " - AC returned in 2nd GENERATE AC: AAC");
                                    break;
                                case "01":
                                    IUC285Proxy.Log("    bit 8-7 = " + str23 + " - AC returned in 2nd GENERATE AC: TC");
                                    break;
                                case "10":
                                    IUC285Proxy.Log("    bit 8-7 = " + str23 +
                                                    " - AC returned in 2nd GENERATE AC: Not requested");
                                    break;
                                case "11":
                                    IUC285Proxy.Log("    bit 8-7 = " + str23 +
                                                    " - AC returned in 2nd GENERATE AC: RFU");
                                    break;
                            }

                            var str24 = strArray4[0].Substring(2, 2);
                            if (str24 == "00")
                                IUC285Proxy.Log("    bit 6-5 = " + str24 + " - AC returned in 1st GENERATE AC: AAC");
                            if (str24 == "01")
                                IUC285Proxy.Log("    bit 6-5 = " + str24 + " - AC returned in 1st GENERATE AC: TC");
                            if (str24 == "10")
                                IUC285Proxy.Log("    bit 6-5 = " + str24 + " - AC returned in 1st GENERATE AC: ARQC");
                            if (str24 == "11")
                                IUC285Proxy.Log("    bit 6-5 = " + str24 + " - AC returned in 1st GENERATE AC: RFU");
                            var str25 = strArray4[0].Substring(4, 1);
                            if (str25 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str25 + "  - RFU");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str25 + "  - RFU");
                            var str26 = strArray4[0].Substring(5, 1);
                            if (str26 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str26 + "  - RFU");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str26 + "  - RFU");
                            var str27 = strArray4[0].Substring(6, 1);
                            if (str27 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str27 + "  - RFU");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str27 + "  - RFU");
                            var str28 = strArray4[0].Substring(7, 1);
                            if (str28 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str28 + "  - RFU");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str28 + "  - RFU");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 3: " + strArray4[1]);
                            var str29 = strArray4[1].Substring(0, 1);
                            if (str29 == "0")
                                IUC285Proxy.Log("    bit 8   = " + str29 + " - Last online transaction completed");
                            else
                                IUC285Proxy.Log("    bit 8   = " + str29 + " - Last online transaction not completed");
                            var str30 = strArray4[1].Substring(1, 1);
                            if (str30 == "0")
                                IUC285Proxy.Log("    bit 7   = " + str30 + " - PIN try limit not exceeded");
                            else
                                IUC285Proxy.Log("    bit 7   = " + str30 + " - PIN try limit exceeded");
                            var str31 = strArray4[1].Substring(2, 1);
                            if (str31 == "0")
                                IUC285Proxy.Log("    bit 6   = " + str31 +
                                                " - Velocity checking counters not exceeded");
                            else
                                IUC285Proxy.Log("    bit 6   = " + str31 + " - Velocity checking counters exceeded");
                            var str32 = strArray4[1].Substring(3, 1);
                            if (str32 == "0")
                                IUC285Proxy.Log("    bit 5   = " + str32 + " - Not a new card");
                            else
                                IUC285Proxy.Log("    bit 5   = " + str32 + " - New card");
                            var str33 = strArray4[1].Substring(4, 1);
                            if (str33 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str33 +
                                                " - Issuer Authentication did not fail on last online transaction");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str33 +
                                                " - Issuer Authentication failed on last online transaction");
                            var str34 = strArray4[1].Substring(5, 1);
                            if (str34 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str34 + " - RFU");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str34 + " - RFU");
                            var str35 = strArray4[1].Substring(6, 1);
                            if (str35 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str35 +
                                                " - Application not blocked by card because PIN Try Limit exceeded");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str35 +
                                                " - Application blocked by card because PIN Try Limit exceeded");
                            var str36 = strArray4[1].Substring(7, 1);
                            if (str36 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str36 + " - RFU");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str36 + " - RFU");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 4: " + strArray4[2]);
                            IUC285Proxy.Log("    bit 8-5 = " + strArray4[2].Substring(0, 4) + " - RFU");
                            var str37 = strArray4[2].Substring(4, 1);
                            if (str37 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str37 + "    - RFU");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str37 + "    - RFU");
                            var str38 = strArray4[2].Substring(5, 1);
                            if (str38 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str38 +
                                                "    - Offline dynamic data authentication did not fail on last transaction or transaction not declined offline");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str38 +
                                                "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
                            var str39 = strArray4[2].Substring(6, 1);
                            if (str39 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str39 +
                                                "    - Offline dynamic data authentication not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str39 +
                                                "    - Offline dynamic data authentication performed");
                            IUC285Proxy.Log("    bit 1   = " + strArray4[2].Substring(7, 1) + " -    RFU");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("----------------------------------------------------");
                        }
                    }
                    else if (CardBrand.Equals(CardBrandEnum.VISA))
                    {
                        IUC285Proxy.Log("---------Issuer Application Data (T9F10) " + CardBrand + " ---------");
                        var startIndex11 = 0;
                        IUC285Proxy.Log("Byte 1 - Length Byte = " + TagDataStr.Substring(startIndex11, 2));
                        var startIndex12 = startIndex11 + 2;
                        IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + TagDataStr.Substring(startIndex12, 2));
                        var startIndex13 = startIndex12 + 2;
                        IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " +
                                        TagDataStr.Substring(startIndex13, 2).ToUpper());
                        var startIndex14 = startIndex13 + 2;
                        var str40 = TagDataStr.Substring(startIndex14, 2);
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex14, 8).ToUpper());
                        IUC285Proxy.Log("CVR Byte 1 - Length = " + str40);
                        var startIndex15 = startIndex14 + 2;
                        var int32 = Convert.ToInt32(str40);
                        var index = 0;
                        var strArray5 = new string[int32];
                        var numArray = new int[int32];
                        var strArray6 = new string[int32];
                        for (; index < int32 && startIndex15 + 2 <= TagDataStr.Length; startIndex15 += 2)
                        {
                            strArray5[index] = TagDataStr.Substring(startIndex15, 2);
                            numArray[index] = Convert.ToInt32(strArray5[index], 16);
                            strArray6[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("-----");
                        if (int32 == 0)
                        {
                            IUC285Proxy.Log("No Additional CVR bits");
                        }
                        else
                        {
                            IUC285Proxy.Log("CVR Byte 2: " + strArray6[0]);
                            var str41 = strArray6[0].Substring(0, 2);
                            switch (str41)
                            {
                                case "00":
                                    IUC285Proxy.Log("    bit 8-7 = " + str41 +
                                                    " - AC returned in 2nd GENERATE AC: AAC");
                                    break;
                                case "01":
                                    IUC285Proxy.Log("    bit 8-7 = " + str41 + " - AC returned in 2nd GENERATE AC: TC");
                                    break;
                                case "10":
                                    IUC285Proxy.Log(
                                        "    bit 8-7 = " + str41 + " - AC returned in 2nd GENERATE AC: ARQC");
                                    break;
                                case "11":
                                    IUC285Proxy.Log("    bit 8-7 = " + str41 +
                                                    " - AC returned in 2nd GENERATE AC: RFU");
                                    break;
                            }

                            var str42 = strArray6[0].Substring(2, 2);
                            if (str42 == "00")
                                IUC285Proxy.Log("    bit 6-5 = " + str42 + " - AC returned in 1st GENERATE AC: AAC");
                            if (str42 == "01")
                                IUC285Proxy.Log("    bit 6-5 = " + str42 + " - AC returned in 1st GENERATE AC: TC");
                            if (str42 == "10")
                                IUC285Proxy.Log("    bit 6-5 = " + str42 + " - AC returned in 1st GENERATE AC: ARQC");
                            if (str42 == "11")
                                IUC285Proxy.Log("    bit 6-5 = " + str42 + " - AC returned in 1st GENERATE AC: RFU");
                            var str43 = strArray6[0].Substring(4, 1);
                            if (str43 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str43 +
                                                "  - Issuer Authentication successful or not performed");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str43 + "  - Issuer Authentication failed");
                            var str44 = strArray6[0].Substring(5, 1);
                            if (str44 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str44 +
                                                "  - Offline PIN verification not performed");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str44 + "  - Offline PIN verification performed");
                            var str45 = strArray6[0].Substring(6, 1);
                            if (str45 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str45 +
                                                "  - Offline PIN verification passed or not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str45 + "  - Offline PIN verification failed");
                            var str46 = strArray6[0].Substring(7, 1);
                            if (str46 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str46 +
                                                "  - Able to go online or offline transaction");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str46 + "  - Unable to go online");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 3: " + strArray6[1]);
                            var str47 = strArray6[1].Substring(0, 1);
                            if (str47 == "0")
                                IUC285Proxy.Log("    bit 8   = " + str47 + " - Last online transaction completed");
                            else
                                IUC285Proxy.Log("    bit 8   = " + str47 + " - Last online transaction not completed");
                            var str48 = strArray6[1].Substring(1, 1);
                            if (str48 == "0")
                                IUC285Proxy.Log("    bit 7   = " + str48 + " - PIN try limit not exceeded");
                            else
                                IUC285Proxy.Log("    bit 7   = " + str48 + " - PIN try limit exceeded");
                            var str49 = strArray6[1].Substring(2, 1);
                            if (str49 == "0")
                                IUC285Proxy.Log("    bit 6   = " + str49 +
                                                " - Velocity checking counters not exceeded");
                            else
                                IUC285Proxy.Log("    bit 6   = " + str49 + " - Velocity checking counters exceeded");
                            var str50 = strArray6[1].Substring(3, 1);
                            if (str50 == "0")
                                IUC285Proxy.Log("    bit 5   = " + str50 + " - Not a new card");
                            else
                                IUC285Proxy.Log("    bit 5   = " + str50 + " - New card");
                            var str51 = strArray6[1].Substring(4, 1);
                            if (str51 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str51 +
                                                " - Issuer Authentication successful on last online transaction or not performed");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str51 +
                                                " - Issuer Authentication failed on last online transaction");
                            var str52 = strArray6[1].Substring(5, 1);
                            if (str52 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str52 +
                                                " - Issuer Authentication performed after online authorization or offline transaction");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str52 +
                                                " - Issuer Authentication not performed after online authorization or offline transaction");
                            var str53 = strArray6[1].Substring(6, 1);
                            if (str53 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str53 + " - Application not blocked by card");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str53 + " - Application blocked by card");
                            var str54 = strArray6[1].Substring(7, 1);
                            if (str54 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str54 +
                                                " - Offline static data authentication passed or was not performed on last transaction");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str54 +
                                                " - Offline static data authentication failed on last transaction or transaction declined offline");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 4: " + strArray6[2]);
                            IUC285Proxy.Log("    bit 8-5 = " + strArray6[2].Substring(0, 4) +
                                            " - Number of Issuer Script Commands");
                            var str55 = strArray6[2].Substring(4, 1);
                            if (str55 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str55 + "    - Issuer Script processing passed");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str55 + "    - Issuer Script processing failed");
                            var str56 = strArray6[2].Substring(5, 1);
                            if (str56 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str56 +
                                                "    - Offline dynamic data authentication passed or was not performed on last transaction");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str56 +
                                                "    - Offline dynamic data authentication failed on last transaction or transaction declined offline");
                            var str57 = strArray6[2].Substring(6, 1);
                            if (str57 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str57 +
                                                "    - Offline dynamic data authentication not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str57 +
                                                "    - Offline dynamic data authentication performed");
                            var str58 = strArray6[2].Substring(7, 1);
                            if (str58 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str58 +
                                                "    - PIN verification command received for a PIN-Expecting card or card does not expect PIN (i.e. Offline PIN verification not supported)");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str58 +
                                                "    - PIN verification command NOT received for a PIN-Expecting card");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("----------------------------------------------------");
                        }
                    }
                    else if (CardBrand.Equals(CardBrandEnum.Discover))
                    {
                        IUC285Proxy.Log("------Issuer Application Data(T9F10) " + CardBrand + " ------");
                        var startIndex16 = 0;
                        IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + TagDataStr.Substring(startIndex16, 2));
                        var startIndex17 = startIndex16 + 2;
                        IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " +
                                        TagDataStr.Substring(startIndex17, 2));
                        var startIndex18 = startIndex17 + 2;
                        var index = 0;
                        var strArray7 = new string[6];
                        var numArray = new int[6];
                        var strArray8 = new string[6];
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex18, 12).ToUpper());
                        for (; index < 6 && startIndex18 + 2 <= TagDataStr.Length; startIndex18 += 2)
                        {
                            strArray7[index] = TagDataStr.Substring(startIndex18, 2);
                            numArray[index] = Convert.ToInt32(strArray7[index], 16);
                            strArray8[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("CVR Byte 1: " + strArray8[0]);
                        var str59 = strArray8[0].Substring(0, 2);
                        switch (str59)
                        {
                            case "00":
                                IUC285Proxy.Log("    bit 8-7 = " + str59 + " - AC returned in 2nd GENERATE AC: AAC");
                                break;
                            case "01":
                                IUC285Proxy.Log("    bit 8-7 = " + str59 + " - AC returned in 2nd GENERATE AC: TC");
                                break;
                            case "10":
                                IUC285Proxy.Log("    bit 8-7 = " + str59 + " - AC returned in 2nd GENERATE AC: ARQC");
                                break;
                            case "11":
                                IUC285Proxy.Log("    bit 8-7 = " + str59 + " - AC returned in 2nd GENERATE AC: RFU");
                                break;
                        }

                        var str60 = strArray8[0].Substring(2, 2);
                        if (str60 == "00")
                            IUC285Proxy.Log("    bit 6-5 = " + str60 + " - AC returned in 1st GENERATE AC: AAC");
                        if (str60 == "01")
                            IUC285Proxy.Log("    bit 6-5 = " + str60 + " - AC returned in 1st GENERATE AC: TC");
                        if (str60 == "10")
                            IUC285Proxy.Log("    bit 6-5 = " + str60 + " - AC returned in 1st GENERATE AC: ARQC");
                        if (str60 == "11")
                            IUC285Proxy.Log("    bit 6-5 = " + str60 + " - AC returned in 1st GENERATE AC: RFU");
                        IUC285Proxy.Log("    bit 4-1 = " + strArray8[0].Substring(4, 4) + " - RFU");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 2: " + strArray8[1]);
                        var str61 = strArray8[1].Substring(0, 1);
                        if (str61 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str61 + "  - DDA NOT Returned");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str61 + "  - DDA Returned");
                        var str62 = strArray8[1].Substring(1, 1);
                        if (str62 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str62 + "  - CDA NOT returned in 1st GENERATE AC");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str62 + "  - CDA returned in 1st GENERATE AC");
                        var str63 = strArray8[1].Substring(2, 1);
                        if (str63 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str63 + "  - CDA NOT returned in 2nd GENERATE AC");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str63 + "  - CDA returned in 2nd GENERATE AC");
                        var str64 = strArray8[1].Substring(3, 1);
                        if (str64 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str64 + "  - Off-line PIN Verification NOT performed");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str64 + "  - Off-line PIN Verification performed");
                        var str65 = strArray8[1].Substring(4, 1);
                        if (str65 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str65 +
                                            "  - Off-line Enciphered PIN Verification NOT performed");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str65 +
                                            "  - Off-line Enciphered PIN Verification performed");
                        var str66 = strArray8[1].Substring(5, 1);
                        if (str66 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str66 + "  - Off-line PIN Verification NOT Successful");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str66 + "  - Off-line PIN Verification Successful");
                        var str67 = strArray8[1].Substring(6, 1);
                        if (str67 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str67 +
                                            "  - CAC-default NOT ignored with CAT3 terminal");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str67 + "  - CAC-default ignored with CAT3 terminal");
                        var str68 = strArray8[1].Substring(7, 1);
                        if (str68 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str68 + "  - RFU");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str68 + "  - RFU");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 3: " + strArray8[2]);
                        IUC285Proxy.Log("    bit 8-5 = " + strArray8[2].Substring(0, 4) +
                                        " - Right nibble of Script Counter");
                        IUC285Proxy.Log("    bit 4-1 = " + strArray8[2].Substring(4, 4) +
                                        " - Right nibble of PIN try Counter");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 4: " + strArray8[3]);
                        var str69 = strArray8[3].Substring(0, 1);
                        if (str69 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str69 + "  - Go on-line during previous transaction");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str69 + "  - Go on-line during previous transaction");
                        var str70 = strArray8[3].Substring(1, 1);
                        if (str70 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str70 +
                                            "  - Issuer Authentication performed during previous online transaction");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str70 +
                                            "  - Issuer Authentication not performed during previous online transaction");
                        var str71 = strArray8[3].Substring(2, 1);
                        if (str71 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str71 +
                                            "  - Issuer Authentication NOT failed during previous transaction");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str71 +
                                            "  - Issuer Authentication failed during previous transaction");
                        var str72 = strArray8[3].Substring(3, 1);
                        if (str72 == "0")
                            IUC285Proxy.Log(
                                "    bit 5   = " + str72 + "  - Script NOT received on previous transaction");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str72 + "  - Script received on previous transaction");
                        var str73 = strArray8[3].Substring(4, 1);
                        if (str73 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str73 + "  - Script NOT failed on previous transaction");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str73 + "  - Script failed on previous transaction");
                        var str74 = strArray8[3].Substring(5, 1);
                        if (str74 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str74 + "  - PTH NOT forced on-line");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str74 + "  - PTH forced on-line");
                        var str75 = strArray8[3].Substring(6, 1);
                        if (str75 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str75 + "  - PDOL NOT forced on-line (during GPO)");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str75 + "  - PDOL forced on-line (during GPO)");
                        var str76 = strArray8[3].Substring(7, 1);
                        if (str76 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str76 + "  - PDOL NOT forced decline (during GPO)");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str76 + "  - PDOL forced decline (during GPO)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 5: " + strArray8[4]);
                        var str77 = strArray8[4].Substring(0, 1);
                        if (str77 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str77 + "  - Valid PDOL check");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str77 + "  - invalid PDOL check");
                        var str78 = strArray8[4].Substring(1, 1);
                        if (str78 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str78 + "  - Off-line PIN verification NOT failed");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str78 + "  - Off-line PIN verification failed");
                        var str79 = strArray8[4].Substring(2, 1);
                        if (str79 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str79 + "  - PIN Try Limit not exceeded");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str79 + "  - PIN Try Limit exceeded");
                        var str80 = strArray8[4].Substring(3, 1);
                        if (str80 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str80 +
                                            "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL). Note: May be profile-specific");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str80 +
                                            "  - Lower Consecutive Offline Transaction limit exceeded (LCOL). Note: May be profile-specific");
                        var str81 = strArray8[4].Substring(4, 1);
                        if (str81 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str81 +
                                            "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL). Note: May be profile-specific");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str81 +
                                            "  - Upper Consecutive Offline Transaction limit exceeded (UCOL). Note: May be profile-specific");
                        var str82 = strArray8[4].Substring(5, 1);
                        if (str82 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str82 +
                                            "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA).Note: May be profile-specific");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str82 +
                                            "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA).Note: May be profile-specific");
                        var str83 = strArray8[4].Substring(6, 1);
                        if (str83 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str83 +
                                            "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA). Note: May be profile-specific");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str83 +
                                            "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA). Note: May be profile-specific");
                        var str84 = strArray8[4].Substring(7, 1);
                        if (str84 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str84 +
                                            "  - Single Transaction Amount limit NOT exceeded (STA). Note: May be profile-specific");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str84 +
                                            "  - Single Transaction Amount limit exceeded (STA). Note: May be profile-specific");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 6: " + strArray8[5]);
                        IUC285Proxy.Log("    bit 8-5 = " + strArray8[5].Substring(0, 4) +
                                        " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
                        IUC285Proxy.Log("    bit 4-1 = " + strArray8[5].Substring(4, 4) +
                                        " - Transaction profile Identifier (0000 by default)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("----------------------------------------------------");
                    }
                    else if (CardBrand.Equals(CardBrandEnum.Discover) && generalSourceType == GeneralSourceType.Tap)
                    {
                        IUC285Proxy.Log("-------Issuer Application Data(T9F10) " + CardBrand + " -------");
                        var startIndex19 = 0;
                        IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + TagDataStr.Substring(startIndex19, 2));
                        var startIndex20 = startIndex19 + 2;
                        IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " +
                                        TagDataStr.Substring(startIndex20, 2));
                        var startIndex21 = startIndex20 + 2;
                        var index = 0;
                        var strArray9 = new string[8];
                        var numArray = new int[8];
                        var strArray10 = new string[8];
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex21, 12).ToUpper());
                        for (; index < 8 && startIndex21 + 2 <= TagDataStr.Length; startIndex21 += 2)
                        {
                            strArray9[index] = TagDataStr.Substring(startIndex21, 2);
                            numArray[index] = Convert.ToInt32(strArray9[index], 16);
                            strArray10[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("CVR Byte 1: Card decision " + strArray10[0]);
                        var str85 = strArray10[0].Substring(0, 1);
                        if (str85 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str85 + "  - Online PIN NOT Required");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str85 + "  - Online PIN Required");
                        var str86 = strArray10[0].Substring(1, 1);
                        if (str86 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str86 + "  - Signature NOT Required");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str86 + "  - Signature Required");
                        var str87 = strArray10[0].Substring(2, 2);
                        switch (str87)
                        {
                            case "00":
                                IUC285Proxy.Log("    bit 6-5 = " + str87 + " - AAC");
                                break;
                            case "01":
                                IUC285Proxy.Log("    bit 6-5 = " + str87 + " - TC");
                                break;
                            case "10":
                                IUC285Proxy.Log("    bit 6-5 = " + str87 + " - ARQC");
                                break;
                            case "11":
                                IUC285Proxy.Log("    bit 6-5 = " + str87 + " - RFU");
                                break;
                        }

                        var str88 = strArray10[0].Substring(4, 1);
                        if (str88 == "0")
                            IUC285Proxy.Log("    bit 4 = " + str88 + " - PID limit NOT reached");
                        else
                            IUC285Proxy.Log("    bit 4 = " + str88 + " - PID limit reached");
                        IUC285Proxy.Log("    bit 3-1 = " + strArray10[0].Substring(3, 3) + " - Script counter");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 2: Compared with CAC-CRM and CAC-CVM " + strArray10[1]);
                        var str89 = strArray10[1].Substring(0, 1);
                        if (str89 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str89 + "  - Online cryptogram NOT required");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str89 + "  - Online cryptogram required");
                        var str90 = strArray10[1].Substring(1, 1);
                        if (str90 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str90 +
                                            "  - Transaction type NOT required to be processed online with online PIN CVM(no cashback)");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str90 +
                                            "  - Transaction type required to be processed online with online PIN CVM(cashback)");
                        var str91 = strArray10[1].Substring(2, 1);
                        if (str91 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str91 +
                                            "  - Transaction type NOT required to be processed offline without any CVM(no refund type transaction)");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str91 +
                                            "  - Transaction type required to be processed offline without any CVM(refund type transaction)");
                        var str92 = strArray10[1].Substring(3, 1);
                        if (str92 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str92 + "  - NOT a domestic transaction");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str92 + "  - Domestic transaction");
                        var str93 = strArray10[1].Substring(4, 1);
                        if (str93 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str93 + "  - NOT an International transaction");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str93 + "  - International transaction");
                        var str94 = strArray10[1].Substring(5, 1);
                        if (str94 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str94 +
                                            "  - PIN Try Limit NOT exceeded (Dual-Interface implementation only)");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str94 +
                                            "  - PIN Try Limit exceeded (Dual-Interface implementation only)");
                        var str95 = strArray10[1].Substring(6, 1);
                        if (str95 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str95 +
                                            "  - Confirmation Code Verification performed (Mobile implementation only)");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str95 +
                                            "  - Confirmation Code Verification NOT performed (Mobile implementation only)");
                        var str96 = strArray10[1].Substring(7, 1);
                        if (str96 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str96 +
                                            "  - Confirmation Code Verification passed (Mobile implementation only)");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str96 +
                                            "  - Confirmation Code Verification NOT performed & failed (Mobile implementation only");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 3: CVM related actions " + strArray10[2]);
                        var str97 = strArray10[2].Substring(0, 1);
                        if (str97 == "0")
                            IUC285Proxy.Log("    bit 8 = " + str97 + " - CVM NOT required");
                        else
                            IUC285Proxy.Log("    bit 8 = " + str97 + " - CVM required");
                        var str98 = strArray10[2].Substring(1, 1);
                        if (str98 == "0")
                            IUC285Proxy.Log("    bit 7 = " + str98 + " - RFU");
                        else
                            IUC285Proxy.Log("    bit 7 = " + str98 + " - RFU");
                        var str99 = strArray10[2].Substring(2, 1);
                        if (str99 == "0")
                            IUC285Proxy.Log("    bit 6 = " + str99 +
                                            " - Consecutive CVM Transaction limit 1 NOT exceeded (CVM-Cons 1)");
                        else
                            IUC285Proxy.Log("    bit 6 = " + str99 +
                                            " - Consecutive CVM Transaction limit 1 exceeded (CVM-Cons 1)");
                        var str100 = strArray10[2].Substring(3, 1);
                        if (str100 == "0")
                            IUC285Proxy.Log("    bit 5 = " + str100 +
                                            " - Consecutive CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
                        else
                            IUC285Proxy.Log("    bit 5 = " + str100 +
                                            " - Consecutive CVM Transaction limit 2 exceeded (CVM-Cons 1)");
                        var str101 = strArray10[2].Substring(4, 1);
                        if (str101 == "0")
                            IUC285Proxy.Log("    bit 4 = " + str101 +
                                            " - Cumulative CVM Transaction amount limit 1 NOT exceeded (CVM-Cons 1)");
                        else
                            IUC285Proxy.Log("    bit 4 = " + str101 +
                                            " - Cumulative CVM Transaction amount limit 1 exceeded (CVM-Cons 1)");
                        var str102 = strArray10[2].Substring(5, 1);
                        if (str102 == "0")
                            IUC285Proxy.Log("    bit 3 = " + str102 +
                                            " - Cumulative CVM Transaction limit 2 NOT exceeded (CVM-Cons 1)");
                        else
                            IUC285Proxy.Log("    bit 3 = " + str102 +
                                            " - Cumulative CVM Transaction limit 2 exceeded (CVM-Cons 1)");
                        var str103 = strArray10[2].Substring(6, 1);
                        if (str103 == "0")
                            IUC285Proxy.Log("    bit 2 = " + str103 +
                                            " - CVM Single Transaction Amount limit 1 NOT exceeded (CVM-STA 1)");
                        else
                            IUC285Proxy.Log("    bit 2 = " + str103 +
                                            " - CVM Single Transaction Amount limit 1 exceeded (CVM-STA 1)");
                        var str104 = strArray10[2].Substring(7, 1);
                        if (str104 == "0")
                            IUC285Proxy.Log("    bit 1 = " + str104 +
                                            " - CVM Single Transaction Amount limit 2 NOT exceeded (CVM-STA 1)");
                        else
                            IUC285Proxy.Log("    bit 1 = " + str104 +
                                            " - CVM Single Transaction Amount limit 2 exceeded (CVM-STA 1)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 4: CRM related actions " + strArray10[3]);
                        var str105 = strArray10[3].Substring(0, 1);
                        if (str105 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str105 +
                                            "  - CDA did NOT fail during previous contactless transaction");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str105 +
                                            "  - CDA failed during previous contactless transaction");
                        var str106 = strArray10[3].Substring(1, 1);
                        if (str106 == "0")
                            IUC285Proxy.Log(
                                "    bit 7   = " + str106 + "  - Last contactless transaction was completed");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str106 +
                                            "  - Last contactless transaction was not completed");
                        var str107 = strArray10[3].Substring(2, 1);
                        if (str107 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str107 +
                                            "  - ‘Go on-line next transaction’ was NOT set by contact or contactless application");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str107 +
                                            "  - ‘Go on-line next transaction’ was set by contact or contactless application");
                        var str108 = strArray10[3].Substring(3, 1);
                        if (str108 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str108 +
                                            "  - Issuer Authentication did NOT fail during previous contact or contactless transaction");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str108 +
                                            "  - Issuer Authentication did fail during previous contact or contactless transaction");
                        var str109 = strArray10[3].Substring(4, 1);
                        if (str109 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str109 +
                                            "  - Script did NOT fail on previous contact or contactless transaction");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str109 +
                                            "  - Script did fail on previous contact or contactless transaction");
                        var str110 = strArray10[3].Substring(5, 1);
                        if (str110 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str110 + "  - NO Invalid PDOL check");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str110 + "  - Invalid PDOL check");
                        var str111 = strArray10[3].Substring(6, 1);
                        if (str111 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str111 + "  - PDOL did NOT force online (during GPO)");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str111 + "  - PDOL did force online (during GPO)");
                        var str112 = strArray10[3].Substring(7, 1);
                        if (str112 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str112 + "  - PDOL did NOT force decline (during GPO)");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str112 + "  - PDOL did force decline (during GPO)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 5: CRM related actions " + strArray10[4]);
                        var str113 = strArray10[4].Substring(0, 1);
                        if (str113 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str113 +
                                            "  - Consecutive Contactless Transaction limit NOT exceeded (CL-Cons)");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str113 +
                                            "  - Consecutive Contactless Transaction limit exceeded (CL-Cons)");
                        var str114 = strArray10[4].Substring(1, 1);
                        if (str114 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str114 +
                                            "  - Cumulative Contactless Transaction limit NOT exceeded (CL-Cumul)");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str114 +
                                            "  - Cumulative Contactless Transaction limit exceeded (CL-Cumul)");
                        var str115 = strArray10[4].Substring(2, 1);
                        if (str115 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str115 +
                                            "  - Single Contactless Transaction Amount limit NOT exceeded (CL-STA)");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str115 +
                                            "  - Single Contactless Transaction Amount limit exceeded (CL-STA)");
                        var str116 = strArray10[4].Substring(3, 1);
                        if (str116 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str116 +
                                            "  - Lower Consecutive Offline Transaction limit NOT exceeded (LCOL)");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str116 +
                                            "  - Lower Consecutive Offline Transaction limit exceeded (LCOL)");
                        var str117 = strArray10[4].Substring(4, 1);
                        if (str117 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str117 +
                                            "  - Upper Consecutive Offline Transaction limit NOT exceeded (UCOL)");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str117 +
                                            "  - Upper Consecutive Offline Transaction limit exceeded (UCOL)");
                        var str118 = strArray10[4].Substring(5, 1);
                        if (str118 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str118 +
                                            "  - Lower Cumulative Offline Transaction Amount limit NOT exceeded (LCOA)");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str118 +
                                            "  - Lower Cumulative Offline Transaction Amount limit exceeded (LCOA)");
                        var str119 = strArray10[4].Substring(6, 1);
                        if (str119 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str119 +
                                            "  - Upper Cumulative Offline Transaction Amount limit NOT exceeded (UCOA)");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str119 +
                                            "  - Upper Cumulative Offline Transaction Amount limit exceeded (UCOA)");
                        var str120 = strArray10[4].Substring(7, 1);
                        if (str120 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str120 +
                                            "  - Single Transaction Amount limit NOT exceeded (STA)");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str120 +
                                            "  - Single Transaction Amount limit exceeded (STA)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 6: Info for Issuer " + strArray10[5]);
                        IUC285Proxy.Log("    bit 8-5 = " + strArray10[5].Substring(0, 4) +
                                        " - ID of PDOL-Decline or PDOL-Online check that forced the transaction to be declined or to go online (Only valid if byte 4, bit 2 or 1 of CVR is set)");
                        IUC285Proxy.Log("    bit 4-1 = " + strArray10[5].Substring(4, 4) +
                                        " - Transaction profile Identifier (0000 by default)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 7: TTQ info for Issuer " + strArray10[6]);
                        var str121 = strArray10[6].Substring(0, 1);
                        if (str121 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str121 + "  - EMV contact chip not supported");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str121 + "  - EMV contact chip supported");
                        var str122 = strArray10[6].Substring(1, 1);
                        if (str122 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str122 + "  - Online-capable reader");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str122 + "  - Not an Online-capable reader");
                        var str123 = strArray10[6].Substring(2, 1);
                        if (str123 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str123 + "  - Online PIN NOT supported");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str123 + "  - Online PIN supported");
                        var str124 = strArray10[6].Substring(3, 1);
                        if (str124 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str124 + "  - Signature NOT supported");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str124 + "  - Signature supported");
                        var str125 = strArray10[6].Substring(4, 1);
                        if (str125 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str125 + "  - Issuer update processing NOT supported");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str125 + "  - Issuer update processing supported");
                        var str126 = strArray10[6].Substring(5, 1);
                        if (str126 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str126 + "  - RFU");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str126 + "  - RFU");
                        var str127 = strArray10[6].Substring(6, 1);
                        if (str127 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str127 + "  - RFU");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str127 + "  - RFU");
                        var str128 = strArray10[6].Substring(7, 1);
                        if (str128 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str128 + "  - RFU");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str128 + "  - RFU");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 8: Info for Issuer related to PID " + strArray10[7]);
                        IUC285Proxy.Log("    bit 8-5 = " + strArray10[7].Substring(0, 8) + " - Program ID");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("----------------------------------------------------");
                    }
                    else if (CardBrand.Equals(CardBrandEnum.Mastercard))
                    {
                        IUC285Proxy.Log("-----Issuer Application Data(T9F10) " + CardBrand + " -----");
                        var startIndex22 = 0;
                        IUC285Proxy.Log("Byte 1 - Derivation Key Index = " + TagDataStr.Substring(startIndex22, 2));
                        var startIndex23 = startIndex22 + 2;
                        IUC285Proxy.Log("Byte 2 - Cryptogram Version Number = " +
                                        TagDataStr.Substring(startIndex23, 2));
                        var startIndex24 = startIndex23 + 2;
                        var index = 0;
                        var strArray11 = new string[6];
                        var numArray = new int[6];
                        var strArray12 = new string[6];
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex24, 12).ToUpper());
                        for (; index < 6 && startIndex24 + 2 <= TagDataStr.Length; startIndex24 += 2)
                        {
                            strArray11[index] = TagDataStr.Substring(startIndex24, 2);
                            numArray[index] = Convert.ToInt32(strArray11[index], 16);
                            strArray12[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("CVR Byte 1: " + strArray12[0]);
                        var str129 = strArray12[0].Substring(0, 2);
                        switch (str129)
                        {
                            case "00":
                                IUC285Proxy.Log("    bit 8-7 = " + str129 + " - AC returned in 2nd GENERATE AC: AAC");
                                break;
                            case "01":
                                IUC285Proxy.Log("    bit 8-7 = " + str129 + " - AC returned in 2nd GENERATE AC: TC");
                                break;
                            case "10":
                                IUC285Proxy.Log("    bit 8-7 = " + str129 + " - AC returned in 2nd GENERATE AC: ARQC");
                                break;
                            case "11":
                                IUC285Proxy.Log("    bit 8-7 = " + str129 + " - AC returned in 2nd GENERATE AC: RFU");
                                break;
                        }

                        var str130 = strArray12[0].Substring(2, 2);
                        if (str130 == "00")
                            IUC285Proxy.Log("    bit 6-5 = " + str130 + " - AC returned in 1st GENERATE AC: AAC");
                        if (str130 == "01")
                            IUC285Proxy.Log("    bit 6-5 = " + str130 + " - AC returned in 1st GENERATE AC: TC");
                        if (str130 == "10")
                            IUC285Proxy.Log("    bit 6-5 = " + str130 + " - AC returned in 1st GENERATE AC: ARQC");
                        if (str130 == "11")
                            IUC285Proxy.Log("    bit 6-5 = " + str130 + " - AC returned in 1st GENERATE AC: RFU");
                        IUC285Proxy.Log("    bit 4   = " + strArray12[0].Substring(4, 1) + " - RFU");
                        var str131 = strArray12[0].Substring(5, 1);
                        if (str131 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str131 + " - Offline PIN Verification NOT Performed");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str131 + " - Offline PIN Verification Performed");
                        var str132 = strArray12[0].Substring(6, 1);
                        if (str132 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str132 +
                                            " - M/Chip Select 4: Offline Encrypted PIN Verification not performed, M/Chip Lite 4: Value not allowed");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str132 +
                                            " - M/Chip Select 4: Offline Encrypted PIN Verification performed");
                        var str133 = strArray12[0].Substring(7, 1);
                        if (str133 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str133 + " - Offline PIN Verification failed");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str133 + " - Offline PIN Verification Successful");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 2: " + strArray12[1]);
                        var str134 = strArray12[1].Substring(0, 1);
                        if (str134 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str134 + "  - DDA NOT Returned");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str134 + "  - DDA Returned");
                        var str135 = strArray12[1].Substring(1, 1);
                        if (str135 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str135 + "  - CDA NOT returned in 1st GENERATE AC");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str135 + "  - CDA returned in 1st GENERATE AC");
                        var str136 = strArray12[1].Substring(2, 1);
                        if (str136 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str136 + "  - CDA NOT returned in 2nd GENERATE AC");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str136 + "  - CDA returned in 2nd GENERATE AC");
                        var str137 = strArray12[1].Substring(3, 1);
                        if (str137 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str137 + "  - Issuer Authentication not performed");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str137 + "  - Issuer Authentication performed");
                        var str138 = strArray12[1].Substring(4, 1);
                        if (str138 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str138 +
                                            "  - CIAC-Default not skipped on CAT3 or not required");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str138 + "  - CIAC-Default skipped on CAT3");
                        IUC285Proxy.Log("    bit 3   = " + strArray12[1].Substring(5, 1) + "  - RFU");
                        IUC285Proxy.Log("    bit 2   = " + strArray12[1].Substring(6, 1) + "  - RFU");
                        IUC285Proxy.Log("    bit 1   = " + strArray12[1].Substring(7, 1) + "  - RFU");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 3: " + strArray12[2]);
                        IUC285Proxy.Log("    bit 8-5 = " + strArray12[2].Substring(0, 4) +
                                        " - Right nibble of Script Counter");
                        IUC285Proxy.Log("    bit 4-1 = " + strArray12[2].Substring(4, 4) +
                                        " - Right nibble of PIN try Counter");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 4: " + strArray12[3]);
                        IUC285Proxy.Log("    bit 8   = " + strArray12[3].Substring(0, 1) + "  - RFU");
                        var str139 = strArray12[3].Substring(1, 1);
                        if (str139 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str139 + "  - Unable To Go Online Not Indicated");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str139 + "  - Unable to go online indicated");
                        var str140 = strArray12[3].Substring(2, 1);
                        if (str140 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str140 + "  - Offline PIN Verification Performed");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str140 + "  - Offline PIN Verification not Performed");
                        var str141 = strArray12[3].Substring(3, 1);
                        if (str141 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str141 + "  - No Failure OF Offline PIN Verification");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str141 + "  - Offline PIN Verification failed");
                        var str142 = strArray12[3].Substring(4, 1);
                        if (str142 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str142 + "  - PTL Not Exceeded");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str142 + "  - PTL Exceeded");
                        var str143 = strArray12[3].Substring(5, 1);
                        if (str143 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str143 + "  - Domestic Transaction");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str143 + "  - International Transaction");
                        var str144 = strArray12[3].Substring(6, 1);
                        if (str144 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str144 + "  - International Transaction");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str144 + "  - Domestic Transaction");
                        var str145 = strArray12[3].Substring(7, 1);
                        if (str145 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str145 +
                                            "  - Terminal Does Not Erroneously Consider Offline PIN OK");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str145 +
                                            "  - Terminal Erroneously Consider Offline PIN OK");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 5: " + strArray12[4]);
                        var str146 = strArray12[4].Substring(0, 1);
                        if (str146 == "0")
                            IUC285Proxy.Log("    bit 8   = " + str146 +
                                            "  - Lower Consecutive Offline Limit Not Exceeded");
                        else
                            IUC285Proxy.Log("    bit 8   = " + str146 + "  - Lower Consecutive Offline Limit Exceeded");
                        var str147 = strArray12[4].Substring(1, 1);
                        if (str147 == "0")
                            IUC285Proxy.Log("    bit 7   = " + str147 +
                                            "  - Upper Consecutive Offline Limit Not Exceeded");
                        else
                            IUC285Proxy.Log("    bit 7   = " + str147 + "  - Upper Consecutive Offline Limit Exceeded");
                        var str148 = strArray12[4].Substring(2, 1);
                        if (str148 == "0")
                            IUC285Proxy.Log("    bit 6   = " + str148 +
                                            "  - Lower Cumulative Offline Limit Not Exceeded");
                        else
                            IUC285Proxy.Log("    bit 6   = " + str148 + "  - Lower Cumulative Offline Limit Exceeded");
                        var str149 = strArray12[4].Substring(3, 1);
                        if (str149 == "0")
                            IUC285Proxy.Log("    bit 5   = " + str149 +
                                            "  - Upper Cumulative Offline Limit Not Exceeded");
                        else
                            IUC285Proxy.Log("    bit 5   = " + str149 + "  - Upper Cumulative Offline Limit Exceeded");
                        var str150 = strArray12[4].Substring(4, 1);
                        if (str150 == "0")
                            IUC285Proxy.Log("    bit 4   = " + str150 +
                                            "  - Go Online On Next Transaction Was Not Set (in this transaction or in a previous one)");
                        else
                            IUC285Proxy.Log("    bit 4   = " + str150 +
                                            "  - Go Online On Next Transaction Was Set (in this transaction or in a previous one)");
                        var str151 = strArray12[4].Substring(5, 1);
                        if (str151 == "0")
                            IUC285Proxy.Log("    bit 3   = " + str151 +
                                            "  - No Issuer Authentication Failed (in this transaction or in a previous one)");
                        else
                            IUC285Proxy.Log("    bit 3   = " + str151 +
                                            "  - Issuer Authentication Failed (in this transaction or in a previous one)");
                        var str152 = strArray12[4].Substring(6, 1);
                        if (str152 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str152 +
                                            "  - No Script Received (in a previous transaction)");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str152 +
                                            "  - Script Received (in a previous transaction)");
                        var str153 = strArray12[4].Substring(7, 1);
                        if (str153 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str153 +
                                            "  - No Script Failed (in a previous transaction)");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str153 +
                                            "  - Script Failed (in a previous transaction)");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("CVR Byte 6: " + strArray12[5]);
                        IUC285Proxy.Log("    bit 8   = " + strArray12[5].Substring(0, 1) + " - RFU");
                        IUC285Proxy.Log("    bit 7   = " + strArray12[5].Substring(1, 1) + " - RFU");
                        IUC285Proxy.Log("    bit 6   = " + strArray12[5].Substring(2, 1) + " - RFU");
                        IUC285Proxy.Log("    bit 5   = " + strArray12[5].Substring(3, 1) + " - RFU");
                        IUC285Proxy.Log("    bit 4   = " + strArray12[5].Substring(4, 1) + " - RFU");
                        IUC285Proxy.Log("    bit 3   = " + strArray12[5].Substring(5, 1) + " - RFU");
                        var str154 = strArray12[5].Substring(6, 1);
                        if (str154 == "0")
                            IUC285Proxy.Log("    bit 2   = " + str154 + " - No Match found in Additional Check Table");
                        else
                            IUC285Proxy.Log("    bit 2   = " + str154 + " - Match found in Additional Check Table");
                        var str155 = strArray12[5].Substring(7, 1);
                        if (str155 == "0")
                            IUC285Proxy.Log("    bit 1   = " + str155 + " - Match found in Additional Check Table");
                        else
                            IUC285Proxy.Log("    bit 1   = " + str155 + " - No Match found in Additional Check Table");
                        IUC285Proxy.Log("-----");
                        IUC285Proxy.Log("DAC = " + TagDataStr.Substring(startIndex24, 4));
                        var startIndex25 = startIndex24 + 4;
                        IUC285Proxy.Log("Counters = " + TagDataStr.Substring(startIndex25, 16));
                        var num = startIndex25 + 16;
                        IUC285Proxy.Log("----------------------------------------------------");
                    }
                    else if (CardBrand.Equals(CardBrandEnum.UnionPay))
                    {
                        IUC285Proxy.Log("------Issuer Application Data(T9F10) " + CardBrand + " ------");
                        var startIndex26 = 0;
                        var str156 = TagDataStr.Substring(startIndex26, 2);
                        IUC285Proxy.Log("Byte 1 - Length Indicator = " + str156);
                        var startIndex27 = startIndex26 + 2;
                        IUC285Proxy.Log("Byte 2 - Derivation Key Index = " + TagDataStr.Substring(startIndex27, 2));
                        var startIndex28 = startIndex27 + 2;
                        IUC285Proxy.Log("Byte 3 - Cryptogram Version Number = " +
                                        TagDataStr.Substring(startIndex28, 2));
                        var startIndex29 = startIndex28 + 2;
                        var str157 = TagDataStr.Substring(startIndex29, 2);
                        IUC285Proxy.Log("CVR: " + TagDataStr.Substring(startIndex29, 8).ToUpper());
                        IUC285Proxy.Log("CVR Byte 1 - Length indicator = " + str157);
                        var startIndex30 = startIndex29 + 2;
                        var int32 = Convert.ToInt32(str157);
                        var index = 0;
                        var strArray13 = new string[int32];
                        var numArray = new int[int32];
                        var strArray14 = new string[int32];
                        for (; index < int32 && startIndex30 + 2 <= TagDataStr.Length; startIndex30 += 2)
                        {
                            strArray13[index] = TagDataStr.Substring(startIndex30, 2);
                            numArray[index] = Convert.ToInt32(strArray13[index], 16);
                            strArray14[index] = Convert.ToString(numArray[index], 2).PadLeft(8, '0');
                            ++index;
                        }

                        IUC285Proxy.Log("-----");
                        if (int32 == 0)
                        {
                            IUC285Proxy.Log("No Additional CVR bits");
                        }
                        else
                        {
                            IUC285Proxy.Log("CVR Byte 2: " + strArray14[0]);
                            var str158 = strArray14[0].Substring(0, 2);
                            switch (str158)
                            {
                                case "00":
                                    IUC285Proxy.Log(
                                        "    bit 8-7 = " + str158 + " - AC returned in 2nd GENERATE AC: AAC");
                                    break;
                                case "01":
                                    IUC285Proxy.Log("    bit 8-7 = " + str158 +
                                                    " - AC returned in 2nd GENERATE AC: TC");
                                    break;
                                case "10":
                                    IUC285Proxy.Log("    bit 8-7 = " + str158 +
                                                    " - AC returned in 2nd GENERATE AC: Not requested");
                                    break;
                                case "11":
                                    IUC285Proxy.Log(
                                        "    bit 8-7 = " + str158 + " - AC returned in 2nd GENERATE AC: RFU");
                                    break;
                            }

                            var str159 = strArray14[0].Substring(2, 2);
                            if (str159 == "00")
                                IUC285Proxy.Log("    bit 6-5 = " + str159 + " - AC returned in 1st GENERATE AC: AAC");
                            if (str159 == "01")
                                IUC285Proxy.Log("    bit 6-5 = " + str159 + " - AC returned in 1st GENERATE AC: TC");
                            if (str159 == "10")
                                IUC285Proxy.Log("    bit 6-5 = " + str159 + " - AC returned in 1st GENERATE AC: ARQC");
                            if (str159 == "11")
                                IUC285Proxy.Log("    bit 6-5 = " + str159 + " - AC returned in 1st GENERATE AC: RFU");
                            var str160 = strArray14[0].Substring(4, 1);
                            if (str160 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str160 +
                                                "  - Issuer Authentication performed and passed, or not performed");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str160 + "  - Issuer Authentication failed");
                            var str161 = strArray14[0].Substring(5, 1);
                            if (str161 == "0")
                                IUC285Proxy.Log(
                                    "    bit 3   = " + str161 + "  - Offline PIN verification not performed");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str161 + "  - Offline PIN verification performed");
                            var str162 = strArray14[0].Substring(6, 1);
                            if (str162 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str162 +
                                                "  - Offline PIN verification passed, or not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str162 + "  - Offline PIN verification failed");
                            var str163 = strArray14[0].Substring(7, 1);
                            if (str163 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str163 +
                                                "  - Able to go online, or transaction performed offline");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str163 + "  - Unable to go online");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 3: " + strArray14[1]);
                            var str164 = strArray14[1].Substring(0, 1);
                            if (str164 == "0")
                                IUC285Proxy.Log("    bit 8   = " + str164 + " - Last online transaction completed");
                            else
                                IUC285Proxy.Log("    bit 8   = " + str164 + " - Last online transaction not completed");
                            var str165 = strArray14[1].Substring(1, 1);
                            if (str165 == "0")
                                IUC285Proxy.Log("    bit 7   = " + str165 + " - PIN try limit not exceeded");
                            else
                                IUC285Proxy.Log("    bit 7   = " + str165 + " - PIN try limit exceeded");
                            var str166 = strArray14[1].Substring(2, 1);
                            if (str166 == "0")
                                IUC285Proxy.Log(
                                    "    bit 6   = " + str166 + " - Velocity checking counters not exceeded");
                            else
                                IUC285Proxy.Log("    bit 6   = " + str166 + " - Velocity checking counters exceeded");
                            var str167 = strArray14[1].Substring(3, 1);
                            if (str167 == "0")
                                IUC285Proxy.Log("    bit 5   = " + str167 + " - Not a new card");
                            else
                                IUC285Proxy.Log("    bit 5   = " + str167 + " - New card");
                            var str168 = strArray14[1].Substring(4, 1);
                            if (str168 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str168 +
                                                " - Issuer Authentication did not fail on last online transaction");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str168 +
                                                " - Issuer Authentication failed on last online transaction");
                            var str169 = strArray14[1].Substring(5, 1);
                            if (str169 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str169 +
                                                " - Issuer Authentication performed after online authorization, or transaction was offline");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str169 +
                                                " - Issuer Authentication was not performed after online authorization");
                            var str170 = strArray14[1].Substring(6, 1);
                            if (str170 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str170 +
                                                " - Application not blocked by card because PIN Try Limit exceeded");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str170 +
                                                " - Application blocked by card because PIN Try Limit exceeded");
                            var str171 = strArray14[1].Substring(7, 1);
                            if (str171 == "0")
                                IUC285Proxy.Log("    bit 1   = " + str171 +
                                                " - Offline static data authentication passed on last transaction, or SDA not performed");
                            else
                                IUC285Proxy.Log("    bit 1   = " + str171 +
                                                " - Offline static data authentication failed on last transaction");
                            IUC285Proxy.Log("-----");
                            IUC285Proxy.Log("CVR Byte 4: " + strArray14[2]);
                            IUC285Proxy.Log("    bit 8-5 = " + strArray14[2].Substring(0, 4) +
                                            " - Number of Issuer Script Commands");
                            var str172 = strArray14[2].Substring(4, 1);
                            if (str172 == "0")
                                IUC285Proxy.Log("    bit 4   = " + str172 + "    - Issuer Script processing passed");
                            else
                                IUC285Proxy.Log("    bit 4   = " + str172 + "    - Issuer Script processing failed");
                            var str173 = strArray14[2].Substring(5, 1);
                            if (str173 == "0")
                                IUC285Proxy.Log("    bit 3   = " + str173 +
                                                "    - Offline dynamic data authentication passed on last transaction, or DDA not performed");
                            else
                                IUC285Proxy.Log("    bit 3   = " + str173 +
                                                "    - Offline dynamic data authentication failed on last transaction");
                            var str174 = strArray14[2].Substring(6, 1);
                            if (str174 == "0")
                                IUC285Proxy.Log("    bit 2   = " + str174 +
                                                "    - Offline dynamic data authentication not performed");
                            else
                                IUC285Proxy.Log("    bit 2   = " + str174 +
                                                "    - Offline dynamic data authentication performed");
                            IUC285Proxy.Log("    bit 1   = " + strArray14[2].Substring(7, 1) + " -    RFU");
                            IUC285Proxy.Log("-----");
                            TagDataStr.Substring(startIndex30, 2);
                            IUC285Proxy.Log("Byte 8 - Algorithm Indicator = " + str156);
                            var num = startIndex30 + 2;
                            IUC285Proxy.Log("----------------------------------------------------");
                        }
                    }
                    else
                    {
                        IUC285Proxy.Log("-----Issuer Application Data(T9F10) " + CardBrand + " AID-----");
                        IUC285Proxy.Log("Unknown card brand, please include the tag for AID");
                        IUC285Proxy.Log("----------------------------------------------------");
                    }
                }
                else if (Tag == "82")
                {
                    IUC285Proxy.Log("--------Application Interchange Profile(T82)--------");
                    var str175 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str176 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str175);
                    IUC285Proxy.Log("  " + str175[0] + " - RFU");
                    IUC285Proxy.Log("  " + str175[1] + " - SDA supported");
                    IUC285Proxy.Log("  " + str175[2] + " - DDA supported");
                    IUC285Proxy.Log("  " + str175[3] + " - Cardholder verification is supported");
                    IUC285Proxy.Log("  " + str175[4] + " - Terminal risk management is to be performed");
                    var ch = str175[5];
                    IUC285Proxy.Log("  " + ch + " - Issuer authentication is supported");
                    ch = str175[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str175[7];
                    IUC285Proxy.Log("  " + ch + " - CDA supported");
                    IUC285Proxy.Log("Byte 2: " + str176);
                    ch = str176[0];
                    IUC285Proxy.Log("  " + ch + " - Reserved for EMV contactless spec");
                    ch = str176[1];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[2];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[3];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str176[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "8E")
                {
                    IUC285Proxy.Log("------------Cardholder Verification(T8E)------------");
                    IUC285Proxy.Log("Amount X: " + TagDataByte[0].ToString("X2") + TagDataByte[1].ToString("X2") +
                                    TagDataByte[2].ToString("X2") + TagDataByte[3].ToString("X2"));
                    IUC285Proxy.Log("Amount Y: " + TagDataByte[4].ToString("X2") + TagDataByte[5].ToString("X2") +
                                    TagDataByte[6].ToString("X2") + TagDataByte[7].ToString("X2"));
                    var strArray = new string[(TagLength - 8) / 2];
                    var index1 = 0;
                    var index2 = 8;
                    while (index2 < TagLength)
                    {
                        strArray[index1] = Convert.ToString(TagDataByte[index2], 2).PadLeft(8, '0');
                        IUC285Proxy.Log("-----CV Rule " + (index1 + 1) + ": " + TagDataByte[index2].ToString("X2") +
                                        TagDataByte[index2 + 1].ToString("X2"));
                        IUC285Proxy.Log("CV Rule Byte 1: " + strArray[index1].Substring(0, 8));
                        IUC285Proxy.Log("  " + strArray[index1].Substring(0, 1) + "      - RFU");
                        var str177 = strArray[index1].Substring(1, 1);
                        if (str177 == "0")
                            IUC285Proxy.Log("  " + str177 +
                                            "      - Fail cardholder verification if this is unsuccessful");
                        else
                            IUC285Proxy.Log("  " + str177 + "      - Apply succeeding CV Rule if this is unsuccessful");
                        switch (strArray[index1].Substring(2, 6))
                        {
                            case "000000":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) + " - Fail CVM Processing");
                                break;
                            case "000001":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) +
                                                " - Plaintext PIN verification performed by ICC");
                                break;
                            case "000010":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) +
                                                " - Enciphered PIN verified online");
                                break;
                            case "000011":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) +
                                                " - Plaintext PIN verification performed by ICC and signature");
                                break;
                            case "000100":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) +
                                                " - Enciphered PIN verification performed by ICC");
                                break;
                            case "000101":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) +
                                                " - Enciphered PIN verification performed by ICC and signature");
                                break;
                            case "011110":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) + " - Signature");
                                break;
                            case "011111":
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) + " - No CVM");
                                break;
                            default:
                                IUC285Proxy.Log("  " + strArray[index1].Substring(2, 6) + " - Reserved");
                                break;
                        }

                        var str178 = TagDataByte[index2 + 1].ToString("X2");
                        IUC285Proxy.Log("CV Rule Byte 2: " + str178);
                        switch (str178)
                        {
                            case "00":
                                IUC285Proxy.Log("  00     - Always");
                                break;
                            case "01":
                                IUC285Proxy.Log("  01     - If unattended cash");
                                break;
                            case "02":
                                IUC285Proxy.Log(
                                    "  02     - If not unattended cash and not manual cash and not purchase with cashback");
                                break;
                            case "03":
                                IUC285Proxy.Log("  03     - If terminal supports the CVM");
                                break;
                            case "04":
                                IUC285Proxy.Log("  04     - If manual cash");
                                break;
                            case "05":
                                IUC285Proxy.Log("  05     - If purchase with cashback");
                                break;
                            case "06":
                                IUC285Proxy.Log(
                                    "  06     - If transaction is in the application currency and is under X value");
                                break;
                            case "07":
                                IUC285Proxy.Log(
                                    "  07     - If transaction is in the application currency and is over X value");
                                break;
                            case "08":
                                IUC285Proxy.Log(
                                    "  08     - If transaction is in the application currency and is under Y value");
                                break;
                            case "09":
                                IUC285Proxy.Log(
                                    "  09     - If transaction is in the application currency and is over Y value");
                                break;
                            default:
                                IUC285Proxy.Log("  " + str178 + " - RFU");
                                break;
                        }

                        index2 += 2;
                        ++index1;
                    }

                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "95")
                {
                    IUC285Proxy.Log("---------Terminal Verification Results(T95)---------");
                    var str179 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str180 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    var str181 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
                    var str182 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
                    var str183 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str179);
                    IUC285Proxy.Log("  " + str179[0] + " - Offline data authentication was not performed");
                    var ch = str179[1];
                    IUC285Proxy.Log("  " + ch + " - SDA failed");
                    ch = str179[2];
                    IUC285Proxy.Log("  " + ch + " - ICC data missing");
                    ch = str179[3];
                    IUC285Proxy.Log("  " + ch + " - Card appears on terminal exception file");
                    ch = str179[4];
                    IUC285Proxy.Log("  " + ch + " - DDA failed");
                    ch = str179[5];
                    IUC285Proxy.Log("  " + ch + " - CDA failed");
                    ch = str179[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str179[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 2: " + str180);
                    ch = str180[0];
                    IUC285Proxy.Log("  " + ch + " - ICC and terminal have different application versions");
                    ch = str180[1];
                    IUC285Proxy.Log("  " + ch + " - Expired application");
                    ch = str180[2];
                    IUC285Proxy.Log("  " + ch + " - Application not yet effective");
                    ch = str180[3];
                    IUC285Proxy.Log("  " + ch + " - Requested service not allowed for card product");
                    ch = str180[4];
                    IUC285Proxy.Log("  " + ch + " - New card");
                    ch = str180[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str180[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str180[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 3: " + str181);
                    ch = str181[0];
                    IUC285Proxy.Log("  " + ch + " - Cardholder verification was not successful");
                    ch = str181[1];
                    IUC285Proxy.Log("  " + ch + " - Unrecognised CVM");
                    ch = str181[2];
                    IUC285Proxy.Log("  " + ch + " - PIN Try Limit exceeded");
                    ch = str181[3];
                    IUC285Proxy.Log("  " + ch + " - PIN entry required and PIN pad not present or not working");
                    ch = str181[4];
                    IUC285Proxy.Log("  " + ch + " - PIN entry required, PIN pad present, but PIN was not entered");
                    ch = str181[5];
                    IUC285Proxy.Log("  " + ch + " - Online PIN entered");
                    ch = str181[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str181[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 4: " + str182);
                    ch = str182[0];
                    IUC285Proxy.Log("  " + ch + " - Transaction exceeds floor limit");
                    ch = str182[1];
                    IUC285Proxy.Log("  " + ch + " - Lower consecutive offline limit exceeded");
                    ch = str182[2];
                    IUC285Proxy.Log("  " + ch + " - Upper consecutive offline limit exceeded");
                    ch = str182[3];
                    IUC285Proxy.Log("  " + ch + " - Transaction selected randomly for online processing");
                    ch = str182[4];
                    IUC285Proxy.Log("  " + ch + " - Merchant forced transaction online");
                    ch = str182[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str182[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str182[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 5: " + str183);
                    ch = str183[0];
                    IUC285Proxy.Log("  " + ch + " - Default TDOL used");
                    ch = str183[1];
                    IUC285Proxy.Log("  " + ch + " - Issuer authentication failed");
                    ch = str183[2];
                    IUC285Proxy.Log("  " + ch + " - Script processing failed before final GENERATE AC");
                    ch = str183[3];
                    IUC285Proxy.Log("  " + ch + " - Script processing failed after final GENERATE AC");
                    ch = str183[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str183[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str183[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str183[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F07")
                {
                    IUC285Proxy.Log("----------Application Usage Control(T9F07)----------");
                    var str184 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str185 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str184);
                    IUC285Proxy.Log("  " + str184[0] + " - Valid for domestic cash transaction");
                    IUC285Proxy.Log("  " + str184[1] + " - Valid for international cash transaction");
                    IUC285Proxy.Log("  " + str184[2] + " - Valid for domestic goods");
                    IUC285Proxy.Log("  " + str184[3] + " - Valid for international goods");
                    IUC285Proxy.Log("  " + str184[4] + " - Valid for domestic services");
                    var ch = str184[5];
                    IUC285Proxy.Log("  " + ch + " - Valid for international services");
                    ch = str184[6];
                    IUC285Proxy.Log("  " + ch + " - Valid at ATMs");
                    ch = str184[7];
                    IUC285Proxy.Log("  " + ch + " - Valid at terminals other than ATMs");
                    IUC285Proxy.Log("Byte 2: " + str185);
                    ch = str185[0];
                    IUC285Proxy.Log("  " + ch + " - Domestic cashback allowed");
                    ch = str185[1];
                    IUC285Proxy.Log("  " + ch + " - International cashback allowed");
                    ch = str185[2];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str185[3];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str185[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str185[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str185[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str185[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F0D")
                {
                    IUC285Proxy.Log("---------Issuer Action Code Default (T9F0D)---------");
                    var str186 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str187 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    var str188 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
                    var str189 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
                    var str190 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str186);
                    var ch = str186[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Offline data authentication was not performed");
                    }
                    else
                    {
                        ch = str186[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Offline data authentication was not performed");
                    }

                    ch = str186[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Offline static data authentication failed");
                    }
                    else
                    {
                        ch = str186[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Offline static data authentication failed");
                    }

                    ch = str186[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if ICC data missing");
                    }
                    else
                    {
                        ch = str186[2];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if ICC data missing");
                    }

                    ch = str186[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Card appears on terminal exception file");
                    }
                    else
                    {
                        ch = str186[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Card appears on terminal exception file");
                    }

                    ch = str186[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Offline dynamic data authentication failed");
                    }
                    else
                    {
                        ch = str186[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Offline dynamic data authentication failed");
                    }

                    ch = str186[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[5];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Combined DDA/AC Generation failed");
                    }
                    else
                    {
                        ch = str186[5];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Combined DDA/AC Generation failed");
                    }

                    ch = str186[6];
                    if (ch.ToString() == "0")
                    {
                        ch = str186[6];
                        IUC285Proxy.Log("  " + ch + " - Do not reject if unable to process online and if SDA selected");
                    }
                    else
                    {
                        ch = str186[6];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if SDA selected");
                    }

                    ch = str186[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 2: " + str187);
                    ch = str187[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str187[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if ICC and terminal have different application versions");
                    }
                    else
                    {
                        ch = str187[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if ICC and terminal have different application versions");
                    }

                    ch = str187[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str187[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Expired application");
                    }
                    else
                    {
                        ch = str187[1];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if Expired application");
                    }

                    ch = str187[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str187[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Application not yet effective");
                    }
                    else
                    {
                        ch = str187[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Application not yet effective");
                    }

                    ch = str187[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str187[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Requested service not allowed for card product");
                    }
                    else
                    {
                        ch = str187[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Requested service not allowed for card product");
                    }

                    ch = str187[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str187[4];
                        IUC285Proxy.Log("  " + ch + " - Do not reject if unable to process online and if New card");
                    }
                    else
                    {
                        ch = str187[4];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if New card");
                    }

                    ch = str187[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str187[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str187[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 3: " + str188);
                    ch = str188[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Cardholder verification was not successful");
                    }
                    else
                    {
                        ch = str188[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Cardholder verification was not successful");
                    }

                    ch = str188[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Unrecognised CVM");
                    }
                    else
                    {
                        ch = str188[1];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if Unrecognised CVM");
                    }

                    ch = str188[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if PIN Try Limit exceeded");
                    }
                    else
                    {
                        ch = str188[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if PIN Try Limit exceeded");
                    }

                    ch = str188[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if PIN entry required and PIN pad not present or not working");
                    }
                    else
                    {
                        ch = str188[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if PIN entry required and PIN pad not present or not working");
                    }

                    ch = str188[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
                    }
                    else
                    {
                        ch = str188[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if PIN entry required, PIN pad present, but PIN was not entered");
                    }

                    ch = str188[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str188[5];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Online PIN entered");
                    }
                    else
                    {
                        ch = str188[5];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if Online PIN entered");
                    }

                    ch = str188[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str188[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 4: " + str189);
                    ch = str189[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str189[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Transaction exceeds floor limit");
                    }
                    else
                    {
                        ch = str189[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Transaction exceeds floor limit");
                    }

                    ch = str189[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str189[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Lower consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str189[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Lower consecutive offline limit exceeded");
                    }

                    ch = str189[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str189[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Upper consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str189[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Upper consecutive offline limit exceeded");
                    }

                    ch = str189[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str189[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Transaction selected randomly for online processing");
                    }
                    else
                    {
                        ch = str189[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Transaction selected randomly for online processing");
                    }

                    ch = str189[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str189[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Merchant forced transaction online");
                    }
                    else
                    {
                        ch = str189[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Merchant forced transaction online");
                    }

                    ch = str189[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str189[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str189[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 5: " + str190);
                    ch = str190[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str190[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Default TDOL used");
                    }
                    else
                    {
                        ch = str190[0];
                        IUC285Proxy.Log("  " + ch + " - Reject if unable to process online and if Default TDOL used");
                    }

                    ch = str190[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str190[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Issuer authentication was unsuccessful");
                    }
                    else
                    {
                        ch = str190[1];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Issuer authentication was unsuccessful");
                    }

                    ch = str190[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str190[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Script processing failed before final GENERATE AC");
                    }
                    else
                    {
                        ch = str190[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Script processing failed before final GENERATE AC");
                    }

                    ch = str190[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str190[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not reject if unable to process online and if Script processing failed after final GENERATE AC");
                    }
                    else
                    {
                        ch = str190[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Reject if unable to process online and if Script processing failed after final GENERATE AC");
                    }

                    ch = str190[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str190[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str190[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str190[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F0E")
                {
                    IUC285Proxy.Log("----------Issuer Action Code Denial(T9F0E)----------");
                    var str191 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str192 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    var str193 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
                    var str194 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
                    var str195 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str191);
                    var ch = str191[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[0];
                        IUC285Proxy.Log(
                            "  " + ch + " - Do not decline if Offline data authentication was not performed");
                    }
                    else
                    {
                        ch = str191[0];
                        IUC285Proxy.Log("  " + ch + " - Decline if Offline data authentication was not performed");
                    }

                    ch = str191[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[1];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Offline static data authentication failed");
                    }
                    else
                    {
                        ch = str191[1];
                        IUC285Proxy.Log("  " + ch + " - Decline if Offline static data authentication failed");
                    }

                    ch = str191[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[2];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if ICC data missing");
                    }
                    else
                    {
                        ch = str191[2];
                        IUC285Proxy.Log("  " + ch + " - Decline if ICC data missing");
                    }

                    ch = str191[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[3];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Card appears on terminal exception file");
                    }
                    else
                    {
                        ch = str191[3];
                        IUC285Proxy.Log("  " + ch + " - Decline if Card appears on terminal exception file");
                    }

                    ch = str191[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[4];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Offline dynamic data authentication failed");
                    }
                    else
                    {
                        ch = str191[4];
                        IUC285Proxy.Log("  " + ch + " - Decline if Offline dynamic data authentication failed");
                    }

                    ch = str191[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[5];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Combined DDA/AC Generation failed");
                    }
                    else
                    {
                        ch = str191[5];
                        IUC285Proxy.Log("  " + ch + " - Decline if Combined DDA/AC Generation failed");
                    }

                    ch = str191[6];
                    if (ch.ToString() == "0")
                    {
                        ch = str191[6];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if SDA selected");
                    }
                    else
                    {
                        ch = str191[6];
                        IUC285Proxy.Log("  " + ch + " - Decline if SDA selected");
                    }

                    ch = str191[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 2: " + str192);
                    ch = str192[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str192[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if ICC and terminal have different application versions");
                    }
                    else
                    {
                        ch = str192[0];
                        IUC285Proxy.Log(
                            "  " + ch + " - Decline if ICC and terminal have different application versions");
                    }

                    ch = str192[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str192[1];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Expired application");
                    }
                    else
                    {
                        ch = str192[1];
                        IUC285Proxy.Log("  " + ch + " - Decline if Expired application");
                    }

                    ch = str192[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str192[2];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Application not yet effective");
                    }
                    else
                    {
                        ch = str192[2];
                        IUC285Proxy.Log("  " + ch + " - Decline if Application not yet effective");
                    }

                    ch = str192[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str192[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if Requested service not allowed for card product");
                    }
                    else
                    {
                        ch = str192[3];
                        IUC285Proxy.Log("  " + ch + " - Decline if Requested service not allowed for card product");
                    }

                    ch = str192[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str192[4];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if New card");
                    }
                    else
                    {
                        ch = str192[4];
                        IUC285Proxy.Log("  " + ch + " - Decline if New card");
                    }

                    ch = str192[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str192[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str192[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 3: " + str193);
                    ch = str193[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[0];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Cardholder verification was not successful");
                    }
                    else
                    {
                        ch = str193[0];
                        IUC285Proxy.Log("  " + ch + " - Decline if Cardholder verification was not successful");
                    }

                    ch = str193[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[1];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Unrecognised CVM");
                    }
                    else
                    {
                        ch = str193[1];
                        IUC285Proxy.Log("  " + ch + " - Decline if Unrecognised CVM");
                    }

                    ch = str193[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[2];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if PIN Try Limit exceeded");
                    }
                    else
                    {
                        ch = str193[2];
                        IUC285Proxy.Log("  " + ch + " - Decline if PIN Try Limit exceeded");
                    }

                    ch = str193[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if PIN entry required and PIN pad not present or not working");
                    }
                    else
                    {
                        ch = str193[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Decline if PIN entry required and PIN pad not present or not working");
                    }

                    ch = str193[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if PIN entry required, PIN pad present, but PIN was not entered");
                    }
                    else
                    {
                        ch = str193[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Decline if PIN entry required, PIN pad present, but PIN was not entered");
                    }

                    ch = str193[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str193[5];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Online PIN entered");
                    }
                    else
                    {
                        ch = str193[5];
                        IUC285Proxy.Log("  " + ch + " - Decline if Online PIN entered");
                    }

                    ch = str193[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str193[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 4: " + str194);
                    ch = str194[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str194[0];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Transaction exceeds floor limit");
                    }
                    else
                    {
                        ch = str194[0];
                        IUC285Proxy.Log("  " + ch + " - Decline if Transaction exceeds floor limit");
                    }

                    ch = str194[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str194[1];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Lower consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str194[1];
                        IUC285Proxy.Log("  " + ch + " - Decline if Lower consecutive offline limit exceeded");
                    }

                    ch = str194[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str194[2];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Upper consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str194[2];
                        IUC285Proxy.Log("  " + ch + " - Decline if Upper consecutive offline limit exceeded");
                    }

                    ch = str194[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str194[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if Transaction selected randomly for online processing");
                    }
                    else
                    {
                        ch = str194[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Decline if Transaction selected randomly for online processing");
                    }

                    ch = str194[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str194[4];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Merchant forced transaction online");
                    }
                    else
                    {
                        ch = str194[4];
                        IUC285Proxy.Log("  " + ch + " - Decline if Merchant forced transaction online");
                    }

                    ch = str194[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str194[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str194[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 5: " + str195);
                    ch = str195[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str195[0];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Default TDOL used");
                    }
                    else
                    {
                        ch = str195[0];
                        IUC285Proxy.Log("  " + ch + " - Decline if Default TDOL used");
                    }

                    ch = str195[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str195[1];
                        IUC285Proxy.Log("  " + ch + " - Do not decline if Issuer authentication was unsuccessful");
                    }
                    else
                    {
                        ch = str195[1];
                        IUC285Proxy.Log("  " + ch + " - Decline if Issuer authentication was unsuccessful");
                    }

                    ch = str195[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str195[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if Script processing failed before final GENERATE AC");
                    }
                    else
                    {
                        ch = str195[2];
                        IUC285Proxy.Log("  " + ch + " - Decline if Script processing failed before final GENERATE AC");
                    }

                    ch = str195[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str195[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not decline if Script processing failed after final GENERATE AC");
                    }
                    else
                    {
                        ch = str195[3];
                        IUC285Proxy.Log("  " + ch + " - Decline if Script processing failed after final GENERATE AC");
                    }

                    ch = str195[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str195[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str195[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str195[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F0F")
                {
                    IUC285Proxy.Log("----------Issuer Action Code Online(T9F0F)----------");
                    var str196 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str197 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    var str198 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
                    var str199 = Convert.ToString(TagDataByte[3], 2).PadLeft(8, '0');
                    var str200 = Convert.ToString(TagDataByte[4], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str196);
                    var ch = str196[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Offline data authentication was not performed");
                    }
                    else
                    {
                        ch = str196[0];
                        IUC285Proxy.Log("  " + ch + " - Go online if Offline data authentication was not performed");
                    }

                    ch = str196[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[1];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Offline static data authentication failed");
                    }
                    else
                    {
                        ch = str196[1];
                        IUC285Proxy.Log("  " + ch + " - Go online if Offline static data authentication failed");
                    }

                    ch = str196[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[2];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if ICC data missing");
                    }
                    else
                    {
                        ch = str196[2];
                        IUC285Proxy.Log("  " + ch + " - Go online if ICC data missing");
                    }

                    ch = str196[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[3];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Card appears on terminal exception file");
                    }
                    else
                    {
                        ch = str196[3];
                        IUC285Proxy.Log("  " + ch + " - Go online if Card appears on terminal exception file");
                    }

                    ch = str196[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Offline dynamic data authentication failed");
                    }
                    else
                    {
                        ch = str196[4];
                        IUC285Proxy.Log("  " + ch + " - Go online if Offline dynamic data authentication failed");
                    }

                    ch = str196[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[5];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Combined DDA/AC Generation failed");
                    }
                    else
                    {
                        ch = str196[5];
                        IUC285Proxy.Log("  " + ch + " - Go online if Combined DDA/AC Generation failed");
                    }

                    ch = str196[6];
                    if (ch.ToString() == "0")
                    {
                        ch = str196[6];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if SDA selected");
                    }
                    else
                    {
                        ch = str196[6];
                        IUC285Proxy.Log("  " + ch + " - Go online if SDA selected");
                    }

                    ch = str196[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 2: " + str197);
                    ch = str197[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str197[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if ICC and terminal have different application versions");
                    }
                    else
                    {
                        ch = str197[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Go online if ICC and terminal have different application versions");
                    }

                    ch = str197[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str197[1];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Expired application");
                    }
                    else
                    {
                        ch = str197[1];
                        IUC285Proxy.Log("  " + ch + " - Go online if Expired application");
                    }

                    ch = str197[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str197[2];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Application not yet effective");
                    }
                    else
                    {
                        ch = str197[2];
                        IUC285Proxy.Log("  " + ch + " - Go online if Application not yet effective");
                    }

                    ch = str197[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str197[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Requested service not allowed for card product");
                    }
                    else
                    {
                        ch = str197[3];
                        IUC285Proxy.Log("  " + ch + " - Go online if Requested service not allowed for card product");
                    }

                    ch = str197[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str197[4];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if New card");
                    }
                    else
                    {
                        ch = str197[4];
                        IUC285Proxy.Log("  " + ch + " - Go online if New card");
                    }

                    ch = str197[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str197[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str197[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 3: " + str198);
                    ch = str198[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[0];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Cardholder verification was not successful");
                    }
                    else
                    {
                        ch = str198[0];
                        IUC285Proxy.Log("  " + ch + " - Go online if Cardholder verification was not successful");
                    }

                    ch = str198[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[1];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Unrecognised CVM");
                    }
                    else
                    {
                        ch = str198[1];
                        IUC285Proxy.Log("  " + ch + " - Go online if Unrecognised CVM");
                    }

                    ch = str198[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[2];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if PIN Try Limit exceeded");
                    }
                    else
                    {
                        ch = str198[2];
                        IUC285Proxy.Log("  " + ch + " - Go online if PIN Try Limit exceeded");
                    }

                    ch = str198[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if PIN entry required and PIN pad not present or not working");
                    }
                    else
                    {
                        ch = str198[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Go online if PIN entry required and PIN pad not present or not working");
                    }

                    ch = str198[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if PIN entry required, PIN pad present, but PIN was not entered");
                    }
                    else
                    {
                        ch = str198[4];
                        IUC285Proxy.Log("  " + ch +
                                        " - Go online if PIN entry required, PIN pad present, but PIN was not entered");
                    }

                    ch = str198[5];
                    if (ch.ToString() == "0")
                    {
                        ch = str198[5];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Online PIN entered");
                    }
                    else
                    {
                        ch = str198[5];
                        IUC285Proxy.Log("  " + ch + " - Go online if Online PIN entered");
                    }

                    ch = str198[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str198[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 4: " + str199);
                    ch = str199[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str199[0];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Transaction exceeds floor limit");
                    }
                    else
                    {
                        ch = str199[0];
                        IUC285Proxy.Log("  " + ch + " - Go online if Transaction exceeds floor limit");
                    }

                    ch = str199[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str199[1];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Lower consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str199[1];
                        IUC285Proxy.Log("  " + ch + " - Go online if Lower consecutive offline limit exceeded");
                    }

                    ch = str199[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str199[2];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Upper consecutive offline limit exceeded");
                    }
                    else
                    {
                        ch = str199[2];
                        IUC285Proxy.Log("  " + ch + " - Go online if Upper consecutive offline limit exceeded");
                    }

                    ch = str199[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str199[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Transaction selected randomly for online processing");
                    }
                    else
                    {
                        ch = str199[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Go online if Transaction selected randomly for online processing");
                    }

                    ch = str199[4];
                    if (ch.ToString() == "0")
                    {
                        ch = str199[4];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Merchant forced transaction online");
                    }
                    else
                    {
                        ch = str199[4];
                        IUC285Proxy.Log("  " + ch + " - Go online if Merchant forced transaction online");
                    }

                    ch = str199[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str199[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str199[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("-----");
                    IUC285Proxy.Log("Byte 5: " + str200);
                    ch = str200[0];
                    if (ch.ToString() == "0")
                    {
                        ch = str200[0];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Default TDOL used");
                    }
                    else
                    {
                        ch = str200[0];
                        IUC285Proxy.Log("  " + ch + " - Go online if Default TDOL used");
                    }

                    ch = str200[1];
                    if (ch.ToString() == "0")
                    {
                        ch = str200[1];
                        IUC285Proxy.Log("  " + ch + " - Do not go online if Issuer authentication was unsuccessful");
                    }
                    else
                    {
                        ch = str200[1];
                        IUC285Proxy.Log("  " + ch + " - Go online if Issuer authentication was unsuccessful");
                    }

                    ch = str200[2];
                    if (ch.ToString() == "0")
                    {
                        ch = str200[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Script processing failed before final GENERATE AC");
                    }
                    else
                    {
                        ch = str200[2];
                        IUC285Proxy.Log("  " + ch +
                                        " - Go online if Script processing failed before final GENERATE AC");
                    }

                    ch = str200[3];
                    if (ch.ToString() == "0")
                    {
                        ch = str200[3];
                        IUC285Proxy.Log("  " + ch +
                                        " - Do not go online if Script processing failed after final GENERATE AC");
                    }
                    else
                    {
                        ch = str200[3];
                        IUC285Proxy.Log("  " + ch + " - Go online if Script processing failed after final GENERATE AC");
                    }

                    ch = str200[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str200[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str200[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str200[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F27")
                {
                    IUC285Proxy.Log("---------Cryptogram information data(T9F27)---------");
                    var str201 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Binary data: " + str201);
                    if (str201.Substring(0, 2) == "00")
                    {
                        IUC285Proxy.Log("  00  - AAC");
                        Tag9F27CryptogramDeclined = true;
                    }

                    if (str201.Substring(0, 2) == "01")
                        IUC285Proxy.Log("  01  - TC");
                    if (str201.Substring(0, 2) == "10")
                        IUC285Proxy.Log("  10  - ARQC");
                    if (str201.Substring(0, 2) == "11")
                        IUC285Proxy.Log("  11  - RFU");
                    IUC285Proxy.Log("  " + str201.Substring(2, 2) + "  - Payment System-specific cryptogram");
                    var str202 = str201.Substring(4, 1);
                    if (str202 == "0")
                        IUC285Proxy.Log("  " + str202 + "   - No advice required");
                    else
                        IUC285Proxy.Log("  " + str202 + "   - Advice required");
                    var str203 = str201.Substring(5, 3);
                    switch (str203)
                    {
                        case "000":
                            IUC285Proxy.Log("  " + str203 + " - No information given");
                            break;
                        case "001":
                            IUC285Proxy.Log("  " + str203 + " - Service not allowed");
                            break;
                        case "010":
                            IUC285Proxy.Log("  " + str203 + " - PIN try limit exceeded");
                            break;
                        case "011":
                            IUC285Proxy.Log("  " + str203 + " - Issuer authetication failed");
                            break;
                        default:
                            if (str203.Substring(0, 1) == "1")
                            {
                                IUC285Proxy.Log("  " + str203 + " - Other values RFU");
                                break;
                            }

                            IUC285Proxy.Log("  " + str203 + " - Reason / advice code");
                            break;
                    }

                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F33")
                {
                    IUC285Proxy.Log("------------Terminal Capabilities(T9F33)------------");
                    var str204 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str205 = Convert.ToString(TagDataByte[1], 2).PadLeft(8, '0');
                    var str206 = Convert.ToString(TagDataByte[2], 2).PadLeft(8, '0');
                    IUC285Proxy.Log("Byte 1: " + str204);
                    IUC285Proxy.Log("  " + str204[0] + " - Manual key entry");
                    var ch = str204[1];
                    IUC285Proxy.Log("  " + ch + " - Magnetic stripe");
                    ch = str204[2];
                    IUC285Proxy.Log("  " + ch + " - IC with contacts");
                    ch = str204[3];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str204[4];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str204[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str204[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str204[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 2: " + str205);
                    ch = str205[0];
                    IUC285Proxy.Log("  " + ch + " - Plaintext PIN for ICC verification");
                    ch = str205[1];
                    IUC285Proxy.Log("  " + ch + " - Enciphered PIN for online verification");
                    ch = str205[2];
                    IUC285Proxy.Log("  " + ch + " - Signature (paper)");
                    ch = str205[3];
                    IUC285Proxy.Log("  " + ch + " - Enciphered PIN for offline verification");
                    ch = str205[4];
                    IUC285Proxy.Log("  " + ch + " - No CVM required");
                    ch = str205[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str205[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str205[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("Byte 3: " + str206);
                    ch = str206[0];
                    IUC285Proxy.Log("  " + ch + " - SDA");
                    ch = str206[1];
                    IUC285Proxy.Log("  " + ch + " - DDA");
                    ch = str206[2];
                    IUC285Proxy.Log("  " + ch + " - Card capture");
                    ch = str206[3];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str206[4];
                    IUC285Proxy.Log("  " + ch + " - CDA");
                    ch = str206[5];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str206[6];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    ch = str206[7];
                    IUC285Proxy.Log("  " + ch + " - RFU");
                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F34")
                {
                    IUC285Proxy.Log("-----------------CVM Results(T9F34)-----------------");
                    var str207 = Convert.ToString(TagDataByte[0], 2).PadLeft(8, '0');
                    var str208 = TagDataByte[1].ToString("X2");
                    var str209 = TagDataByte[2].ToString("X2");
                    IUC285Proxy.Log("Byte 1 - CVM Performed: " + str207);
                    IUC285Proxy.Log("  " + str207.Substring(0, 1) + "      - RFU");
                    var str210 = str207.Substring(1, 1);
                    if (str210 == "0")
                        IUC285Proxy.Log("  " + str210 + "      - Fail cardholder verification if this is unsuccessful");
                    else
                        IUC285Proxy.Log("  " + str210 + "      - Apply succeeding CV Rule if this is unsuccessful");
                    switch (str207.Substring(2, 6))
                    {
                        case "000000":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) + " - Fail CVM Processing");
                            break;
                        case "000001":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) +
                                            " - Plaintext PIN verification performed by ICC");
                            break;
                        case "000010":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) + " - Enciphered PIN verified online");
                            break;
                        case "000011":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) +
                                            " - Plaintext PIN verification performed by ICC and signature");
                            break;
                        case "000100":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) +
                                            " - Enciphered PIN verification performed by ICC");
                            break;
                        case "000101":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) +
                                            " - Enciphered PIN verification performed by ICC and signature");
                            break;
                        case "011110":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) + " - Signature");
                            break;
                        case "011111":
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) + " - No CVM");
                            break;
                        default:
                            IUC285Proxy.Log("  " + str207.Substring(2, 6) + " - Reserved");
                            break;
                    }

                    IUC285Proxy.Log("Byte 2 - CVM Condition: " + str208);
                    switch (str208)
                    {
                        case "00":
                            IUC285Proxy.Log("  00     - Always");
                            break;
                        case "01":
                            IUC285Proxy.Log("  01     - If unattended cash");
                            break;
                        case "02":
                            IUC285Proxy.Log(
                                "  02     - If not unattended cash and not manual cash and not purchase with cashback");
                            break;
                        case "03":
                            IUC285Proxy.Log("  03     - If terminal supports the CVM");
                            break;
                        case "04":
                            IUC285Proxy.Log("  04     - If manual cash");
                            break;
                        case "05":
                            IUC285Proxy.Log("  05     - If purchase with cashback");
                            break;
                        case "06":
                            IUC285Proxy.Log(
                                "  06     - If transaction is in the application currency and is under X value");
                            break;
                        case "07":
                            IUC285Proxy.Log(
                                "  07     - If transaction is in the application currency and is over X value");
                            break;
                        case "08":
                            IUC285Proxy.Log(
                                "  08     - If transaction is in the application currency and is under Y value");
                            break;
                        case "09":
                            IUC285Proxy.Log(
                                "  09     - If transaction is in the application currency and is over Y value");
                            break;
                        default:
                            IUC285Proxy.Log("  " + str208 + " - RFU");
                            break;
                    }

                    IUC285Proxy.Log("Byte 3 - CVM Result: " + str209);
                    switch (str209)
                    {
                        case "00":
                            IUC285Proxy.Log("  00     - Unknown");
                            break;
                        case "01":
                            IUC285Proxy.Log("  01     - Failed");
                            break;
                        case "02":
                            IUC285Proxy.Log("  02     - Successful");
                            break;
                        default:
                            IUC285Proxy.Log("  " + str209 + "     - Undefined");
                            break;
                    }

                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "9F6E")
                {
                    IUC285Proxy.Log(
                        "---------Third Party Data - FFI (Visa) - Device Type (MasterCard) (9F6E)----------");
                    if (CardBrand == CardBrandEnum.Mastercard)
                    {
                        IUC285Proxy.Log("---------Device Type (MasterCard) (9F6E)----------");
                        if (TagDataStr.Length <= 12)
                            return;
                        var s = ConvertHexToString(TagDataStr.Substring(8, 4), errors);
                        IUC285Proxy.Log("DeviceType: " + s);
                        int result;
                        if (!int.TryParse(s, out result) || result <= 0 || result == 20)
                            return;
                        IsMobileWallet = true;
                        IUC285Proxy.Log("MobileWallet Transaction");
                    }
                    else
                    {
                        if (CardBrand != CardBrandEnum.VISA)
                            return;
                        IUC285Proxy.Log("---------Device Type (VISA) (9F6E)----------");
                        if (TagDataStr.Length < 4)
                            return;
                        IUC285Proxy.Log("DeviceType: " + TagDataStr);
                        if ((Convert.ToInt16(TagDataStr.Substring(1, 1), 16) & 2) <= 0)
                            return;
                        IsMobileWallet = true;
                        IUC285Proxy.Log("MobileWallet Transaction");
                    }
                }
                else if (Tag == "1003")
                {
                    IUC285Proxy.Log("---------Confirmation response code (1003)----------");
                    var ch = Convert.ToChar(TagDataByte[0]);
                    switch (ch)
                    {
                        case 'A':
                            IUC285Proxy.Log("  " + ch + " - Approved");
                            break;
                        case 'C':
                            IUC285Proxy.Log("  " + ch + " - Completed(refund)");
                            break;
                        case 'D':
                            IUC285Proxy.Log("  " + ch + " - Declined");
                            break;
                        case 'E':
                            IUC285Proxy.Log("  " + ch + " - Error");
                            break;
                    }

                    IUC285Proxy.Log("----------------------------------------------------");
                }
                else if (Tag == "1010")
                {
                    IUC285Proxy.Log("-------------Error response code(1010)--------------");
                    var length = TagDataStr.Length;
                    var stringBuilder = new StringBuilder();
                    var strArray = new string[length / 2];
                    var index = 0;
                    for (var startIndex = 0; startIndex < TagDataStr.Length; startIndex += 2)
                    {
                        strArray[index] = TagDataStr.Substring(startIndex, 2);
                        ++index;
                    }

                    foreach (var str211 in strArray)
                    {
                        var int32 = Convert.ToInt32(str211, 16);
                        stringBuilder.Append(char.ConvertFromUtf32(int32));
                    }

                    ErrorCode = stringBuilder.ToString();
                    IUC285Proxy.Log("Error response code = " + ErrorCode);
                    IUC285Proxy.Log("----------------------------------------------------");
                    FallbackStatusAction fallbackStatusAction;
                    Fallback = _fallbackErrorStatusActions.TryGetValue(ErrorCode, out fallbackStatusAction)
                        ? fallbackStatusAction
                        : FallbackStatusAction.NoAction;
                    switch (ErrorCode)
                    {
                        case "CDIV":
                            FallbackReason = FallbackType.CardDataInvalid;
                            break;
                        case "CNSUP":
                            FallbackReason = FallbackType.CardNotSupported;
                            break;
                    }
                }
                else if (Tag == "5F20")
                {
                    CardHolderName = ConvertHexToString(TagDataStr, errors);
                }
                else
                {
                    if (!(Tag == "FF1F"))
                        return;
                    FF1FOnGuardCardData = ConvertHexToString(TagDataStr, errors);
                }
            }
        }

        public static string ConvertHexToString(string hexString, IList<Error> errors)
        {
            var stringBuilder = new StringBuilder();
            for (var startIndex = 0; startIndex < hexString.Length; startIndex += 2)
            {
                int result;
                if (int.TryParse(hexString.Substring(startIndex, 2), NumberStyles.HexNumber, null, out result))
                {
                    stringBuilder.Append(Convert.ToChar(result));
                }
                else
                {
                    if (errors != null)
                        errors.Add(new Error
                        {
                            Code = "HTS001",
                            Message = "Failed to Convert Data to string."
                        });
                    return null;
                }
            }

            return stringBuilder.ToString();
        }
    }
}