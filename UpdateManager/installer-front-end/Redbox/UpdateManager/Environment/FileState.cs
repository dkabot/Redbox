namespace Redbox.UpdateManager.Environment
{
    internal enum FileState
    {
        Unknown,
        Staged,
        Activated,
        ActivationFailedOnce,
        ActivationFailedTwice,
        NoDestinationDirectory,
        NoDestinationPath,
        ActivationFailedNoRetry,
    }
}
