using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model
{
    public class FileSetPollRequestList
    {
        public FileSetPollRequestList()
        {
            FileSetPollRequests = new List<FileSetPollRequest>();
        }

        public List<FileSetPollRequest> FileSetPollRequests { get; set; }

        public string ToShortFormat()
        {
            if (FileSetPollRequests.Count == 0)
                return string.Empty;
            var pos = 0;
            var count = FileSetPollRequests.Count();
            var sb = new StringBuilder();
            FileSetPollRequests.ForEach(item =>
            {
                sb.Append(string.Format("{0},{1},{2}", item.FileSetId, item.FileSetRevisionId, (int)item.FileSetState));
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
                FileSetPollRequests.Clear();
                LinqExtensions.ForEach<string>((IEnumerable<string>)data.Split(new string[1]
                {
                    "|"
                }, StringSplitOptions.RemoveEmptyEntries), (Action<string>)(item =>
                {
                    var strArray = item.Split(new string[1]
                    {
                        ","
                    }, StringSplitOptions.RemoveEmptyEntries);
                    FileSetPollRequests.Add(new FileSetPollRequest
                    {
                        FileSetId = Convert.ToInt64(strArray[0]),
                        FileSetRevisionId = Convert.ToInt64(strArray[1]),
                        FileSetState = (FileSetState)Convert.ToInt32(strArray[2])
                    });
                }));
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("FileSetPollRequestList - Unhandled exception.", ex);
            }
        }
    }
}