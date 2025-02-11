using Redbox.Core;
using System.Text;

namespace Redbox.UpdateService.Client
{
    internal class Work
    {
        public string GetScriptText() => Encoding.Unicode.GetString(this.Command.Base64ToBytes());

        public long ID { get; set; }

        public string Command { get; set; }

        public string Name { get; set; }
    }
}
