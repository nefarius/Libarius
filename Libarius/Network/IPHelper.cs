using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Libarius.Network
{
    public static class IpHelper
    {
        /// <summary>
        ///     Returns the private IP address from the first adapter found on the client machine.
        /// </summary>
        public static IPAddress PrivateIpAddress
        {
            get
            {
                return
                    Dns.GetHostEntry(Environment.MachineName)
                        .AddressList.Where(i => i.AddressFamily == AddressFamily.InterNetwork)
                        .FirstOrDefault();
            }
        }

        /// <summary>
        ///     Queries checkip.dyndns.org to retrieve the public visible IP address.
        /// </summary>
        public static IPAddress PublicIpAddress
        {
            get
            {
                string content;
                var request = (HttpWebRequest) WebRequest.Create("http://checkip.dyndns.org/");
                var ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");

                request.UserAgent = "curl"; // this simulate curl Linux command
                request.Method = "GET";

                using (var response = request.GetResponse())
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
        ///     Returns the default gateway.
        /// </summary>
        public static IEnumerable<IPAddress> DefaultGatewayAdresses
        {
            get
            {
                var result = from i in NetworkInterface.GetAllNetworkInterfaces()
                    let ifprops = i.GetIPProperties()
                    from gw in ifprops.GatewayAddresses
                    where gw.Address.AddressFamily.Equals(AddressFamily.InterNetwork)
                    select gw.Address;

                return result;
            }
        }

        /// <summary>
        ///     Returns the active connections dhcp server addresses.
        /// </summary>
        public static IEnumerable<IPAddress> DhcpServerAddresses
        {
            get
            {
                var result = from i in NetworkInterface.GetAllNetworkInterfaces()
                    let ifprops = i.GetIPProperties()
                    from dhcp in ifprops.DhcpServerAddresses
                    where dhcp.AddressFamily.Equals(AddressFamily.InterNetwork)
                    select dhcp;

                return result;
            }
        }

        /// <summary>
        ///     Returns the active connections dns server addresses.
        /// </summary>
        public static IEnumerable<IPAddress> DnsServerAddresses
        {
            get
            {
                var result = from i in NetworkInterface.GetAllNetworkInterfaces()
                    where i.NetworkInterfaceType.Equals(NetworkInterfaceType.Ethernet)
                    let ifprops = i.GetIPProperties()
                    from dns in ifprops.DnsAddresses
                    where dns.AddressFamily.Equals(AddressFamily.InterNetwork)
                    select dns;

                return result;
            }
        }
    }
}