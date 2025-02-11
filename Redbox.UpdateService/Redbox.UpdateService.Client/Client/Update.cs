using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Client
{
    public class Update
    {
        public List<string> Categories;
        public Guid Id;
        public bool IsDownloaded;
        public bool IsInstalled;
        public Uri MoreInfoUrl;
        public string Title;
    }
}