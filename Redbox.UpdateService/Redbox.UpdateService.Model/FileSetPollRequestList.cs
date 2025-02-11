using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Redbox.UpdateService.Model
{
  public class FileSetPollRequestList
  {
    public FileSetPollRequestList() => this.FileSetPollRequests = new List<FileSetPollRequest>();

    public List<FileSetPollRequest> FileSetPollRequests { get; set; }

    public string ToShortFormat()
    {
      if (this.FileSetPollRequests.Count == 0)
        return string.Empty;
      int pos = 0;
      int count = this.FileSetPollRequests.Count<FileSetPollRequest>();
      StringBuilder sb = new StringBuilder();
      this.FileSetPollRequests.ForEach((Action<FileSetPollRequest>) (item =>
      {
        sb.Append(string.Format("{0},{1},{2}", (object) item.FileSetId, (object) item.FileSetRevisionId, (object) (int) item.FileSetState));
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
        this.FileSetPollRequests.Clear();
        ((IEnumerable<string>) data.Split(new string[1]
        {
          "|"
        }, StringSplitOptions.RemoveEmptyEntries)).ForEach<string>((Action<string>) (item =>
        {
          string[] strArray = item.Split(new string[1]
          {
            ","
          }, StringSplitOptions.RemoveEmptyEntries);
          this.FileSetPollRequests.Add(new FileSetPollRequest()
          {
            FileSetId = Convert.ToInt64(strArray[0]),
            FileSetRevisionId = Convert.ToInt64(strArray[1]),
            FileSetState = (FileSetState) Convert.ToInt32(strArray[2])
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
