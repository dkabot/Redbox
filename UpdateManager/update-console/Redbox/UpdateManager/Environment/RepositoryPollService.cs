using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class RepositoryPollService : IPollRequestReply
    {
        private string m_previousClientRepositoryDataDTOHash = string.Empty;
        private List<ClientRepositoryDataDTO> m_previousClientRepositoryDataDTO = new List<ClientRepositoryDataDTO>();
        private int m_syncId;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _lockTimeout = 3000;

        public static RepositoryPollService Instance => Singleton<RepositoryPollService>.Instance;

        public ErrorList GetPollRequest(out List<PollRequest> pollRequestList)
        {
            ErrorList pollRequest1 = new ErrorList();
            try
            {
                pollRequestList = new List<PollRequest>();
                if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                {
                    pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("RepositoryPollService.GetPollRequest", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                    return pollRequest1;
                }
                try
                {
                    List<ClientRepositoryDataDTO> currentClientRepositoryDataDTO;
                    pollRequest1.AddRange(RepositoryPollService.GetClientRepositoryDataDTO(out currentClientRepositoryDataDTO));
                    string str = RepositoryDiffMerge.Hash(currentClientRepositoryDataDTO);
                    if (this.m_syncId == 0 || string.IsNullOrEmpty(this.m_previousClientRepositoryDataDTOHash))
                    {
                        this.m_syncId = 1;
                        RepositoryDiffWrapper wrapper = new RepositoryDiffWrapper()
                        {
                            O = str,
                            A = currentClientRepositoryDataDTO
                        };
                        PollRequest pollRequest2 = new PollRequest()
                        {
                            PollRequestType = PollRequestType.Repositories,
                            SyncId = 1,
                            Data = RepositoryDiffMerge.Minimize(wrapper)
                        };
                        pollRequestList.Add(pollRequest2);
                        this.m_previousClientRepositoryDataDTO = currentClientRepositoryDataDTO;
                        this.m_previousClientRepositoryDataDTOHash = str;
                        return pollRequest1;
                    }
                    if (str != this.m_previousClientRepositoryDataDTOHash)
                    {
                        RepositoryDiffWrapper rdw;
                        if (!RepositoryDiffMerge.TryDiff(this.m_previousClientRepositoryDataDTO, currentClientRepositoryDataDTO, true, out rdw))
                        {
                            this.m_syncId = 0;
                            pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewWarning("RPS01", "TryDiff failed", "Resetting SyncID and sending up a full sync"));
                            return pollRequest1;
                        }
                        ++this.m_syncId;
                        PollRequest pollRequest3 = new PollRequest()
                        {
                            PollRequestType = PollRequestType.Repositories,
                            SyncId = this.m_syncId,
                            Data = RepositoryDiffMerge.Minimize(rdw)
                        };
                        pollRequestList.Add(pollRequest3);
                        this.m_previousClientRepositoryDataDTOHash = str;
                        this.m_previousClientRepositoryDataDTO = currentClientRepositoryDataDTO;
                    }
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                pollRequest1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("RPS002", "There was an unhandled exception in RepositoryPollService.GetPollRequest", ex));
                pollRequestList = new List<PollRequest>();
            }
            return pollRequest1;
        }

        public ErrorList ProcessPollReply(List<PollReply> pollReplyList)
        {
            ErrorList errorList = new ErrorList();
            try
            {
                LogHelper.Instance.Log("RepositoryService.ProcessPollReply", LogEntryType.Debug);
                if (pollReplyList == null || !pollReplyList.Any<PollReply>())
                {
                    LogHelper.Instance.Log("No pollReplyList in RepositoryService.ProcessPollReply", LogEntryType.Debug);
                    return errorList;
                }
                LogHelper.Instance.Log("PollReplyList in RepositoryService.ProcessPollReply: " + pollReplyList.ToJson(), LogEntryType.Debug);
                if (pollReplyList.Any<PollReply>((Func<PollReply, bool>)(pr => pr.PollReplyType == PollReplyType.RepositoryChangeSet && pr.SyncId == 0 && string.IsNullOrEmpty(pr.Data) || pr.PollReplyType == PollReplyType.UpdateRepositories)))
                {
                    LogHelper.Instance.Log("Server requested full sync", LogEntryType.Info);
                    if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                    {
                        errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("RepositoryPollService.ProcessPollReply", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                        return errorList;
                    }
                    try
                    {
                        this.m_syncId = 0;
                    }
                    finally
                    {
                        this._lock.ExitWriteLock();
                    }
                }
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("RPS003", "There was an unhandled exception in RepositoryPollService.ProcessPollReply", ex));
            }
            return errorList;
        }

        private RepositoryPollService()
        {
        }

        private static IEnumerable<Redbox.UpdateManager.ComponentModel.Error> GetClientRepositoryDataDTO(
          out List<ClientRepositoryDataDTO> currentClientRepositoryDataDTO)
        {
            ErrorList repositoryDataDto1 = new ErrorList();
            currentClientRepositoryDataDTO = new List<ClientRepositoryDataDTO>();
            try
            {
                IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
                ITransferService service1 = ServiceLocator.Instance.GetService<ITransferService>();
                List<string> allRepositories = repositoryService.GetAllRepositories();
                currentClientRepositoryDataDTO = allRepositories.Select<string, ClientRepositoryDataDTO>((Func<string, ClientRepositoryDataDTO>)(repositoryName => new ClientRepositoryDataDTO()
                {
                    R = repositoryName,
                    H = repositoryService.GetHeadRevision(repositoryName),
                    A = repositoryService.GetActiveRevision(repositoryName),
                    S = repositoryService.GetStagedRevision(repositoryName),
                    I = "0000000000000000000000000000000000000000"
                })).ToList<ClientRepositoryDataDTO>();
                bool isRunning;
                repositoryDataDto1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service1.AreJobsRunning(out isRunning));
                if (isRunning)
                {
                    IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
                    List<ITransferJob> jobs;
                    repositoryDataDto1.AddRange((IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)service1.GetJobs(out jobs, true));
                    foreach (ITransferJob transferJob in jobs)
                    {
                        ChangeSet changeSet = service2.Get<ChangeSet>(transferJob.ID);
                        ClientRepositoryDataDTO repositoryDataDto2 = currentClientRepositoryDataDTO.FirstOrDefault<ClientRepositoryDataDTO>((Func<ClientRepositoryDataDTO, bool>)(crd => crd.R == changeSet.Name));
                        if (repositoryDataDto2 != null)
                            repositoryDataDto2.I = changeSet.Head;
                    }
                }
                currentClientRepositoryDataDTO.Sort();
            }
            catch (Exception ex)
            {
                repositoryDataDto1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("RPS010", "There was an error in gathering the clientRepositoryInfo", ex));
            }
            return (IEnumerable<Redbox.UpdateManager.ComponentModel.Error>)repositoryDataDto1;
        }
    }
}
