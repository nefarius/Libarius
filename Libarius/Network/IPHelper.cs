using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Libarius.Network
{
    public static class IPHelper
    {
        /// <summary>
        /// Returns the private IP address from the first adapter found on the client machine.
        /// </summary>
        public static IPAddress PrivateIpAddress
        {
            get
            {
                return Dns.GetHostEntry(Environment.MachineName).AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork).FirstOrDefault();
            }
        }

        /// <summary>
        /// Queries checkip.dyndns.org to retrieve the public visible IP address.
        /// </summary>
        public static IPAddress PublicIpAddress
        {
            get
            {
                string content;
                var request = (HttpWebRequest)WebRequest.Create("http://checkip.dyndns.org/");
                Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

                request.UserAgent = "curl"; // this simulate curl Linux command
                request.Method = "GET";

                using (WebResponse response = request.GetResponse())
                {
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }
                }

                return IPAddress.Parse(ip.Matches(content)[0].ToString());
            }
        }

        /// <summary>
        /// Returns the default gateway.
        /// </summary>
        public static IPAddress DefaultGateway
        {
            get
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                .Where(i => i.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                .FirstOrDefault().GetIPProperties().GatewayAddresses
                .FirstOrDefault().Address;
            }
        }
    }
}
