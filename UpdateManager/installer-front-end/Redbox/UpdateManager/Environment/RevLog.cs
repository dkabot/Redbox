using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Redbox.UpdateManager.Environment
{
    internal class RevLog : IRevLog
    {
        public string Hash { get; set; }

        public string Label { get; set; }

        public List<ChangeItem> Changes { get; set; }

        public string HashFileName(string name)
        {
            string asciishA1Hash = Encoding.ASCII.GetBytes(name).ToASCIISHA1Hash();
            LogHelper.Instance.Log("{0} generated for file: {1} in IRevLog.HashFileName", (object)asciishA1Hash, (object)name);
            return string.Format("{0}-{1}.patch", (object)this.Hash, (object)asciishA1Hash);
        }
    }
}
