namespace Redbox.UpdateService.Model
{
    public class ClientRepositoryData
    {
        public const string EmptyHash = "0000000000000000000000000000000000000000";
        public Revlog Active;
        public Revlog Assigned;
        public Revlog Head;
        public Revlog InTransit;
        public string RepositoryName;
        public long RepositoryOID;
        public Revlog Staged;

        public ClientRepositoryData()
        {
        }

        public ClientRepositoryData(ClientRepositoryDataDTO dto)
        {
            RepositoryName = dto.R;
            InTransit = new Revlog
            {
                Hash = dto.I
            };
            Staged = new Revlog
            {
                Hash = dto.S
            };
            Active = new Revlog
            {
                Hash = dto.A
            };
            Head = new Revlog
            {
                Hash = dto.H
            };
            Assigned = new Revlog();
        }

        public class Revlog
        {
            public string Hash;
            public string LabelName;
            public long OID;

            public Revlog()
            {
                Hash = "0000000000000000000000000000000000000000";
                LabelName = string.Empty;
                OID = 0L;
            }
        }

        public class Repository
        {
            public string Name;
            public long OID;
        }
    }
}