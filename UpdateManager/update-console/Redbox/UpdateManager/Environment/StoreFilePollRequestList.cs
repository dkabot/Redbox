using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redbox.UpdateManager.Environment
{
    internal class StoreFilePollRequestList
    {
        public StoreFilePollRequestList()
        {
            this.StoreFilePollRequests = new List<StoreFilePollRequest>();
        }

        public List<StoreFilePollRequest> StoreFilePollRequests { get; set; }

        public string ToShortFormat()
        {
            if (this.StoreFilePollRequests.Count == 0)
                return string.Empty;
            int pos = 0;
            int count = this.StoreFilePollRequests.Count<StoreFilePollRequest>();
            StringBuilder sb = new StringBuilder();
            this.StoreFilePollRequests.ForEach((Action<StoreFilePollRequest>)(item =>
            {
                sb.Append(string.Format("{0},{1},{2}", (object)item.StoreFile, (object)item.StoreFileData, (object)item.SyncId));
                ++pos;
                if (pos >= count)
                    return;
                sb.Append("|");
            }));
            return sb.ToString();
        }

        public void FromShortFormat(string data)
        {
            try
            {
                this.StoreFilePollRequests.Clear();
                ((IEnumerable<string>)data.Split(new string[1]
                {
          "|"
                }, StringSplitOptions.RemoveEmptyEntries)).ForEach<string>((Action<string>)(item =>
                {
                    string[] strArray = item.Split(new string[1]
            {
            ","
                  }, StringSplitOptions.RemoveEmptyEntries);
                    this.StoreFilePollRequests.Add(new StoreFilePollRequest()
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
