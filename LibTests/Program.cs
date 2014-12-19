using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Libarius.Network;

namespace LibTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Private: {0}\nGateway: {1}\nPublic:  {2}", 
                IpHelper.PrivateIpAddress, IpHelper.DefaultGateway, IpHelper.PublicIpAddress);

            Console.ReadKey();
        }
    }
}
