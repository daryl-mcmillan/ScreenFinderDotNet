using ScreenFinderDotNet.Dial;
using ScreenFinderDotNet.Upnp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet
{
    class Program
    {
        static void Main(string[] args)
        {

            int port = 34567;

            IPAddress addr = GetAddresses().FirstOrDefault();

            string descriptorLocation = string.Format("http://{0}:{1}/dd.xml", addr.ToString(), port);

            if (addr == null)
            {
                return;
            }
            using( DialService dialService = DialService.Start(port) ) {
                DialUpnpHandler upnpHandler = new DialUpnpHandler(descriptorLocation);
                using (UpnpService service = UpnpService.Start(upnpHandler))
                {
                    Console.ReadLine();
                }
            }

        }

        static IEnumerable<IPAddress> GetAddresses()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var nic in interfaces)
            {
                if (nic.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                var ip4addrs = nic.GetIPProperties()
                    .UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                foreach (var ip4addr in ip4addrs)
                {
                    yield return ip4addr.Address;
                }
            }

        }

    }
}
