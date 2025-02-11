using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class ScanResult : IScanResult, IDisposable
    {
        private readonly List<DecodeResult> RawDecodeResults = new List<DecodeResult>();
        private readonly ISnapResult SnapResult;
        private bool DeleteOnDispose;
        private bool Disposed;

        internal ScanResult(ISnapResult isnapResult_0)
            : this(isnapResult_0.Path)
        {
            SnapResult = isnapResult_0;
            ImagePath = isnapResult_0.Path;
        }

        internal ScanResult(string imagePath)
        {
            ExecutionTime = new TimeSpan();
            ImagePath = imagePath;
        }

        internal ScanResult()
            : this(string.Empty)
        {
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            if (DeleteOnDispose && DecodeResults.Count > 0 && SnapResult != null)
                SnapResult.Dispose();
            RawDecodeResults.Clear();
        }

        public bool DeviceError { get; internal set; }

        public TimeSpan ExecutionTime { get; internal set; }

        public List<IDecodeResult> DecodeResults
        {
            get
            {
                var list_0 = new List<IDecodeResult>();
                RawDecodeResults.ForEach(each => list_0.Add(each));
                return list_0;
            }
        }

        public string ImagePath { get; }

        internal void ResetOnException()
        {
            ExecutionTime = new TimeSpan();
            RawDecodeResults.Clear();
        }

        internal void Add(string code)
        {
            if (code.Equals("redbox", StringComparison.CurrentCultureIgnoreCase))
                return;
            var decodeResult = RawDecodeResults.Find(each => each.Matrix == code);
            if (decodeResult == null)
                RawDecodeResults.Add(new DecodeResult(code));
            else
                decodeResult.IncrementCount();
        }
    }
}