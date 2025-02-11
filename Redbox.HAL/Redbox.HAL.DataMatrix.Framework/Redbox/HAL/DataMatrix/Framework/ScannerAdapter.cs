using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal abstract class ScannerAdapter : IDisposable
    {
        protected readonly IRuntimeService RuntimeService;
        private bool m_disposed;

        protected ScannerAdapter()
        {
            RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
        }

        public void Dispose()
        {
            if (m_disposed)
                return;
            m_disposed = true;
            DisposeInner();
        }

        protected string GenerateSnapFileName()
        {
            var uniqueFile = RuntimeService.GenerateUniqueFile("jpg");
            return BarcodeConfiguration.Instance.UseRuntimePath
                ? Path.Combine(RuntimeService.RuntimePath("Video"), uniqueFile)
                : Path.Combine(BarcodeConfiguration.Instance.WorkingPath, uniqueFile);
        }

        protected abstract void DisposeInner();
    }
}