using System;

namespace Redbox.UpdateService.Model
{
    internal class ClientRepositoryDataDTO : IComparable
    {
        public string R;
        public string I;
        public string S;
        public string A;
        public string H;

        public ClientRepositoryDataDTO()
        {
        }

        public ClientRepositoryDataDTO(ClientRepositoryData crd)
        {
            this.R = crd.RepositoryName;
            this.I = crd.InTransit != null ? crd.InTransit.Hash : "0000000000000000000000000000000000000000";
            this.S = crd.Staged != null ? crd.Staged.Hash : "0000000000000000000000000000000000000000";
            this.A = crd.Active != null ? crd.Active.Hash : "0000000000000000000000000000000000000000";
            this.H = crd.Head != null ? crd.Head.Hash : "0000000000000000000000000000000000000000";
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            return obj is ClientRepositoryDataDTO repositoryDataDto ? this.R.CompareTo(repositoryDataDto.R) : throw new ArgumentException("Object is not a ClientRepositoryDataDTO");
        }

        public ClientRepositoryDataDTO Copy()
        {
            return new ClientRepositoryDataDTO()
            {
                R = string.Copy(this.R),
                I = string.Copy(this.I),
                S = string.Copy(this.S),
                A = string.Copy(this.A),
                H = string.Copy(this.H)
            };
        }
    }
}
