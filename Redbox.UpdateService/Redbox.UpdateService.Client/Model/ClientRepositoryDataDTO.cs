using System;

namespace Redbox.UpdateService.Model
{
    public class ClientRepositoryDataDTO : IComparable
    {
        public string A;
        public string H;
        public string I;
        public string R;
        public string S;

        public ClientRepositoryDataDTO()
        {
        }

        public ClientRepositoryDataDTO(ClientRepositoryData crd)
        {
            R = crd.RepositoryName;
            I = crd.InTransit != null ? crd.InTransit.Hash : "0000000000000000000000000000000000000000";
            S = crd.Staged != null ? crd.Staged.Hash : "0000000000000000000000000000000000000000";
            A = crd.Active != null ? crd.Active.Hash : "0000000000000000000000000000000000000000";
            H = crd.Head != null ? crd.Head.Hash : "0000000000000000000000000000000000000000";
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            return obj is ClientRepositoryDataDTO repositoryDataDto
                ? R.CompareTo(repositoryDataDto.R)
                : throw new ArgumentException("Object is not a ClientRepositoryDataDTO");
        }

        public ClientRepositoryDataDTO Copy()
        {
            return new ClientRepositoryDataDTO
            {
                R = string.Copy(R),
                I = string.Copy(I),
                S = string.Copy(S),
                A = string.Copy(A),
                H = string.Copy(H)
            };
        }
    }
}