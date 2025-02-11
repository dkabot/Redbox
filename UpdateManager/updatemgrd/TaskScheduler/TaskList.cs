using System;
using System.Collections;

namespace TaskScheduler
{
    internal class TaskList : IEnumerable, IDisposable
    {
        private ScheduledTasks st;
        private string nameComputer;

        internal TaskList() => this.st = new ScheduledTasks();

        internal TaskList(string computer) => this.st = new ScheduledTasks(computer);

        internal string TargetComputer
        {
            get => this.nameComputer;
            set
            {
                this.st.Dispose();
                this.st = new ScheduledTasks(value);
                this.nameComputer = value;
            }
        }

        public Task NewTask(string name) => this.st.CreateTask(name);

        public void Delete(string name) => this.st.DeleteTask(name);

        public Task this[string name] => this.st.OpenTask(name);

        public IEnumerator GetEnumerator() => (IEnumerator)new TaskList.Enumerator(this.st);

        public void Dispose() => this.st.Dispose();

        private class Enumerator : IEnumerator
        {
            private ScheduledTasks outer;
            private string[] nameTask;
            private int curIndex;
            private Task curTask;

            internal Enumerator(ScheduledTasks st)
            {
                this.outer = st;
                this.nameTask = st.GetTaskNames();
                this.Reset();
            }

            public bool MoveNext()
            {
                bool flag = ++this.curIndex < this.nameTask.Length;
                if (flag)
                    this.curTask = this.outer.OpenTask(this.nameTask[this.curIndex]);
                return flag;
            }

            public void Reset()
            {
                this.curIndex = -1;
                this.curTask = (Task)null;
            }

            public object Current => (object)this.curTask;
        }
    }
}
