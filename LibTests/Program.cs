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
            Console.WriteLine(SystemHelper.OsFriendlyName);

            Console.WriteLine("SiteDesc: {0}", AdHelper.MachineSiteDescription);

            Console.ReadKey();
        }
    }
}
