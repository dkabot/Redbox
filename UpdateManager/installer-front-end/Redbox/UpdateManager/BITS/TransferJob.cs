using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Redbox.UpdateManager.BITS
{
    internal class TransferJob : ITransferJob
    {
        private List<ITransferItem> m_items;

        public ErrorList SetNoProgressTimeout(uint timeout)
        {
            ErrorList errorList = new ErrorList();
            if (timeout < 1U)
                return errorList;
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.SetNoProgressTimeout(timeout);
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error setting no progress time out of job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error setting no progress time out of job  job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList SetMinimumRetryDelay(uint seconds)
        {
            ErrorList errorList = new ErrorList();
            if (seconds < 60U)
                seconds = 60U;
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.SetMinimumRetryDelay(seconds);
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C998", string.Format("Error setting minimum retry delay of job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error setting minimum retry delay of job  job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList TakeOwnership()
        {
            ErrorList ownership = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.TakeOwnership();
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                ownership.Add(Error.NewError("C999", string.Format("Error taking ownership of job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
            }
            catch (Exception ex)
            {
                ownership.Add(Error.NewError("C998", string.Format("Error taking ownership of job  job: {0}.", (object)this.ID), ex));
                ownership.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return ownership;
        }

        public ErrorList AddItem(string url, string file)
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            IEnumBackgroundCopyFiles pEnum = (IEnumBackgroundCopyFiles)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.TakeOwnership();
                ppJob.AddFile(url, file);
                ppJob.EnumFiles(out pEnum);
                this.m_items = TransferJob.GetFiles(pEnum);
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error adding item to job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error Suspend job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
                if (pEnum != null)
                    Marshal.ReleaseComObject((object)pEnum);
            }
            return errorList;
        }

        public ErrorList Complete()
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.Complete();
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error completing job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error Completing job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList Cancel()
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.Cancel();
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error canceling job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error Canceling job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList Suspend()
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.Suspend();
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error suspending job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error Suspend job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList Resume()
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.Resume();
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error resume job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error Suspend job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList SetPriority(TransferJobPriority priority)
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.SetPriority((BG_JOB_PRIORITY)priority);
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error setting job priority on job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error setting priority on job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public ErrorList SetCallback(ITransferCallbackParameters parameters)
        {
            ErrorList errorList = new ErrorList();
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                ppJob.SetNotifyFlags(BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED | BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR);
                string Program = Path.Combine(parameters.Path, parameters.Executable);
                StringBuilder stringBuilder = new StringBuilder();
                foreach (string str in parameters.Arguments)
                    stringBuilder.Append(string.Format(" {0}", (object)str));
                string Parameters = string.Format("{0}{1}", (object)Program, stringBuilder.Length > 0 ? (object)stringBuilder.ToString() : (object)string.Empty);
                ((IBackgroundCopyJob2)ppJob).SetNotifyCmdLine(Program, Parameters);
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errorList.Add(Error.NewError("C999", string.Format("Error setting callback on job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errorList.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errorList.Add(Error.NewError("C998", string.Format("Error setting callback job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
            return errorList;
        }

        public void GetErrors(out ErrorList errors)
        {
            errors = new ErrorList();
            if (this.Status != TransferStatus.TransientError && this.Status != TransferStatus.Error)
                return;
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                Guid id = this.ID;
                o.GetJob(ref id, out ppJob);
                IBackgroundCopyError ppError;
                ppJob.GetError(out ppError);
                string pErrorDescription;
                ppError.GetErrorDescription(0U, out pErrorDescription);
                string pContextDescription;
                ppError.GetErrorContextDescription(0U, out pContextDescription);
                errors.Add(Error.NewError("B999", pErrorDescription, pContextDescription));
            }
            catch (COMException ex)
            {
                string pErrorDescription;
                o.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
                errors.Add(Error.NewError("C999", string.Format("Error getting errors on job: {0}. {1}", (object)this.ID, (object)pErrorDescription), (Exception)ex));
                errors.Add(Error.NewError("B999", "Error accesing BITS job.", (Exception)new Win32Exception(Marshal.GetLastWin32Error())));
            }
            catch (Exception ex)
            {
                errors.Add(Error.NewError("C998", string.Format("Error getting errors on job: {0}.", (object)this.ID), ex));
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
        }

        public ErrorList GetItems(out List<ITransferItem> items)
        {
            items = new List<ITransferItem>((IEnumerable<ITransferItem>)this.m_items.ToArray());
            return new ErrorList();
        }

        public TransferJobType JobType { get; private set; }

        public string Owner { get; private set; }

        public string Name { get; private set; }

        public Guid ID { get; private set; }

        public DateTime StartTime { get; private set; }

        public DateTime ModifiedTime { get; private set; }

        public DateTime? FinishTime { get; private set; }

        public ulong TotalBytesTransfered { get; private set; }

        public ulong TotalBytes { get; private set; }

        public TransferStatus Status { get; private set; }

        internal TransferJob(Guid id, IBackgroundCopyJob job) => this.Initialize(id, job);

        internal TransferJob(Guid id)
        {
            IBackgroundCopyManager o = (IBackgroundCopyManager)new BackgroundCopyManager();
            IBackgroundCopyJob ppJob = (IBackgroundCopyJob)null;
            try
            {
                o.GetJob(ref id, out ppJob);
                this.ID = id;
                this.Initialize(id, ppJob);
            }
            finally
            {
                Marshal.ReleaseComObject((object)o);
                if (ppJob != null)
                    Marshal.ReleaseComObject((object)ppJob);
            }
        }

        private static List<ITransferItem> GetFiles(IEnumBackgroundCopyFiles files)
        {
            uint puCount;
            files.GetCount(out puCount);
            List<ITransferItem> files1 = new List<ITransferItem>();
            for (int index = 0; (long)index < (long)puCount; ++index)
            {
                IBackgroundCopyFile rgelt;
                files.Next(1U, out rgelt, out uint _);
                files1.Add((ITransferItem)new TransferItem(rgelt));
                Marshal.ReleaseComObject((object)rgelt);
            }
            return files1;
        }

        private void Initialize(Guid id, IBackgroundCopyJob job)
        {
            IEnumBackgroundCopyFiles pEnum = (IEnumBackgroundCopyFiles)null;
            try
            {
                string pVal1;
                job.GetOwner(out pVal1);
                this.Owner = pVal1;
                BG_JOB_TIMES pVal2;
                job.GetTimes(out pVal2);
                this.StartTime = pVal2.CreationTime.ToUTCDateTime().Value;
                this.FinishTime = pVal2.TransferCompletionTime.ToUTCDateTime();
                DateTime? utcDateTime = pVal2.ModificationTime.ToUTCDateTime();
                this.ModifiedTime = utcDateTime.HasValue ? utcDateTime.Value : this.StartTime;
                BG_JOB_PROGRESS pVal3;
                job.GetProgress(out pVal3);
                this.TotalBytesTransfered = pVal3.BytesTransferred;
                this.TotalBytes = pVal3.BytesTotal;
                string pVal4;
                job.GetDisplayName(out pVal4);
                this.Name = pVal4;
                job.EnumFiles(out pEnum);
                this.m_items = TransferJob.GetFiles(pEnum);
                BG_JOB_STATE pVal5;
                job.GetState(out pVal5);
                BG_JOB_TYPE pVal6;
                job.GetType(out pVal6);
                switch (pVal6)
                {
                    case BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD:
                        this.JobType = TransferJobType.Download;
                        break;
                    case BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD:
                        this.JobType = TransferJobType.Upload;
                        break;
                }
                this.Status = (TransferStatus)pVal5;
                this.ID = id;
            }
            finally
            {
                if (pEnum != null)
                    Marshal.ReleaseComObject((object)pEnum);
            }
        }
    }
}
