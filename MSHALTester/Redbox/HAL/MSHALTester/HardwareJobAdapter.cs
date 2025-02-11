using System.ComponentModel;
using Redbox.HAL.Client;

namespace Redbox.HAL.MSHALTester;

internal sealed class HardwareJobAdapter : INotifyPropertyChanged
{
    internal HardwareJobAdapter(HardwareJob job)
    {
        Job = job;
    }

    public string ID => Job.ID;

    public string ProgramName => Job.ProgramName;

    public HardwareJobStatus Status => Job.Status;

    public HardwareJobPriority Priority => Job.Priority;

    internal HardwareJob Job { get; }

    internal bool Removable { get; set; }
    public event PropertyChangedEventHandler PropertyChanged;

    public void Merge(HardwareJob job)
    {
        if (!Job.Merge(job))
            return;
        NotifyPropertyChanged("Status");
        NotifyPropertyChanged("Priority");
    }

    private void NotifyPropertyChanged(string name)
    {
        var propertyChanged = PropertyChanged;
        if (propertyChanged == null)
            return;
        propertyChanged(this, new PropertyChangedEventArgs(name));
    }
}