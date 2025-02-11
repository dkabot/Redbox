namespace Redbox.UpdateService.Model
{
    internal class ClientRepositoryData
    {
        public const string EmptyHash = "0000000000000000000000000000000000000000";
        public string RepositoryName;
        public long RepositoryOID;
        public ClientRepositoryData.Revlog Assigned;
        public ClientRepositoryData.Revlog InTransit;
        public ClientRepositoryData.Revlog Head;
        public ClientRepositoryData.Revlog Staged;
        public ClientRepositoryData.Revlog Active;

        public ClientRepositoryData()
        {
        }

        public ClientRepositoryData(ClientRepositoryDataDTO dto)
        {
            this.RepositoryName = dto.R;
            this.InTransit = new ClientRepositoryData.Revlog()
            {
                Hash = dto.I
            };
            this.Staged = new ClientRepositoryData.Revlog()
            {
                Hash = dto.S
            };
            this.Active = new ClientRepositoryData.Revlog()
            {
                Hash = dto.A
            };
            this.Head = new ClientRepositoryData.Revlog()
            {
                Hash = dto.H
            };
            this.Assigned = new ClientRepositoryData.Revlog();
        }

        public class Revlog
        {
            public string LabelName;
            public string Hash;
            public long OID;

            public Revlog()
            {
                this.Hash = "0000000000000000000000000000000000000000";
                this.LabelName = string.Empty;
                this.OID = 0L;
            }
        }

        public class Repository
        {
            public string Name;
            public long OID;
        }
    }
}
