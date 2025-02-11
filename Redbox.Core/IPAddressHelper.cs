using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Redbox.Core
{
    public static class IPAddressHelper
    {
        private const string MicrosoftVideoDevice = "Microsoft TV/Video Connection";

        public static IPAddress GetAddressForHostName(string hostName)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return null;
            var hostAddresses = Dns.GetHostAddresses(hostName);
            if (hostAddresses.Length == 0)
                return null;
            if (string.Compare(hostName, "localhost", true) == 0)
                return hostAddresses[0];
            var addressForHostName = (IPAddress)null;
            var bindableAddress = GetBindableAddress();
            foreach (var ipAddress in hostAddresses)
                if (!(ipAddress.ToString() != bindableAddress.ToString()))
                {
                    addressForHostName = ipAddress;
                    break;
                }

            return addressForHostName;
        }

        private static IPAddress GetBindableAddress()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
                if ((!(networkInterface.Description == "Microsoft TV/Video Connection") ||
                     networkInterface.GetPhysicalAddress().GetAddressBytes() != new byte[6]) &&
                    networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    var ipProperties = networkInterface.GetIPProperties();
                    if (ipProperties != null)
                        foreach (var unicastAddress in ipProperties.UnicastAddresses)
                            if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork)
                                return unicastAddress.Address;
                }

            return null;
        }
    }
}