using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class NilScanResult : IScanResult, IDisposable
    {
        internal NilScanResult(string error)
        {
            HardwareErrorDescription = error;
        }

        public string HardwareErrorDescription { get; private set; }

        public void Dispose()
        {
        }

        public bool DeviceError => false;

        public List<IDecodeResult> DecodeResults { get; } = new List<IDecodeResult>();

        public TimeSpan ExecutionTime { get; }

        public string ImagePath => string.Empty;
    }
}