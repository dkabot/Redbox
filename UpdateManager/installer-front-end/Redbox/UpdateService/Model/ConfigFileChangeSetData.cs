using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    internal class ConfigFileChangeSetData
    {
        public long OID { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public long GenerationOID { get; set; }

        public List<Redbox.UpdateService.Model.ConfigurationData> ConfigurationData { get; set; }
    }
}
