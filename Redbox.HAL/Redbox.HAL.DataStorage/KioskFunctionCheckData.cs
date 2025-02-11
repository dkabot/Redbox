using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage
{
    internal sealed class KioskFunctionCheckData : IKioskFunctionCheckData
    {
        public string VerticalSlotTestResult { get; internal set; }

        public string InitTestResult { get; internal set; }

        public string VendDoorTestResult { get; internal set; }

        public string TrackTestResult { get; internal set; }

        public string SnapDecodeTestResult { get; internal set; }

        public string TouchscreenDriverTestResult { get; internal set; }

        public string CameraDriverTestResult { get; internal set; }

        public DateTime Timestamp { get; internal set; }

        public string UserIdentifier { get; internal set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Kiosk Check Data");
            stringBuilder.AppendLine(string.Format("  Vertical slot result = {0}", VerticalSlotTestResult));
            stringBuilder.AppendLine(string.Format("  Init result = {0}", InitTestResult));
            stringBuilder.AppendLine(string.Format("  Vend door result = {0}", VendDoorTestResult));
            stringBuilder.AppendLine(string.Format("  Track result = {0}", TrackTestResult));
            stringBuilder.AppendLine(string.Format("  Snap and decode result = {0}", SnapDecodeTestResult));
            stringBuilder.AppendLine(string.Format("  Camera driver result = {0}", CameraDriverTestResult));
            stringBuilder.AppendLine(string.Format("  Touchscreen driver result = {0}", TouchscreenDriverTestResult));
            stringBuilder.AppendLine(string.Format("  Timestamp = {0}", Timestamp.ToString()));
            stringBuilder.AppendLine(string.Format("  User token = {0}", UserIdentifier));
            return stringBuilder.ToString();
        }
    }
}