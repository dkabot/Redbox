using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.UpdateManager.Environment
{
    internal class HealthService : IHealthService
    {
        private Dictionary<string, HealthService.HealthItem> _healthItemList = new Dictionary<string, HealthService.HealthItem>();
        private Timer _healthTimer;
        private bool _isRunning;
        private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private int _lockTimeout = 3000;

        public static HealthService Instance => Singleton<HealthService>.Instance;

        public ErrorList Initialize()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                ServiceLocator.Instance.AddService(typeof(IHealthService), (object)this);
                this._healthTimer = new Timer((TimerCallback)(o => this.UpdateHealth()));
                this._isRunning = false;
                LogHelper.Instance.Log("Initialized the health service", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS999", "An unhandled exception occurred while initializing the health service.", ex));
            }
            return errorList;
        }

        public ErrorList Start()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._healthTimer.Change(new TimeSpan(0L), new TimeSpan(0, 0, 60));
                this._isRunning = true;
                LogHelper.Instance.Log("Starting the health service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS998", "An unhandled exception occurred while starting the health service.", ex));
            }
            return errorList;
        }

        public ErrorList Stop()
        {
            ErrorList errorList = new ErrorList();
            try
            {
                this._healthTimer.Change(-1, -1);
                this._isRunning = false;
                LogHelper.Instance.Log("Stopping the health service.", LogEntryType.Info);
            }
            catch (Exception ex)
            {
                errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS997", "An unhandled exception occurred while stopping the health service.", ex));
            }
            return errorList;
        }

        public ErrorList Add(string key, TimeSpan expireTime, Action rescue)
        {
            ErrorList errorList1 = new ErrorList();
            try
            {
                if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                {
                    errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Add", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                    return errorList1;
                }
                try
                {
                    if (this._healthItemList.ContainsKey(key))
                    {
                        ErrorList errorList2 = new ErrorList();
                        errorList2.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS8002", "An unhandled exception occurred in health service", string.Format("Exception occurred when trying to add a health item with a duplicate key {0}", (object)key)));
                        return errorList2;
                    }
                    this._healthItemList.Add(key, new HealthService.HealthItem(key, expireTime, rescue));
                    return new ErrorList();
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS996", "An unhandled exception occurred while adding a key to the health service.", ex));
            }
            return errorList1;
        }

        public ErrorList Remove(string key)
        {
            ErrorList errorList1 = new ErrorList();
            try
            {
                if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                {
                    errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Remove", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                    return errorList1;
                }
                try
                {
                    if (this._healthItemList.TryGetValue(key, out HealthService.HealthItem _))
                    {
                        this._healthItemList.Remove(key);
                        LogHelper.Instance.Log("Health service: Removing {0}", (object)key);
                        return new ErrorList();
                    }
                    ErrorList errorList2 = new ErrorList();
                    errorList2.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS8001", "An unhandled exception occurred in health service", string.Format("Exception occurred when trying to remove a health item with a non-existent key {0}", (object)key)));
                    return errorList2;
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS995", "An unhandled exception occurred while removing a key to the health service.", ex));
            }
            return errorList1;
        }

        public ErrorList Update(string key)
        {
            ErrorList errorList1 = new ErrorList();
            try
            {
                if (!this._lock.TryEnterWriteLock(this._lockTimeout))
                {
                    errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.Update", "TryEnterWriteLock timeout expired prior to acquiring the lock."));
                    return errorList1;
                }
                try
                {
                    HealthService.HealthItem healthItem;
                    if (this._healthItemList.TryGetValue(key, out healthItem))
                    {
                        healthItem.UpdateHealth();
                        return new ErrorList();
                    }
                    ErrorList errorList2 = new ErrorList();
                    errorList2.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS8000", "An unhandled exception occurred in health service", string.Format("Exception occurred when trying to update a health item with a non-existent key {0}", (object)key)));
                    return errorList2;
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                errorList1.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("HS994", "An unhandled exception occurred while updating a key to the health service.", ex));
            }
            return errorList1;
        }

        private HealthService()
        {
        }

        private void UpdateHealth()
        {
            if (!this._lock.TryEnterWriteLock(this._lockTimeout))
            {
                Redbox.UpdateManager.ComponentModel.Error.NewError("HealthService.UpdateHealth", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
            }
            else
            {
                try
                {
                    if (!this._isRunning)
                        return;
                    this._healthItemList.ForEach<KeyValuePair<string, HealthService.HealthItem>>((Action<KeyValuePair<string, HealthService.HealthItem>>)(key =>
                    {
                        try
                        {
                            key.Value.CheckHealth();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Instance.Log("HS996 An unhandled exception was raised checking the health for {0}", ex, (object)key.Key);
                        }
                    }));
                }
                finally
                {
                    this._lock.ExitWriteLock();
                }
            }
        }

        private class HealthItem
        {
            private int InCheckup;

            public HealthItem(string key, TimeSpan expireTime, Action rescue)
            {
                this.LastCheckup = DateTime.Now;
                this.ExpireTime = expireTime;
                this.Rescue = rescue;
                this.Key = key;
            }

            internal void CheckHealth() => this.Checkup();

            internal void UpdateHealth()
            {
                this.LastCheckup = DateTime.Now;
                LogHelper.Instance.Log("Health service: Updating {0}", (object)this.Key);
            }

            private bool IsExpired() => this.LastCheckup.Add(this.ExpireTime) < DateTime.Now;

            private void Checkup()
            {
                try
                {
                    if (Interlocked.CompareExchange(ref this.InCheckup, 1, 0) == 1)
                    {
                        LogHelper.Instance.Log(string.Format("HealthItem Checkup: {0} already executing.", (object)this.Key));
                    }
                    else
                    {
                        try
                        {
                            if (!this.IsExpired())
                                return;
                            this.Rescue();
                            this.UpdateHealth();
                        }
                        finally
                        {
                            this.InCheckup = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Instance.Log("There was an exception in HealthService.Checkup", ex);
                }
            }

            private string Key { get; set; }

            private DateTime LastCheckup { get; set; }

            private TimeSpan ExpireTime { get; set; }

            private Action Rescue { get; set; }
        }
    }
}
