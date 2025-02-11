using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class SnapResult : ISnapResult, IDisposable
    {
        private bool Disposed;

        internal SnapResult(string path)
        {
            SnapOk = TestPath(path);
            Path = SnapOk ? path : string.Empty;
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            if (!TestPath(Path))
                return;
            try
            {
                File.Delete(Path);
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log("[SnapResult] unable to delete path '{0}'", Path);
                LogHelper.Instance.Log(ex.Message);
            }
        }

        public string Path { get; }

        public bool SnapOk { get; }

        private bool TestPath(string path)
        {
            return !string.IsNullOrEmpty(path) && File.Exists(path);
        }
    }
}