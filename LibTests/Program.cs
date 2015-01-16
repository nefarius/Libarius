using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libarius.Active_Directory;
using Libarius.Network;
using Libarius.System;

namespace LibTests
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var gw in IpHelper.DefaultGatewayAdresses)
            {
                Console.WriteLine(gw);
            }

            foreach (var gw in IpHelper.DhcpServerAddresses)
            {
                Console.WriteLine(gw);
            }

            foreach (var gw in IpHelper.DnsServerAddresses)
            {
                Console.WriteLine(gw);
            }

            Console.WriteLine(AdHelper.MachineDomain);

            Console.ReadKey();
        }
    }
}
