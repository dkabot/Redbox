namespace UpdateClientService.API.Services.FileSets
{
    public class StateFile
    {
        private FileSetState _inProgressFileSetState;

        public StateFile(
            long fileSetId,
            long activeRevisionId,
            long inProgressRevisionId,
            FileSetState inProgressFileSetState)
        {
            FileSetId = fileSetId;
            ActiveRevisionId = activeRevisionId;
            InProgressRevisionId = inProgressRevisionId;
            _inProgressFileSetState = inProgressFileSetState;
        }

        public long FileSetId { get; }

        public long ActiveRevisionId { get; private set; }

        public long InProgressRevisionId { get; set; }

        public long CurrentRevisionId => !IsRevisionDownloadInProgress ? ActiveRevisionId : InProgressRevisionId;

        public FileSetState InProgressFileSetState
        {
            get => _inProgressFileSetState;
            set
            {
                _inProgressFileSetState = value;
                if (_inProgressFileSetState != FileSetState.Active)
                    return;
                ActiveRevisionId = InProgressRevisionId;
                InProgressRevisionId = 0L;
            }
        }

        public FileSetState CurrentFileSetState =>
            !IsRevisionDownloadInProgress ? FileSetState.Active : _inProgressFileSetState;

        public bool IsRevisionDownloadInProgress => InProgressRevisionId > 0L;

        public bool HasActiveRevision => ActiveRevisionId > 0L;
    }
}