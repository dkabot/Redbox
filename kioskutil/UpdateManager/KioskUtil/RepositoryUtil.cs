using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.Environment;

namespace Redbox.UpdateManager.KioskUtil
{
    public static class RepositoryUtil
    {
        private const string RepositoryLocation =
            "C:\\Program Files\\Redbox\\REDS\\Update Manager\\.store\\.repository";

        public const string EmptyHash = "0000000000000000000000000000000000000000";

        public static void ListRevisions(string repositoryName)
        {
            GetRevLogList(repositoryName).ForEach(l =>
            {
                Console.WriteLine(l.Hash);
                l.Changes.ForEach(ci => Console.WriteLine("Composite:" + ci.Composite + "IsSeed:" + ci.IsSeed));
            });
        }

        public static void TrimRevisions(string repositoryName)
        {
            var revLogList = GetRevLogList(repositoryName);
            if (revLogList.Count < 1)
            {
                Console.WriteLine("No repository revisions were found. No need to trim.");
            }
            else
            {
                var current = ReadCurrentRevisionInternal(repositoryName);
                if (current == "0000000000000000000000000000000000000000")
                {
                    Console.WriteLine("Current revision is not set. No need to trim.");
                }
                else
                {
                    var index1 = revLogList.FindIndex(r => r.Hash == current);
                    if (index1 == -1)
                    {
                        Console.WriteLine("Current revision index was not found. No need to trim.");
                    }
                    else
                    {
                        var indexList = new List<int>();
                        for (var index2 = 1; index2 < index1; ++index2)
                        {
                            var revLog = revLogList[index2];
                            var keep = false;
                            revLog.Changes.ForEach(ci =>
                            {
                                if (!ci.IsSeed)
                                    return;
                                keep = true;
                            });
                            if (!keep)
                                indexList.Add(index2);
                        }

                        Console.WriteLine("Current Hash: {0} Index:{1}", current, index1);
                        if (indexList.Count <= 0)
                            return;
                        indexList.ForEach(item => Console.WriteLine("Deleting index: {0} ", item));
                        GetRevLog(repositoryName).RemoveAllAt(indexList);
                    }
                }
            }
        }

        private static List<RevLog> GetRevLogList(string name)
        {
            return GetRevLog(name).AsList();
        }

        private static PersistentList<RevLog> GetRevLog(string name)
        {
            return new PersistentList<RevLog>(GetRevlogPath(name));
        }

        private static string ReadCurrentRevisionInternal(string name)
        {
            var currentPath = GetCurrentPath(name);
            if (!File.Exists(currentPath))
                return "0000000000000000000000000000000000000000";
            try
            {
                using (var fileStream = new FileStream(currentPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        return streamReader.ReadToEnd().ToObject<string>();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised in RepositoryService.ReadCurrentRevisionInternal.", ex);
                return "0000000000000000000000000000000000000000";
            }
        }

        private static string GetRepositoryPath(string repositoryName)
        {
            return Path.Combine("C:\\Program Files\\Redbox\\REDS\\Update Manager\\.store\\.repository", repositoryName);
        }

        private static string GetRevlogPath(string repositoryName)
        {
            return Path.Combine(GetRepositoryPath(repositoryName), ".revlog");
        }

        private static string GetCurrentPath(string repositoryName)
        {
            return Path.Combine(GetRepositoryPath(repositoryName), ".current");
        }
    }
}