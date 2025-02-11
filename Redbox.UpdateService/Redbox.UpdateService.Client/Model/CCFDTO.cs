using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model
{
    public class CCFDTO
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long SGId { get; set; }

        public DateTime? SGD { get; set; }

        public long AGId { get; set; }

        public DateTime? AGD { get; set; }

        public static List<ClientConfigFile> GetClientConfigFileDataList(List<CCFDTO> dtoList)
        {
            var configFileDataList = new List<ClientConfigFile>();
            foreach (var dto in dtoList)
            {
                var clientConfigFile = new ClientConfigFile();
                configFileDataList.Add(clientConfigFile);
                clientConfigFile.ConfigFileOID = dto.Id;
                clientConfigFile.Name = dto.Name;
                clientConfigFile.StagedGenerationId = dto.SGId;
                clientConfigFile.StagedGenerationDate = dto.SGD;
                clientConfigFile.ActiveGenerationId = dto.AGId;
                clientConfigFile.ActiveGenerationDate = dto.AGD;
            }

            return configFileDataList;
        }

        public static List<CCFDTO> GetCCFDTOList(List<ClientConfigFile> list)
        {
            var ccfdtoList = new List<CCFDTO>();
            foreach (var clientConfigFile in list)
            {
                var ccfdto = new CCFDTO();
                ccfdtoList.Add(ccfdto);
                ccfdto.Id = clientConfigFile.ConfigFileOID;
                ccfdto.Name = clientConfigFile.Name;
                ccfdto.SGId = clientConfigFile.StagedGenerationId;
                ccfdto.SGD = clientConfigFile.StagedGenerationDate;
                ccfdto.AGId = clientConfigFile.ActiveGenerationId;
                ccfdto.AGD = clientConfigFile.ActiveGenerationDate;
            }

            return ccfdtoList;
        }
    }
}