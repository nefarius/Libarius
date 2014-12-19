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
            Console.WriteLine(IpHelper.DefaultGateway);

            Console.ReadKey();
        }
    }
}
