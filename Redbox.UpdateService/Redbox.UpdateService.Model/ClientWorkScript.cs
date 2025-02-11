using Redbox.Core;
using System.Text;

namespace Redbox.UpdateService.Model
{
  public class ClientWorkScript
  {
    public long ID { get; set; }

    public string Command { get; set; }

    public string GetScriptText() => Encoding.Unicode.GetString(this.Command.Base64ToBytes());
  }
}
