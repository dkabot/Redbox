using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.Transfer
{
    public class TransferJob : ITransferJob
    {
        private List<ITransferItem> m_items;

        internal TransferJob(Guid id, IBackgroundCopyJob job)
        {
            Initialize(id, job);
        }

        internal TransferJob(Guid id)
        {
            var o = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                o.GetJob(ref id, out ppJob);
                ID = id;
                Initialize(id, ppJob);
            }
            finally
            {
                Marshal.ReleaseComObject(o);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }
        }

        public List<Error> SetNoProgressTimeout(uint timeout)
        {
            var errors = new List<Error>();
            if (timeout < 1U)
                return errors;
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.SetNoProgressTimeout(timeout);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error setting no progress timeout of job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error setting no progress timeout of job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> SetMinimumRetryDelay(uint seconds)
        {
            var errors = new List<Error>();
            if (seconds < 60U)
                seconds = 60U;
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.SetMinimumRetryDelay(seconds);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error setting minimum retry delay of job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error setting minimum retry delay of job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> TakeOwnership()
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.TakeOwnership();
            }
            catch (COMException ex)
            {
                HandleCOMException("Error taking ownership of job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error taking ownership of job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> AddItem(string url, string file)
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            var pEnum = (IEnumBackgroundCopyFiles)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.TakeOwnership();
                ppJob.AddFile(url, file);
                ppJob.EnumFiles(out pEnum);
                m_items = GetFiles(pEnum);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error adding item to job. Given url: " + url + " Given file path: " + file,
                    backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error adding item to job. Given url: " + url + " Given file path: " + file,
                    errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
                if (pEnum != null)
                    Marshal.ReleaseComObject(pEnum);
            }

            return errors;
        }

        public List<Error> Complete()
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.Complete();
            }
            catch (COMException ex)
            {
                HandleCOMException("Error completing job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error completing job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> Cancel()
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.Cancel();
            }
            catch (COMException ex)
            {
                HandleCOMException("Error canceling job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error canceling job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> Suspend()
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.Suspend();
            }
            catch (COMException ex)
            {
                HandleCOMException("Error suspending job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error suspending job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> Resume()
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.Resume();
            }
            catch (COMException ex)
            {
                HandleCOMException("Error resuming job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error resuming job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> SetPriority(TransferJobPriority priority)
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.SetPriority((BG_JOB_PRIORITY)priority);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error setting job priority on job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error setting job priority on job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> SetCallback(ITransferCallbackParameters parameters)
        {
            var errors = new List<Error>();
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                ppJob.SetNotifyFlags(BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_TRANSFERRED |
                                     BG_JOB_NOTIFICATION_TYPE.BG_NOTIFY_JOB_ERROR);
                var Program = Path.Combine(parameters.Path, parameters.Executable);
                var stringBuilder = new StringBuilder();
                foreach (var str in parameters.Arguments)
                    stringBuilder.Append(string.Format(" {0}", str));
                var Parameters = string.Format("{0}{1}", Program,
                    stringBuilder.Length > 0 ? stringBuilder.ToString() : (object)string.Empty);
                ((IBackgroundCopyJob2)ppJob).SetNotifyCmdLine(Program, Parameters);
            }
            catch (COMException ex)
            {
                HandleCOMException("Error setting callback on job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error setting callback on job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> GetErrors()
        {
            var errors = new List<Error>();
            if (Status != TransferStatus.TransientError && Status != TransferStatus.Error)
                return errors;
            var backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
            var ppJob = (IBackgroundCopyJob)null;
            try
            {
                var id = ID;
                backgroundCopyManager.GetJob(ref id, out ppJob);
                IBackgroundCopyError ppError;
                ppJob.GetError(out ppError);
                string pErrorDescription;
                ppError.GetErrorDescription(0U, out pErrorDescription);
                string pContextDescription;
                ppError.GetErrorContextDescription(0U, out pContextDescription);
                errors.Add(new Error
                {
                    Code = "B999",
                    Message = pErrorDescription + ". " + pContextDescription
                });
            }
            catch (COMException ex)
            {
                HandleCOMException("Error getting errors on job", backgroundCopyManager, errors, ex);
            }
            catch (Exception ex)
            {
                HandleGenericException("Error getting errors on job", errors, ex);
            }
            finally
            {
                Marshal.ReleaseComObject(backgroundCopyManager);
                if (ppJob != null)
                    Marshal.ReleaseComObject(ppJob);
            }

            return errors;
        }

        public List<Error> GetItems(out List<ITransferItem> items)
        {
            items = new List<ITransferItem>(m_items.ToArray());
            return new List<Error>();
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

        private static List<ITransferItem> GetFiles(IEnumBackgroundCopyFiles files)
        {
            uint puCount;
            files.GetCount(out puCount);
            var files1 = new List<ITransferItem>();
            for (var index = 0; index < puCount; ++index)
            {
                IBackgroundCopyFile rgelt;
                files.Next(1U, out rgelt, out var _);
                files1.Add(new TransferItem(rgelt));
                Marshal.ReleaseComObject(rgelt);
            }

            return files1;
        }

        private void HandleCOMException(
            string baseMessage,
            IBackgroundCopyManager manager,
            List<Error> errors,
            COMException ex)
        {
            string pErrorDescription;
            manager.GetErrorDescription(ex.ErrorCode, 0U, out pErrorDescription);
            errors.AddRange(new List<Error>
            {
                new Error
                {
                    Code = "C999",
                    Message = string.Format("{0}: {1}. {2}. Exception -> {3}", baseMessage, ID, pErrorDescription,
                        ex.GetFullMessage())
                },
                GetLastWin32Error()
            });
        }

        private void HandleGenericException(string baseMessage, List<Error> errors, Exception e)
        {
            errors.Add(new Error
            {
                Code = "C998",
                Message = string.Format("{0}: {1} -> Exception {2}", baseMessage, ID, e.GetFullMessage())
            });
        }

        private static Error GetLastWin32Error()
        {
            return new Error
            {
                Code = "B999",
                Message = "Error accessing BITS job. Exception -> " +
                          new Win32Exception(Marshal.GetLastWin32Error()).GetFullMessage()
            };
        }

        private void Initialize(Guid id, IBackgroundCopyJob job)
        {
            var pEnum = (IEnumBackgroundCopyFiles)null;
            try
            {
                string pVal1;
                job.GetOwner(out pVal1);
                Owner = pVal1;
                BG_JOB_TIMES pVal2;
                job.GetTimes(out pVal2);
                StartTime = pVal2.CreationTime.ToUTCDateTime().Value;
                FinishTime = pVal2.TransferCompletionTime.ToUTCDateTime();
                ModifiedTime = pVal2.ModificationTime.ToUTCDateTime() ?? StartTime;
                BG_JOB_PROGRESS pVal3;
                job.GetProgress(out pVal3);
                TotalBytesTransfered = pVal3.BytesTransferred;
                TotalBytes = pVal3.BytesTotal;
                string pVal4;
                job.GetDisplayName(out pVal4);
                Name = pVal4;
                job.EnumFiles(out pEnum);
                m_items = GetFiles(pEnum);
                BG_JOB_STATE pVal5;
                job.GetState(out pVal5);
                BG_JOB_TYPE pVal6;
                job.GetType(out pVal6);
                switch (pVal6)
                {
                    case BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD:
                        JobType = TransferJobType.Download;
                        break;
                    case BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD:
                        JobType = TransferJobType.Upload;
                        break;
                }

                Status = (TransferStatus)pVal5;
                if (Status == TransferStatus.Error)
                {
                    job.GetError(out var _);
                    job.GetErrorCount(out var _);
                }

                ID = id;
            }
            finally
            {
                if (pEnum != null)
                    Marshal.ReleaseComObject(pEnum);
            }
        }
    }
}