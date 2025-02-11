namespace TaskScheduler
{
    internal class Scheduler
    {
        private readonly TaskList tasks;

        public Scheduler() => this.tasks = new TaskList();

        public Scheduler(string computer)
        {
            this.tasks = new TaskList();
            this.TargetComputer = computer;
        }

        public string TargetComputer
        {
            get => this.tasks.TargetComputer;
            set => this.tasks.TargetComputer = value;
        }

        public TaskList Tasks => this.tasks;
    }
}
