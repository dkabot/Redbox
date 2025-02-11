using System;
using System.Collections.Generic;
using System.Linq;
using DeviceService.ComponentModel;
using RBA_SDK_ComponentModel;

namespace DeviceService.Domain
{
    public class VasResult
    {
        public static readonly char[] VasStringSeperator = new char[1]
        {
            '\u001C'
        };

        public VasCmd VasCommand { get; set; }

        public ERROR_ID CommandResult { get; set; } = ERROR_ID.RESULT_ERROR;

        public VasStatus Status { get; set; } = VasStatus.Unknown;

        public string VasDataResult { get; set; }

        public string AdditionalData { get; set; }

        public List<string> Data { get; set; }

        public bool Success
        {
            get
            {
                if (CommandResult != ERROR_ID.RESULT_SUCCESS)
                    return false;
                return VasCommand != VasCmd.GetData || Status == VasStatus.Successful;
            }
        }

        public string GetVasData(ReadCardContext context)
        {
            var data = Data;
            if ((data != null ? !data.Any() ? 1 : 0 : 1) != 0)
                return null;
            if (string.IsNullOrEmpty(VasDataResult))
                return null;
            switch (VasDataResult)
            {
                case "0":
                    context.Wallet = WalletType.Apple;
                    return ParseApple(context);
                case "1":
                    context.Wallet = WalletType.Google;
                    return ParseGoogle();
                default:
                    return null;
            }
        }

        private string ParseApple(ReadCardContext context)
        {
            var apple = (string)null;
            foreach (var str1 in Data)
            {
                var strArray = str1.Split(VasStringSeperator);
                if (!(strArray[0] != 0.ToString()) && !(strArray[1] != "0"))
                {
                    var str2 = strArray[2];
                    var str3 = strArray[3];
                    var str4 = strArray[4];
                    if (!string.IsNullOrEmpty(str4) && !(str4 == "0"))
                    {
                        var hexString = strArray[5];
                        if (!string.IsNullOrEmpty(hexString) && hexString != "0")
                        {
                            var str5 = ParseTags.ConvertHexToString(hexString, context?.Errors);
                            if (!string.IsNullOrEmpty(str5))
                            {
                                apple = str5;
                                break;
                            }
                        }
                    }
                }
            }

            return apple;
        }

        private string ParseGoogle()
        {
            var google = (string)null;
            var strArray1 = Data[0].Split(VasStringSeperator);
            var str1 = strArray1[0];
            var str2 = strArray1[1];
            if (!(str1 != 0.ToString()))
            {
                var str3 = str2;
                var num = 0;
                var str4 = num.ToString();
                if (!(str3 != str4))
                {
                    var str5 = strArray1[2];
                    var str6 = strArray1[3];
                    var str7 = strArray1[4];
                    foreach (var str8 in Data.Skip(1))
                    {
                        var strArray2 = str8.Split(VasStringSeperator);
                        var str9 = strArray2[0];
                        num = 0;
                        var str10 = num.ToString();
                        if (!(str9 != str10))
                        {
                            var str11 = strArray2[1];
                            num = 0;
                            var str12 = num.ToString();
                            if (!(str11 != str12))
                            {
                                google = strArray2[2];
                                if (!string.IsNullOrWhiteSpace(google))
                                    Enum.TryParse(strArray2[3], out VasCardType _);
                            }
                        }
                    }

                    return google;
                }
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("Status: {0}-({1}) - AdditionalData: {2} - VasDataResult: {3} - Data: {4}", Status,
                Status.ToString(), AdditionalData, VasDataResult, string.Join(",", Data));
        }
    }
}