namespace TaskScheduler
{
    internal enum TaskStatus
    {
        Ready = 267008, // 0x00041300
        Running = 267009, // 0x00041301
        Disabled = 267010, // 0x00041302
        NeverRun = 267011, // 0x00041303
        NoMoreRuns = 267012, // 0x00041304
        NotScheduled = 267013, // 0x00041305
        Terminated = 267014, // 0x00041306
        NoTriggers = 267015, // 0x00041307
        NoTriggerTime = 267016, // 0x00041308
    }
}
