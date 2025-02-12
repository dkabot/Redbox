using System;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Responses;

namespace DeviceService.Client.Jobs
{
    public abstract class BaseReadCardJob : IReadCardJob
    {
        protected readonly Guid _readCardCommandRequestId = Guid.NewGuid();
        protected bool _cancelled;
        protected IDeviceServiceClient _deviceServiceClient;
        protected bool _inProcess;
        protected Action<BaseResponseEvent> _jobCompletedCallback;
        protected bool _readCompleted;
        protected Action<BaseResponseEvent> _readJobEvents;
        protected int? _timeout;

        public BaseReadCardJob(
            IDeviceServiceClient deviceServiceClient,
            Action<BaseResponseEvent> jobCompleteCallback,
            Action<BaseResponseEvent> readJobEvents,
            int? timeout)
        {
            var baseReadCardJob = this;
            _deviceServiceClient = deviceServiceClient;
            _jobCompletedCallback = baseResponseEvent =>
            {
                baseReadCardJob._readCompleted = true;
                jobCompleteCallback(baseResponseEvent);
            };
            _readJobEvents = readJobEvents;
            _timeout = timeout;
        }

        public bool Execute()
        {
            var flag = false;
            if (!_cancelled && !_inProcess)
            {
                _inProcess = true;
                PerformCardRead();
                flag = true;
            }

            return flag;
        }

        public Guid RequestId => _readCardCommandRequestId;

        public bool Cancel(Action<BaseResponseEvent> cancelCompleteCallback)
        {
            var flag = false;
            if (!_readCompleted && !_cancelled)
            {
                _cancelled = true;
                flag = true;
                _deviceServiceClient.CancelCommand(_readCardCommandRequestId, cancelCompleteCallback);
            }

            return flag;
        }

        protected abstract void PerformCardRead();
    }
}