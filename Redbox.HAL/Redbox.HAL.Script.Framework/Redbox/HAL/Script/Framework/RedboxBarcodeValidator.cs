using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework
{
    internal sealed class RedboxBarcodeValidator : IBarcodeValidatorService
    {
        public bool IsValid(string barcode)
        {
            if (!ControllerConfiguration.Instance.EnableUnrecognizedBarcodeReject)
                return true;
            bool flag;
            try
            {
                int.Parse(barcode);
                flag = true;
            }
            catch
            {
                flag = false;
            }

            return flag && barcode.Length == 9;
        }
    }
}