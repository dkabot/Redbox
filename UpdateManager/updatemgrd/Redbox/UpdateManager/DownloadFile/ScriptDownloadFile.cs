using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;

namespace Redbox.UpdateManager.DownloadFile
{
    internal class ScriptDownloadFile : DownloadFileBase
    {
        private const string DownloadFileTypeName = "Script download";

        public ScriptDownloadFile(IDownloadFileData downloadFileData) : base(downloadFileData)
        {
        }

        public override ErrorList ProcessDownloadFile()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.None)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckNone());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostInstall)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckPostInstall());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Complete)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckComplete());
            if (this.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Error)
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.CheckError());
            return errorList;
        }

        private ErrorList CheckNone()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.None)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Script download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            try
            {
                if (string.IsNullOrEmpty(this.DownloadFileData.ActivateScript))
                {
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Script cannot be empty.", (string)null));
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostInstall;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Script is available pending execution", (string)null));
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in ScriptDownloadFile.CheckNone", ex));
            }
            return errorList;
        }

        private ErrorList CheckPostInstall()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostInstall)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Script download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            if (!this.IsValidTime())
            {
                LogHelper.Instance.Log("ScriptDownloadFile: CheckPostInstall is skipping download file {0} until start: {1} and end time: {2} are valid.", (object)this.DownloadFileData.Name, (object)this.DownloadFileData.StartTime, (object)this.DownloadFileData.EndTime);
                return errorList;
            }
            if (!string.IsNullOrEmpty(this.DownloadFileData.ActivateScript))
            {
                bool scriptCompleted;
                errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.RunScript(out scriptCompleted));
                if (errorList.ContainsError())
                {
                    this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
                    this.DownloadFileData.Message = errorList[0].Description;
                    errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
                    return errorList;
                }
                if (!scriptCompleted)
                    return errorList;
            }
            this.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SaveDownloadFile());
            return errorList;
        }

        private ErrorList CheckComplete()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Complete)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Script download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile success", (string)null));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DeleteDownloadFile());
            return errorList;
        }

        private ErrorList CheckError()
        {
            ErrorList errorList = new ErrorList();
            if (this.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Error)
                return errorList;
            LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", (object)"Script download", (object)this.DownloadFileData.DownloadFileDataState.ToString(), (object)this.DownloadFileData.Name));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile error", this.DownloadFileData.Message));
            errorList.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)this.DeleteDownloadFile());
            return errorList;
        }
    }
}
