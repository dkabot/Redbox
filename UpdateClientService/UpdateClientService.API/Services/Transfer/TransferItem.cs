namespace UpdateClientService.API.Services.Transfer
{
    public class TransferItem : ITransferItem
    {
        internal TransferItem(IBackgroundCopyFile file)
        {
            string pVal1;
            file.GetLocalName(out pVal1);
            string pVal2;
            file.GetRemoteName(out pVal2);
            BG_FILE_PROGRESS pVal3;
            file.GetProgress(out pVal3);
            Path = System.IO.Path.GetDirectoryName(pVal1);
            Name = System.IO.Path.GetFileName(pVal1);
            RemoteURL = pVal2;
            TotalBytes = pVal3.BytesTotal;
            TotalTransferd = pVal3.BytesTransferred;
        }

        public string Name { get; set; }

        public string Path { get; set; }

        public string RemoteURL { get; set; }

        public ulong TotalTransferd { get; set; }

        public ulong TotalBytes { get; set; }
    }
}