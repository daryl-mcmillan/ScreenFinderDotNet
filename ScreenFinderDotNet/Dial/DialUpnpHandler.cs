using ScreenFinderDotNet.Upnp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Dial
{
    class DialUpnpHandler : IUpnpHandler
    {

        private readonly string m_descriptorLocation;

        public DialUpnpHandler(string descriptorLocation)
        {
            m_descriptorLocation = descriptorLocation;
        }

        public void ProcessRequest(UpnpRequest request)
        {

            if (request.Verb != "M-SEARCH")
            {
                return;
            }

            if (request.Uri != "*")
            {
                return;
            }

            bool multiscreenRequest = request
                .Headers["ST"]
                .Any(h => h.Value == "urn:dial-multiscreen-org:service:dial:1");
            if (!multiscreenRequest)
            {
                return;
            }

            if (!multiscreenRequest)
            {
                return;
            }

            Console.WriteLine("multiscreen discovery request");

            request.Reply(
                builder =>
                {
                    builder.SetStatus(200, "OK");
                    builder.AddHeader("LOCATION", m_descriptorLocation );
                    builder.AddHeader("CACHE-CONTROL", "max-age=0");
                    builder.AddHeader("ST", "urn:dial-multiscreen-org:service:dial:1");
                }
            );

        }
    }
}
