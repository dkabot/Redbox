using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.FileSets
{
    internal class FileSetCleanup
    {
        public ErrorList Run()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("Starting FileSetCleanup");
                List<StateFile> stateFiles;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)StateFile.GetAll(out stateFiles));
                if (errorList.ContainsError())
                    return errorList;
                FileCacheService.Instance.GetAllFileSets().ForEach((Action<long>)(fileSetId =>
                {
                    List<long> revisionIds = new List<long>();
                    stateFiles.ForEach((Action<StateFile>)(sf =>
            {
                      if (sf.FileSetId != fileSetId)
                          return;
                      if (sf.HasActive())
                          revisionIds.Add(sf.RevisionId);
                      if (!sf.HasInProgress())
                          return;
                      revisionIds.Add(sf.InProgressRevisionId);
                  }));
                    revisionIds = revisionIds.Distinct<long>().ToList<long>();
                    if (revisionIds.Count == 0)
                        return;
                    List<ClientFileSetRevision> revisions = new List<ClientFileSetRevision>();
                    revisionIds.ForEach((Action<long>)(r =>
            {
                      ClientFileSetRevision revision = FileSetService.Instance.GetRevision(fileSetId, r);
                      if (revision == null)
                          return;
                      revisions.Add(revision);
                  }));
                    if (revisions.Count == 0)
                        return;
                    ClientFileSetRevision clientFileSetRevision = revisions.Last<ClientFileSetRevision>();
                    int retentionDays = clientFileSetRevision.RetentionDays;
                    int retentionRevisions = clientFileSetRevision.RetentionRevisions;
                    if (retentionDays == 0 && retentionRevisions == 0)
                        return;
                    List<FileSetFileInfo> allFileInfos = new List<FileSetFileInfo>();
                    FileCacheService.Instance.GetAllFileInfos(fileSetId).ForEach((Action<string>)(path => allFileInfos.Add(FileSetService.Instance.GetFileInfo(path))));
                    List<FileSetFileInfo> requiredFiles = new List<FileSetFileInfo>();
                    List<string> list = FileCacheService.Instance.GetAllRevisionFiles(fileSetId).OrderByDescending<string, string>((Func<string, string>)(s => s)).ToList<string>();
                    int pos = 1;
                    Action<string> action = (Action<string>)(path =>
            {
                      ClientFileSetRevision revision = FileSetService.Instance.GetRevision(path);
                      if (revision == null)
                          return;
                      bool flag = true;
                      try
                      {
                          if (revisions.Exists((Predicate<ClientFileSetRevision>)(r => r.RevisionId == revision.RevisionId)))
                              return;
                          bool? nullable4 = new bool?();
                          bool? nullable5 = new bool?();
                          if (retentionRevisions > 0 && pos > retentionRevisions)
                              nullable4 = new bool?(true);
                          int totalDays = (int)(DateTime.Now - File.GetCreationTime(path)).TotalDays;
                          if (totalDays <= 1)
                              return;
                          if (retentionDays > 0 && totalDays > retentionDays)
                              nullable5 = new bool?(true);
                          bool? nullable6 = nullable4;
                          if (((nullable6 ?? false) ? 1 : 0) != 0)
                          {
                              nullable6 = nullable5;
                              if (((nullable6 ?? false) ? 1 : 0) != 0)
                              {
                                  flag = false;
                                  LogHelper.Instance.Log(string.Format("Deleting FileSetId: {0} RevisionId {1} PatchRevisionId {2}", (object)fileSetId, (object)revision.RevisionId, (object)revision.PatchRevisionId), LogEntryType.Debug);
                                  FileCacheService.Instance.DeleteRevision(fileSetId, revision.RevisionId, revision.PatchRevisionId);
                              }
                          }
                      }
                      finally
                      {
                          if (flag)
                              this.AddRequiredFiles(allFileInfos, requiredFiles, revision);
                      }
                      ++pos;
                  });
                    list.ForEach(action);
                    this.RemoveFiles(fileSetId, allFileInfos, requiredFiles);
                }));
                LogHelper.Instance.Log("Finished FileSetCleanup");
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetCleanup.Run", "Unhandled exception occurred", ex));
            }
            return errorList;
        }

        private void AddRequiredFiles(
          List<FileSetFileInfo> allFileInfos,
          List<FileSetFileInfo> requiredFiles,
          ClientFileSetRevision revision)
        {
            List<FileSetFileInfo> list1 = revision.Files.Join((IEnumerable<FileSetFileInfo>)allFileInfos, a => new
            {
                FileId = a.FileId,
                FileRevisionId = a.FileRevisionId
            }, b => new
            {
                FileId = b.FileId,
                FileRevisionId = b.FileRevisionId
            }, (Func<ClientFileSetFile, FileSetFileInfo, FileSetFileInfo>)((a, b) => b)).ToList<FileSetFileInfo>();
            requiredFiles.AddRange((IEnumerable<FileSetFileInfo>)list1);
            List<FileSetFileInfo> list2 = revision.PatchFiles.Join((IEnumerable<FileSetFileInfo>)allFileInfos, a => new
            {
                FileId = a.FileId,
                FileRevisionId = a.PatchFileRevisionId
            }, b => new
            {
                FileId = b.FileId,
                FileRevisionId = b.FileRevisionId
            }, (Func<ClientPatchFileSetFile, FileSetFileInfo, FileSetFileInfo>)((a, b) => b)).ToList<FileSetFileInfo>();
            requiredFiles.AddRange((IEnumerable<FileSetFileInfo>)list2);
        }

        private void RemoveFiles(
          long fileSetId,
          List<FileSetFileInfo> allFileInfos,
          List<FileSetFileInfo> requiredFiles)
        {
            allFileInfos.Except<FileSetFileInfo>(requiredFiles.Distinct<FileSetFileInfo>()).ToList<FileSetFileInfo>().ForEach((Action<FileSetFileInfo>)(r =>
            {
                LogHelper.Instance.Log(string.Format("Deleting FileSetId: {0} FileId {1} FileRevisionId {2}", (object)fileSetId, (object)r.FileId, (object)r.FileRevisionId), LogEntryType.Debug);
                FileCacheService.Instance.DeleteFile(fileSetId, r.FileId, r.FileRevisionId);
            }));
        }
    }
}
