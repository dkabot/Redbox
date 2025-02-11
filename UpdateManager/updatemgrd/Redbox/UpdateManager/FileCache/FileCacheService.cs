using Redbox.Core;
using Redbox.DeltaCompression.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Redbox.UpdateManager.FileCache
{
    internal class FileCacheService : IFileCacheService
    {
        private string _root;

        public static FileCacheService Instance => Singleton<FileCacheService>.Instance;

        public Redbox.UpdateManager.ComponentModel.ErrorList Initialize(string rootPath)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this._root = rootPath;
                if (!Directory.Exists(this._root))
                    Directory.CreateDirectory(this._root);
                LogHelper.Instance.Log("FileCacheService Directory: {0}", (object)rootPath);
                ServiceLocator.Instance.AddService(typeof(IFileCacheService), (object)this);
                LogHelper.Instance.Log("Initialized the FileCacheService", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.Initialize", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public bool FileExists(long fileSetId, long fileId, long fileRevisionId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                return File.Exists(this.GetFilePath(fileSetId, fileId, fileRevisionId));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.FileExists", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool RevisionExists(long fileSetId, long revisionId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                return this.GetAllRevisionFiles(fileSetId, revisionId).Count<string>() > 0;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.RevisionExists", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool RevisionExists(long fileSetId, long revisionId, long patchRevisionId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                return File.Exists(this.GetRevisionPath(fileSetId, revisionId, patchRevisionId));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.RevisionExists", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddFile(
          long fileSetId,
          long fileId,
          long fileRevisionId,
          byte[] data,
          string info)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this.CheckFilePath(fileSetId, fileId);
                File.WriteAllBytes(this.GetFilePath(fileSetId, fileId, fileRevisionId), data);
                File.WriteAllText(this.GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
                LogHelper.Instance.Log(string.Format("FileCacheService.AddFile - Info: {0}", (object)info));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFile", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddFilePatch(
          long fileSetId,
          long fileId,
          long fileRevisionId,
          long filePatchRevisionId,
          byte[] data,
          string info)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this.CheckFilePath(fileSetId, fileId);
                if (!this.GetFile(fileSetId, fileId, filePatchRevisionId, out byte[] _))
                {
                    errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFilePatch", string.Format("Patch source file is missing FileId: {0} FileRevisionId {1}", (object)fileId, (object)filePatchRevisionId)));
                    return errorList;
                }
                using (FileStream source = new FileStream(this.GetFilePath(fileSetId, fileId, filePatchRevisionId), FileMode.Open, FileAccess.Read))
                {
                    source.Position = 0L;
                    using (MemoryStream patch = new MemoryStream(data))
                    {
                        patch.Position = 0L;
                        using (FileStream target = new FileStream(this.GetFilePath(fileSetId, fileId, fileRevisionId), FileMode.OpenOrCreate, FileAccess.Write))
                            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)FileCacheService.ConvertFromXDeltaErrors((List<Redbox.DeltaCompression.Error>)XDeltaHelper.Apply((Stream)source, (Stream)patch, (Stream)target)));
                    }
                }
                File.WriteAllText(this.GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
                LogHelper.Instance.Log(string.Format("FileCacheService.AddFilePatch - Info: {0}", (object)info));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFilePatch", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromXDeltaErrors(
          List<Redbox.DeltaCompression.Error> xDeltaErrors)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
            xDeltaErrors.ForEach((Action<Redbox.DeltaCompression.Error>)(e => errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details))));
            return errors;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddRevision(
          long fileSetId,
          long revisionId,
          long patchRevisionId,
          byte[] data,
          string info)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this.CheckFileSetPath(fileSetId);
                File.WriteAllBytes(this.GetRevisionPath(fileSetId, revisionId, patchRevisionId), data);
                File.WriteAllText(this.GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId), info);
                LogHelper.Instance.Log(string.Format("FileCacheService.AddRevision - Info: {0}", (object)info));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddRevision", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddFile(
          long fileSetId,
          long fileId,
          long fileRevisionId,
          string pathToMoveFrom)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this.CheckFilePath(fileSetId, fileId);
                string filePath = this.GetFilePath(fileSetId, fileId, fileRevisionId);
                File.Move(pathToMoveFrom, filePath);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFile", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public Redbox.UpdateManager.ComponentModel.ErrorList AddRevision(
          long fileSetId,
          long revisionId,
          long patchRevisionId,
          string pathToMoveFrom)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                this.CheckFileSetPath(fileSetId);
                string revisionPath = this.GetRevisionPath(fileSetId, revisionId, patchRevisionId);
                File.Move(pathToMoveFrom, revisionPath);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddRevision", "Unhandled exception occurred.", ex));
            }
            return errorList;
        }

        public void DeleteFile(long fileSetId, long fileId, long fileRevisionId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                string filePath = this.GetFilePath(fileSetId, fileId, fileRevisionId);
                if ((int)(DateTime.Now - File.GetCreationTime(filePath)).TotalDays <= 1)
                {
                    LogHelper.Instance.Log("FileCacheService.DeleteFile - Cannot delete files less than 2 days - FileSetId {0}, FileId {1}, FileRevisionId {2}", (object)fileSetId, (object)fileId, (object)fileRevisionId);
                }
                else
                {
                    LogHelper.Instance.Log("FileCacheService.DeleteFile - FileSetId {0}, FileId {1}, FileRevisionId {2}", (object)fileSetId, (object)fileId, (object)fileRevisionId);
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    string fileInfoPath = this.GetFileInfoPath(fileSetId, fileId, fileRevisionId);
                    if (!File.Exists(fileInfoPath))
                        return;
                    File.Delete(fileInfoPath);
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.DeleteFile", "Unhandled exception occurred.", ex));
            }
        }

        public void DeleteRevision(long fileSetId, long revisionId, long patchRevisionId)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                LogHelper.Instance.Log("FileCacheService.DeleteRevision - FileSetId {0}, RevisionId {1}, PatchRevisionId {2}", (object)fileSetId, (object)revisionId, (object)patchRevisionId);
                string revisionPath = this.GetRevisionPath(fileSetId, revisionId, patchRevisionId);
                if (File.Exists(revisionPath))
                    File.Delete(revisionPath);
                string revisionInfoPath = this.GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId);
                if (!File.Exists(revisionInfoPath))
                    return;
                File.Delete(revisionInfoPath);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.DeleteRevision", "Unhandled exception occurred.", ex));
            }
        }

        public bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data)
        {
            data = (byte[])null;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                string filePath = this.GetFilePath(fileSetId, fileId, fileRevisionId);
                if (File.Exists(filePath))
                {
                    data = File.ReadAllBytes(filePath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetFile", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool GetFileInfo(string path, out byte[] data)
        {
            data = (byte[])null;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                if (File.Exists(path))
                {
                    data = File.ReadAllBytes(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetFileInfo", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool CopyFileToPath(
          long fileSetId,
          long fileId,
          long fileRevisionId,
          string copyToPath)
        {
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                string filePath = this.GetFilePath(fileSetId, fileId, fileRevisionId);
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, copyToPath, true);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.CopyFileToPath", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool GetRevision(long fileSetId, long revisionId, out byte[] data)
        {
            data = (byte[])null;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                List<string> allRevisionFiles = this.GetAllRevisionFiles(fileSetId, revisionId);
                if (allRevisionFiles.Count<string>() == 0)
                    return false;
                string path = allRevisionFiles.OrderByDescending<string, string>((Func<string, string>)(f => f)).FirstOrDefault<string>((Func<string, bool>)(f => Path.GetExtension(f).Equals(".revision", StringComparison.CurrentCultureIgnoreCase)));
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return false;
                data = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool GetRevision(
          long fileSetId,
          long revisionId,
          long patchRevisionId,
          out byte[] data)
        {
            data = (byte[])null;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                string revisionPath = this.GetRevisionPath(fileSetId, revisionId, patchRevisionId);
                if (File.Exists(revisionPath))
                {
                    data = File.ReadAllBytes(revisionPath);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public bool GetRevision(string path, out byte[] data)
        {
            data = (byte[])null;
            Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
            try
            {
                if (File.Exists(path))
                {
                    data = File.ReadAllBytes(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", ex));
            }
            return false;
        }

        public List<string> GetAllRevisionFiles(long fileSetId)
        {
            return ((IEnumerable<string>)Directory.GetFiles(this.GetRevisionPath(fileSetId), "*.revision", SearchOption.TopDirectoryOnly)).ToList<string>();
        }

        public List<long> GetAllFileSets()
        {
            List<long> fileSets = new List<long>();
            ((IEnumerable<string>)Directory.GetDirectories(this._root)).ForEach<string>((Action<string>)(d =>
            {
                string fileName = Path.GetFileName(d);
                try
                {
                    fileSets.Add(Convert.ToInt64(fileName));
                }
                catch
                {
                }
            }));
            return fileSets;
        }

        private List<string> GetAllRevisionFiles(long fileSetId, long revisionId)
        {
            return ((IEnumerable<string>)Directory.GetFiles(this.GetRevisionPath(fileSetId), string.Format("{0}-*.revision", (object)revisionId), SearchOption.TopDirectoryOnly)).ToList<string>();
        }

        public List<string> GetAllFiles(long fileSetId)
        {
            return ((IEnumerable<string>)Directory.GetFiles(this.GetFilePath(fileSetId), "*.file", SearchOption.TopDirectoryOnly)).ToList<string>();
        }

        public List<string> GetAllFileInfos(long fileSetId)
        {
            return ((IEnumerable<string>)Directory.GetFiles(this.GetFilePath(fileSetId), "*.fileinfo", SearchOption.TopDirectoryOnly)).ToList<string>();
        }

        private string GetRevisionPath(long fileSetId)
        {
            return Path.Combine(this._root, string.Format("{0}\\", (object)fileSetId));
        }

        private string GetRevisionPath(long fileSetId, long revisionId, long patchRevisionId)
        {
            return Path.Combine(this._root, string.Format("{0}\\{1}", (object)fileSetId, (object)this.GetRevisionName(revisionId, patchRevisionId)));
        }

        private string GetRevisionInfoPath(long fileSetId, long revisionId, long patchRevisionId)
        {
            return Path.Combine(this._root, string.Format("{0}\\{1}", (object)fileSetId, (object)this.GetRevisionInfoName(revisionId, patchRevisionId)));
        }

        private string GetRevisionName(long revisionId, long patchRevisionId)
        {
            return string.Format("{0}-{1}.revision", (object)revisionId, (object)patchRevisionId);
        }

        private string GetRevisionInfoName(long revisionId, long patchRevisionId)
        {
            return string.Format("{0}-{1}.revisioninfo", (object)revisionId, (object)patchRevisionId);
        }

        private string GetFilePath(long fileSetId)
        {
            return Path.Combine(this._root, string.Format("{0}\\File\\", (object)fileSetId));
        }

        private string GetFilePath(long fileSetId, long fileId, long fileRevisionId)
        {
            return Path.Combine(this._root, string.Format("{0}\\File\\{1}", (object)fileSetId, (object)this.GetFileName(fileId, fileRevisionId)));
        }

        private string GetFileInfoPath(long fileSetId, long fileId, long fileRevisionId)
        {
            return Path.Combine(this._root, string.Format("{0}\\File\\{1}", (object)fileSetId, (object)this.GetFileInfoName(fileId, fileRevisionId)));
        }

        public string GetFileName(long fileId, long fileRevisionId)
        {
            return string.Format("{0}-{1}.file", (object)fileId, (object)fileRevisionId);
        }

        private string GetFileInfoName(long fileId, long fileRevisionId)
        {
            return string.Format("{0}-{1}.fileinfo", (object)fileId, (object)fileRevisionId);
        }

        private void CheckFilePath(long fileSetId, long fileId)
        {
            string path = Path.Combine(this._root, string.Format("{0}\\File", (object)fileSetId));
            if (Directory.Exists(path))
                return;
            Directory.CreateDirectory(path);
        }

        private void CheckFileSetPath(long fileSetId)
        {
            string path = Path.Combine(this._root, string.Format("{0}", (object)fileSetId));
            if (Directory.Exists(path))
                return;
            Directory.CreateDirectory(path);
        }

        private FileCacheService()
        {
        }
    }
}
