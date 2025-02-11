using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal abstract class AbstractBarcodeReader : IBarcodeReader, IDisposable
    {
        private bool Disposed;

        protected AbstractBarcodeReader(BarcodeServices barcodeServices_0)
        {
            Service = barcodeServices_0;
        }

        public IScanResult Scan(string file)
        {
            var result = new ScanResult(file);
            OnScan(result);
            return result;
        }

        public IScanResult Scan(ISnapResult isnapResult_0)
        {
            var result = new ScanResult(isnapResult_0);
            OnScan(result);
            return result;
        }

        public bool IsLicensed { get; protected set; }

        public BarcodeServices Service { get; }

        public void Dispose()
        {
            if (Disposed)
                return;
            OnDispose(true);
        }

        protected abstract void OnScan(ScanResult result);

        protected virtual void OnDispose(bool disposing)
        {
        }
    }
}