using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Redbox.UpdateService.Model
{
    internal static class RepositoryDiffMerge
    {
        private static readonly Dictionary<string, string> CompressionLibrary = new Dictionary<string, string>()
    {
      {
        "%",
        "0000000000000000000000000000000000000000"
      }
    };

        public static bool TryDiff(
          List<ClientRepositoryDataDTO> input,
          List<ClientRepositoryDataDTO> output,
          bool verify,
          out RepositoryDiffWrapper rdw)
        {
            try
            {
                rdw = new RepositoryDiffWrapper();
                List<Dictionary<string, string>> dictionaryList = new List<Dictionary<string, string>>();
                List<ClientRepositoryDataDTO> repositoryDataDtoList = new List<ClientRepositoryDataDTO>();
                foreach (ClientRepositoryDataDTO repositoryDataDto1 in output)
                {
                    ClientRepositoryDataDTO i = repositoryDataDto1;
                    ClientRepositoryDataDTO repositoryDataDto2 = input.FirstOrDefault<ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, bool>)(fromItem => i.R == fromItem.R));
                    if (repositoryDataDto2 != null)
                    {
                        bool flag = false;
                        Dictionary<string, string> dictionary = new Dictionary<string, string>()
            {
              {
                "R",
                repositoryDataDto1.R
              }
            };
                        if (repositoryDataDto1.H != repositoryDataDto2.H)
                        {
                            dictionary.Add("H", repositoryDataDto1.H);
                            flag = true;
                        }
                        if (repositoryDataDto1.I != repositoryDataDto2.I)
                        {
                            dictionary.Add("I", repositoryDataDto1.I);
                            flag = true;
                        }
                        if (repositoryDataDto1.A != repositoryDataDto2.A)
                        {
                            dictionary.Add("A", repositoryDataDto1.A);
                            flag = true;
                        }
                        if (repositoryDataDto1.S != repositoryDataDto2.S)
                        {
                            dictionary.Add("S", repositoryDataDto1.S);
                            flag = true;
                        }
                        if (flag)
                            dictionaryList.Add(dictionary);
                    }
                    else
                        repositoryDataDtoList.Add(repositoryDataDto1);
                }
                List<string> list = input.Where<ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, bool>)(inputItem => !output.Any<ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, bool>)(outputItem => inputItem.R == outputItem.R)))).Select<ClientRepositoryDataDTO, string>((Func<ClientRepositoryDataDTO, string>)(inputItem => inputItem.R)).ToList<string>();
                rdw = new RepositoryDiffWrapper()
                {
                    O = RepositoryDiffMerge.Hash(output),
                    I = RepositoryDiffMerge.Hash(input),
                    U = dictionaryList,
                    A = repositoryDataDtoList,
                    D = list
                };
                return !verify || RepositoryDiffMerge.Verify(input, rdw);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an unhandled exception in TryDiff", ex);
                rdw = (RepositoryDiffWrapper)null;
                return false;
            }
        }

        public static bool TryMerge(
          List<ClientRepositoryDataDTO> input,
          RepositoryDiffWrapper rdw,
          out List<ClientRepositoryDataDTO> output)
        {
            try
            {
                if (RepositoryDiffMerge.Hash(input) != rdw.I)
                {
                    LogHelper.Instance.Log("RDS200", (object)"The hash check of the input failed in merging");
                    output = new List<ClientRepositoryDataDTO>();
                    return false;
                }
                if (rdw.U != null && rdw.U.Any<Dictionary<string, string>>())
                {
                    foreach (Dictionary<string, string> instance in rdw.U)
                    {
                        ClientRepositoryDataDTO u = instance.ToJson().ToObject<ClientRepositoryDataDTO>();
                        ClientRepositoryDataDTO repositoryDataDto = input.First<ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, bool>)(i => u.R == i.R));
                        if (u.H != null)
                            repositoryDataDto.H = u.H;
                        if (u.I != null)
                            repositoryDataDto.I = u.I;
                        if (u.A != null)
                            repositoryDataDto.A = u.A;
                        if (u.S != null)
                            repositoryDataDto.S = u.S;
                    }
                }
                if (rdw.A != null && rdw.A.Any<ClientRepositoryDataDTO>())
                    input.AddRange((IEnumerable<ClientRepositoryDataDTO>)rdw.A);
                if (rdw.D != null && rdw.D.Any<string>())
                    rdw.D.ForEach((Action<string>)(i => input.RemoveAll((Predicate<ClientRepositoryDataDTO>)(j => i == j.R))));
                output = input;
                if (!(RepositoryDiffMerge.Hash(output) != rdw.O))
                    return true;
                LogHelper.Instance.Log("RDS100", (object)"The hash check of the output failed in merging");
                output = new List<ClientRepositoryDataDTO>();
                return false;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an unhandled exception in TryMerge", ex);
                output = new List<ClientRepositoryDataDTO>();
                return false;
            }
        }

        public static string Hash(List<ClientRepositoryDataDTO> hashMe)
        {
            if (hashMe == null)
                return string.Empty;
            hashMe.Sort();
            return Encoding.ASCII.GetBytes(hashMe.ToJson()).ToASCIISHA1Hash();
        }

        public static string Minimize(RepositoryDiffWrapper wrapper)
        {
            ListDictionary instance = new ListDictionary()
      {
        {
          (object) "I",
          (object) wrapper.I
        },
        {
          (object) "O",
          (object) wrapper.O
        }
      };
            if (wrapper.U != null && wrapper.U.Any<Dictionary<string, string>>())
                instance.Add((object)"U", (object)wrapper.U);
            if (wrapper.A != null && wrapper.A.Any<ClientRepositoryDataDTO>())
                instance.Add((object)"A", (object)wrapper.A);
            if (wrapper.D != null && wrapper.D.Any<string>())
                instance.Add((object)"D", (object)wrapper.D);
            string json = instance.ToJson();
            return RepositoryDiffMerge.CompressionLibrary.Aggregate<KeyValuePair<string, string>, string>(json, (Func<string, KeyValuePair<string, string>, string>)((current, i) => current.Replace(i.Value, i.Key)));
        }

        public static RepositoryDiffWrapper Expand(string str)
        {
            str = RepositoryDiffMerge.CompressionLibrary.Aggregate<KeyValuePair<string, string>, string>(str, (Func<string, KeyValuePair<string, string>, string>)((current, i) => current.Replace(i.Key, i.Value)));
            return str.ToObject<RepositoryDiffWrapper>();
        }

        internal static List<ClientRepositoryDataDTO> Copy(this List<ClientRepositoryDataDTO> list)
        {
            return list.Select<ClientRepositoryDataDTO, ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, ClientRepositoryDataDTO>)(each => each.Copy())).ToList<ClientRepositoryDataDTO>();
        }

        private static bool Verify(List<ClientRepositoryDataDTO> input, RepositoryDiffWrapper rdw)
        {
            try
            {
                RepositoryDiffWrapper wrapper = rdw.Copy();
                bool flag = RepositoryDiffMerge.TryMerge(input.Copy(), RepositoryDiffMerge.Expand(RepositoryDiffMerge.Minimize(wrapper)), out List<ClientRepositoryDataDTO> _);
                if (!flag)
                    LogHelper.Instance.Log("RDS400", (object)"Verification failed at merging.");
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("RDS401", (object)"Verification failed with an unknown exception.", (object)ex);
                return false;
            }
        }
    }
}
