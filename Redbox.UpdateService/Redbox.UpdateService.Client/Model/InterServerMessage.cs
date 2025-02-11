namespace Redbox.UpdateService.Model
{
    public class InterServerMessage
    {
        public InterServerMessage(InterServerTask task, Server server)
        {
            Task = task;
            Server = server;
        }

        public InterServerMessage()
        {
        }

        public InterServerTask Task { get; set; }

        public Server Server { get; set; }
    }
}