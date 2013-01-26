using ScreenFinderDotNet.Upnp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet
{
    class Program
    {
        static void Main(string[] args)
        {

            using (UpnpService service = UpnpService.Start())
            {
                Console.ReadLine();
            }

        }
    }
}
