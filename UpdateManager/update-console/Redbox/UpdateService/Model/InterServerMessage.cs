namespace Redbox.UpdateService.Model
{
    internal class InterServerMessage
    {
        public InterServerMessage(InterServerTask task, Server server)
        {
            this.Task = task;
            this.Server = server;
        }

        public InterServerMessage()
        {
        }

        public InterServerTask Task { get; set; }

        public Server Server { get; set; }
    }
}
