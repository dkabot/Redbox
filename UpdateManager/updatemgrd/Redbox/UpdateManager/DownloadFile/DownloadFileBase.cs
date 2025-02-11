using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.UpdateManager.DownloadFile
{
    internal class DownloadFileBase : IDownloadFile
    {
        private IDownloadFileData _downloadFileData;
        private const string ResultTableName = "Results";
        private const string MarkScriptCompletedText = "kernel.markscriptcomplete";

        public DownloadFileBase(IDownloadFileData downloadFileData)
        {
            this.DownloadFileData = downloadFileData;
        }

        public IDownloadFileData DownloadFileData
        {
            get => this._downloadFileData;
            private set => this._downloadFileData = value;
        }

        public virtual ErrorList ProcessDownloadFile() => new ErrorList();

        public virtual ErrorList DeleteDownloadFile()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                DownloadFileFactory.Instance.DeleteDownloadFile(this.DownloadFileData);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFB001", "Unhandled exception occurred in DownloadFilebase.DeleteDownloadFile", ex));
            }
            return errorList;
        }

        public virtual ErrorList SaveDownloadFile()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                DownloadFileFactory.Instance.SaveDownloadFile(this.DownloadFileData);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFB001", "Unhandled exception occurred in DownloadFilebase.SaveDownloadFile", ex));
            }
            return errorList;
        }

        internal ErrorList SendDownloadFileStatus(
          StatusMessage.StatusMessageType statusMessageType,
          string description,
          string message)
        {
            var data = new
            {
                DownloadFileId = this.DownloadFileData.Id,
                Name = this.DownloadFileData.Name,
                DownloadFileDataType = this.DownloadFileData.DownloadFileDataType.ToString(),
                DownloadFileDataState = this.DownloadFileData.DownloadFileDataState.ToString(),
                Message = message
            };
            return this.SendStatus(statusMessageType, description, (object)data);
        }

        internal ErrorList SendResult(StatusMessage.StatusMessageType statusMessageType, string result)
        {
            return this.SendStatus(statusMessageType, "DownloadFile script result", (object)result);
        }

        internal ErrorList SendStatus(
          StatusMessage.StatusMessageType statusMessageType,
          string description,
          object data)
        {
            return DownloadFileFactory.Instance.SendStatus(statusMessageType, this.DownloadFileData.StatusKey, description, data);
        }

        internal bool IsValidTime()
        {
            TimeSpan result1;
            TimeSpan result2;
            if (string.IsNullOrEmpty(this.DownloadFileData.StartTime) || string.IsNullOrEmpty(this.DownloadFileData.EndTime) || !TimeSpan.TryParse(this.DownloadFileData.StartTime, out result1) || !TimeSpan.TryParse(this.DownloadFileData.EndTime, out result2) || result1 == result2)
                return true;
            DateTime now = DateTime.Now;
            return result2 < result1 ? now.TimeOfDay <= result2 || now.TimeOfDay >= result1 : now.TimeOfDay >= result1 && now.TimeOfDay <= result2;
        }

        internal ErrorList RunScript(out bool scriptCompleted)
        {
            ErrorList errorList = new ErrorList();
            scriptCompleted = true;
            if (string.IsNullOrEmpty(this.DownloadFileData.ActivateScript))
                return errorList;
            IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
            string result1 = string.Empty;
            string chunk = Encoding.Unicode.GetString(this.DownloadFileData.ActivateScript.Base64ToBytes());
            LogHelper.Instance.Log("DownloadFileBase.RunScript: DownloadFile Id {0} Name {1} running attached script", (object)this.DownloadFileData.Id, (object)this.DownloadFileData.Name);
            Exception exception = (Exception)null;
            try
            {
                Dictionary<object, object> result2;
                service.ExecuteChunkNoLock(chunk, true, "Results", out result2, out scriptCompleted);
                if (result2 != null)
                    result1 = result2.ToJson();
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("DFS9999", string.Format("DownloadFileBase.RunScript: unhandled exception in script. Script for download file Id {0} Name {1}", (object)this.DownloadFileData.Id, (object)this.DownloadFileData.Name), ex));
                exception = ex;
            }
            if (exception != null)
            {
                var instance = new
                {
                    Result = false,
                    Exception = true,
                    Message = exception.Message
                };
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendResult(StatusMessage.StatusMessageType.Error, instance.ToJson()));
                return errorList;
            }
            bool flag = chunk.IndexOf("kernel.markscriptcomplete", StringComparison.CurrentCultureIgnoreCase) > -1;
            if (service.ScriptCompleted || !flag)
            {
                scriptCompleted = true;
                if (!string.IsNullOrEmpty(result1))
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendResult(StatusMessage.StatusMessageType.Info, result1));
            }
            else
            {
                scriptCompleted = false;
                LogHelper.Instance.Log("DownloadFile Id {0} Name {1} script is incomplete.", (object)this.DownloadFileData.Id, (object)this.DownloadFileData.Name);
            }
            return errorList;
        }
    }
}
