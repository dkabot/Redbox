using System;
using System.Runtime.InteropServices;
using ClearImage;
using RBDM;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.DataMatrix.Framework
{
    internal sealed class ClearImageReaderV2 : AbstractBarcodeReader
    {
        private readonly clsRBDM TheReader;

        internal ClearImageReaderV2()
            : base(BarcodeServices.Inlite)
        {
            try
            {
                var int32 = Marshal.GetHINSTANCE(GetType().Module).ToInt32();
                var ciServer = (CiServer)new CiServerClass();
                ciServer.OpenExt(int32, 74391114, 0);
                TheReader = new clsRBDM
                {
                    Ci = ciServer,
                    Image = ciServer.CreateImage()
                };
                IsLicensed = true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Log(
                    "An unhandled exception was raised in BarcodeReader.InitializeClearImage. Calling CiServer.OpenExt to send license key failed.",
                    ex);
                TheReader = null;
                IsLicensed = false;
            }
        }

        protected override void OnScan(ScanResult result)
        {
            if (!IsLicensed)
            {
                LogHelper.Instance.Log("This version isn't licensed.");
                result.ResetOnException();
            }
            else
            {
                var maxBcIn = 6;
                for (var index = 0; index < 2; ++index)
                {
                    var flag = false;
                    try
                    {
                        TheReader.Image.Open(result.ImagePath);
                        using (var executionTimer = new ExecutionTimer())
                        {
                            var num = TheReader.Find(maxBcIn);
                            executionTimer.Stop();
                            result.ExecutionTime = executionTimer.Elapsed;
                            if (num > 0)
                                foreach (CiBarcode barcode in TheReader.Barcodes)
                                    result.Add(barcode.Text);
                        }
                    }
                    catch (Exception ex)
                    {
                        flag = true;
                        LogHelper.Instance.Log("[ClearImageReaderV2] Exception during scan.", ex);
                        result.ResetOnException();
                    }

                    try
                    {
                        TheReader.Image.Close();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Instance.Log("[ClearImageReaderV2] Exception closing barcode image.", ex);
                    }

                    if (!flag)
                        return;
                }

                result.ResetOnException();
            }
        }
    }
}