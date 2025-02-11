using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model
{
    internal static class RepositoryDiffMerge
    {
        private static readonly Dictionary<string, string> CompressionLibrary = new Dictionary<string, string>
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
                var dictionaryList = new List<Dictionary<string, string>>();
                var repositoryDataDtoList = new List<ClientRepositoryDataDTO>();
                foreach (var repositoryDataDto1 in output)
                {
                    var i = repositoryDataDto1;
                    var repositoryDataDto2 = input.FirstOrDefault(fromItem => i.R == fromItem.R);
                    if (repositoryDataDto2 != null)
                    {
                        var flag = false;
                        var dictionary = new Dictionary<string, string>
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
                    {
                        repositoryDataDtoList.Add(repositoryDataDto1);
                    }
                }

                var list = input.Where(inputItem => !output.Any(outputItem => inputItem.R == outputItem.R))
                    .Select(inputItem => inputItem.R).ToList();
                rdw = new RepositoryDiffWrapper
                {
                    O = Hash(output),
                    I = Hash(input),
                    U = dictionaryList,
                    A = repositoryDataDtoList,
                    D = list
                };
                return !verify || Verify(input, rdw);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("There was an unhandled exception in TryDiff", ex);
                rdw = null;
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
                if (Hash(input) != rdw.I)
                {
                    LogHelper.Instance.Log("RDS200", "The hash check of the input failed in merging");
                    output = new List<ClientRepositoryDataDTO>();
                    return false;
                }

                if (rdw.U != null && rdw.U.Any())
                    foreach (var dictionary in rdw.U)
                    {
                        var u =
                            dictionary.ToJson().ToObject<ClientRepositoryDataDTO>();
                        var repositoryDataDto = input.First(i => u.R == i.R);
                        if (u.H != null)
                            repositoryDataDto.H = u.H;
                        if (u.I != null)
                            repositoryDataDto.I = u.I;
                        if (u.A != null)
                            repositoryDataDto.A = u.A;
                        if (u.S != null)
                            repositoryDataDto.S = u.S;
                    }

                if (rdw.A != null && rdw.A.Any())
                    input.AddRange(rdw.A);
                if (rdw.D != null && rdw.D.Any())
                    rdw.D.ForEach(i => input.RemoveAll(j => i == j.R));
                output = input;
                if (!(Hash(output) != rdw.O))
                    return true;
                LogHelper.Instance.Log("RDS100", "The hash check of the output failed in merging");
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
            var listDictionary = new ListDictionary
            {
                {
                    "I",
                    wrapper.I
                },
                {
                    "O",
                    wrapper.O
                }
            };
            if (wrapper.U != null && wrapper.U.Any())
                listDictionary.Add("U", wrapper.U);
            if (wrapper.A != null && wrapper.A.Any())
                listDictionary.Add("A", wrapper.A);
            if (wrapper.D != null && wrapper.D.Any())
                listDictionary.Add("D", wrapper.D);
            var json = listDictionary.ToJson();
            return CompressionLibrary.Aggregate(json, (current, i) => current.Replace(i.Value, i.Key));
        }

        public static RepositoryDiffWrapper Expand(string str)
        {
            str = CompressionLibrary.Aggregate(str, (current, i) => current.Replace(i.Key, i.Value));
            return str.ToObject<RepositoryDiffWrapper>();
        }

        internal static List<ClientRepositoryDataDTO> Copy(this List<ClientRepositoryDataDTO> list)
        {
            return list.Select(each => each.Copy()).ToList();
        }

        private static bool Verify(List<ClientRepositoryDataDTO> input, RepositoryDiffWrapper rdw)
        {
            try
            {
                var wrapper = rdw.Copy();
                var flag = TryMerge(input.Copy(), Expand(Minimize(wrapper)), out _);
                if (!flag)
                    LogHelper.Instance.Log("RDS400", "Verification failed at merging.");
                return flag;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("RDS401", "Verification failed with an unknown exception.", ex);
                return false;
            }
        }
    }
}