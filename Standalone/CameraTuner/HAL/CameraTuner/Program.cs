using System;
using System.Windows.Forms;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Services;
using Redbox.HAL.Component.Model.Threading;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.CameraTuner
{
    public class Program : IDisposable
    {
        private readonly Guid AppGuid = new Guid("96C0A029-9835-41B5-8F45-53E9AD339B6D");
        private readonly NamedLock InstanceLock;
        private bool Disposed;

        public Program()
        {
            InstanceLock = new NamedLock(AppGuid.ToString());
        }

        public void Dispose()
        {
            DisposeInner(true);
        }

        ~Program()
        {
            DisposeInner(false);
        }

        public void Run(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ServiceLocator.Instance.AddService(typeof(IRuntimeService), new RuntimeService());
            ServiceLocator.Instance.AddService(typeof(IEncryptionService), new TripleDesEncryptionService());
            var tunerLog = new TunerLog();
            ServiceLocator.Instance.AddService(typeof(ILogger), tunerLog);
            BarcodeConfiguration.MakeNewInstance2();
            var instance = new BarcodeReaderFactory();
            ServiceLocator.Instance.AddService(typeof(IBarcodeReaderFactory), instance);
            instance.Initialize(new ErrorList());
            Application.Run(new MainForm(tunerLog));
        }

        [STAThread]
        public static void Main(string[] args)
        {
            using (var program = new Program())
            {
                if (!program.InstanceLock.IsOwned)
                    Environment.Exit(10);
                program.Run(args);
            }
        }

        private void DisposeInner(bool fromDispose)
        {
            if (Disposed)
                return;
            Disposed = true;
            if (!fromDispose)
                return;
            InstanceLock.Dispose();
        }
    }
}