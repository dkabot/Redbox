using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Redbox.HAL.Client
{
    public sealed class ProgramResult
    {
        public ProgramResult(
            Guid id,
            DateTime timeStamp,
            int deck,
            int slot,
            string code,
            string itemId,
            string message)
        {
            ID = id;
            Deck = deck;
            Slot = slot;
            Code = code;
            ItemID = new ResultItem(itemId);
            Message = message;
            TimeStamp = timeStamp;
        }

        private ProgramResult()
        {
        }

        public Guid ID { get; internal set; }

        public int Deck { get; internal set; }

        public int Slot { get; internal set; }

        public string Code { get; internal set; }

        public ResultItem ItemID { get; internal set; }

        public string Message { get; internal set; }

        public DateTime TimeStamp { get; internal set; }

        public DateTime? ReturnTime { get; internal set; }

        public ResultItem PreviousItem { get; internal set; }

        public IDictionary<string, object> MessageData { get; internal set; }

        public static ProgramResult FromString(string value)
        {
            var strArray1 = value.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (string.Compare(strArray1[0], "RESULT", true) != 0 || strArray1.Length < 7)
                return null;
            var programResult = new ProgramResult
            {
                ID = new Guid(strArray1[1]),
                TimeStamp = DateTime.Parse(strArray1[2]),
                Code = strArray1[3].Trim()
            };
            var strArray2 = strArray1[4].Trim().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (strArray2.Length == 2)
            {
                int result;
                if (int.TryParse(strArray2[0], out result))
                    programResult.Deck = result;
                if (int.TryParse(strArray2[1], out result))
                    programResult.Slot = result;
            }

            var str1 = strArray1[5].Trim();
            if (str1 != "~")
                programResult.ItemID = new ResultItem(str1);
            programResult.Message = strArray1[6];
            if (!string.IsNullOrEmpty(programResult.Message) && programResult.Message.StartsWith("{"))
                programResult.MessageData =
                    JsonConvert.DeserializeObject<IDictionary<string, object>>(programResult.Message);
            if (9 == strArray1.Length)
            {
                var s = strArray1[7].Trim();
                if (s != "~")
                    programResult.ReturnTime = DateTime.Parse(s);
                var str2 = strArray1[8].Trim();
                if (str2 != "~")
                    programResult.PreviousItem = new ResultItem(str2);
            }

            return programResult;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("{0} {1} [{2}", ID, TimeStamp, Code);
            if (Deck != 0 && Slot != 0)
                stringBuilder.AppendFormat("@{0},{1}", Deck, Slot);
            if (ItemID != null)
                stringBuilder.AppendFormat(":{0}", ItemID);
            stringBuilder.AppendFormat("] {0}", Message);
            if (ReturnTime.HasValue)
                stringBuilder.AppendFormat("{0}", ReturnTime);
            if (PreviousItem != null)
                stringBuilder.AppendFormat("{0}", PreviousItem);
            return stringBuilder.ToString();
        }
    }
}