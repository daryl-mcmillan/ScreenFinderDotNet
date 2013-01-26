using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal class UpnpService : IDisposable
    {

        private static readonly IPAddress UPNP_ADDR = new IPAddress(new byte[] { 239, 255, 255, 250 });
        private const int UPNP_PORT = 1900;
        private readonly UdpClient m_client;
        private bool m_disposed = false;

        private UpnpService( UdpClient client )
        {
            m_client = client;
        }

        public static UpnpService Start()
        {
            UdpClient udp = new UdpClient(UPNP_PORT);
            udp.JoinMulticastGroup(UPNP_ADDR);
            UpnpService service = new UpnpService(udp);
            service.Accept();
            return service;
        }

        private void Accept()
        {
            m_client.BeginReceive(ProcessIncomingData, null);
        }

        private void ProcessIncomingData( IAsyncResult result )
        {
            IPEndPoint clientInfo = null;
            byte[] requestData;
            try
            {
                requestData = m_client.EndReceive(result, ref clientInfo);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            string requestString = DecodeOrNull(requestData);
            if (!String.IsNullOrEmpty(requestString))
            {
                UpnpRequest request;
                bool parsed = UpnpRequest.TryParse(clientInfo, requestString, out request);
                if (parsed)
                {
                    ProcessRequest(request);
                }
            }
            Accept();
        }

        private void ProcessRequest(UpnpRequest request)
        {
            if (request.Verb == "M-SEARCH"
                && request.Uri == "*")
            {
                bool multiscreenRequest = request
                    .Headers["ST"]
                    .Any(h => h.Value == "urn:dial-multiscreen-org:service:dial:1");

                if (multiscreenRequest)
                {
                    Console.WriteLine("multiscreen");
                }
            }

        }

        private string DecodeOrNull(byte[] data)
        {
            try
            {
                string decoded = Encoding.UTF8.GetString(data);
                return decoded;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (m_disposed)
            {
                return;
            }
            IDisposable d = m_client;
            d.Dispose();
            m_disposed = true;
        }

    }
}
