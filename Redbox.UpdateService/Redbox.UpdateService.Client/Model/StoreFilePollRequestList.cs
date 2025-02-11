using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model
{
    public class StoreFilePollRequestList
    {
        public StoreFilePollRequestList()
        {
            StoreFilePollRequests = new List<StoreFilePollRequest>();
        }

        public List<StoreFilePollRequest> StoreFilePollRequests { get; set; }

        public string ToShortFormat()
        {
            if (StoreFilePollRequests.Count == 0)
                return string.Empty;
            var pos = 0;
            var count = StoreFilePollRequests.Count();
            var sb = new StringBuilder();
            StoreFilePollRequests.ForEach(item =>
            {
                sb.Append(string.Format("{0},{1},{2}", item.StoreFile, item.StoreFileData, item.SyncId));
                ++pos;
                if (pos >= count)
                    return;
                sb.Append("|");
            });
            return sb.ToString();
        }

        public void FromShortFormat(string data)
        {
            try
            {
                StoreFilePollRequests.Clear();
                LinqExtensions.ForEach<string>((IEnumerable<string>)data.Split(new string[1]
                {
                    "|"
                }, StringSplitOptions.RemoveEmptyEntries), (Action<string>)(item =>
                {
                    var strArray = item.Split(new string[1]
                    {
                        ","
                    }, StringSplitOptions.RemoveEmptyEntries);
                    StoreFilePollRequests.Add(new StoreFilePollRequest
                    {
                        StoreFile = Convert.ToInt64(strArray[0]),
                        StoreFileData = Convert.ToInt64(strArray[1]),
                        SyncId = Convert.ToInt32(strArray[2])
                    });
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("StoreFilePollRequestList - Unhandled exception.", ex);
            }
        }
    }
}