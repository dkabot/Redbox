using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class StatusMessageService : IPollRequestReply, IStatusMessageService
    {
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _lockTimeout = 3000;
        private static string QueueLabel = "StatusMessage";

        public static StatusMessageService Instance => Singleton<StatusMessageService>.Instance;

        public void Initialize()
        {
        }

        public ErrorList EnqueueMessage(
          StatusMessage.StatusMessageType type,
          string key,
          string subKey,
          string description,
          string data,
          bool encodeData)
        {
            StatusMessage entry = new StatusMessage()
            {
                Type = type,
                Key = key,
                SubKey = subKey,
                Description = description,
                Data = data,
                TimeStamp = DateTime.UtcNow,
                Encode = encodeData
            };
            if (encodeData)
                entry.EncodeData();
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                ErrorList errorList = new ErrorList();
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService.EnqueueMessage", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                return errorList;
            }
            try
            {
                ServiceLocator.Instance.GetService<IQueueService>().Enqueue(StatusMessageService.QueueLabel, (object)entry);
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
            return new ErrorList();
        }

        public ErrorList EnqueueMessage(
          StatusMessage.StatusMessageType type,
          string key,
          string description,
          string data,
          bool encodeData)
        {
            return this.EnqueueMessage(type, key, (string)null, description, data, encodeData);
        }

        public ErrorList EnqueueMessage(
          StatusMessage.StatusMessageType type,
          string key,
          string subKey,
          string description,
          string data)
        {
            return this.EnqueueMessage(type, key, subKey, description, data, true);
        }

        public ErrorList EnqueueMessage(
          StatusMessage.StatusMessageType type,
          string key,
          string description,
          string data)
        {
            return this.EnqueueMessage(type, key, (string)null, description, data, true);
        }

        public ErrorList GetPollRequest(out List<PollRequest> pollRequests)
        {
            pollRequests = new List<PollRequest>();
            ErrorList pollRequest1 = new ErrorList();
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StatusMessageService.GetPollRequest", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                return pollRequest1;
            }
            try
            {
                IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
                for (StatusMessage instance = service.Peek<StatusMessage>(StatusMessageService.QueueLabel); instance != null; instance = service.Peek<StatusMessage>(StatusMessageService.QueueLabel))
                {
                    PollRequest pollRequest2 = new PollRequest()
                    {
                        PollRequestType = PollRequestType.StatusMessage,
                        Data = instance.ToJson()
                    };
                    pollRequests.Add(pollRequest2);
                    LogHelper.Instance.Log("Dequeued status message key: {0}", (object)instance.Key);
                    service.Dequeue(StatusMessageService.QueueLabel);
                }
            }
            catch (Exception ex)
            {
                pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError(nameof(StatusMessageService), "Unhandled exception occurred in GetPollRequest.", ex));
            }
            finally
            {
                this._lock.ExitWriteLock();
            }
            return pollRequest1;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList) => new ErrorList();

        private StatusMessageService()
        {
            ServiceLocator.Instance.AddService(typeof(IStatusMessageService), (object)this);
        }
    }
}
