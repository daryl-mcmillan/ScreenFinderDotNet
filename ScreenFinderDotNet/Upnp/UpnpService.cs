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
        private readonly IUpnpHandler[] m_handlers;
        private bool m_disposed = false;

        private UpnpService( UdpClient client, IUpnpHandler[] handlers )
        {
            m_client = client;
            m_handlers = handlers;
        }

        public static UpnpService Start( params IUpnpHandler[] handlers )
        {
            UdpClient udp = new UdpClient(UPNP_PORT);
            udp.JoinMulticastGroup(UPNP_ADDR);
            UpnpService service = new UpnpService(udp, handlers);
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

                Action<string> replyFunc = delegate(string message)
                {
                    byte[] responseData = Encoding.UTF8.GetBytes(message);
                    m_client.Send(responseData, responseData.Length, clientInfo);
                };

                UpnpRequest request;
                bool parsed = UpnpRequest.TryParse( replyFunc, clientInfo, requestString, out request);
                if (parsed)
                {
                    ProcessRequest(request);
                }
            }
            Accept();
        }

        private void ProcessRequest(UpnpRequest request)
        {
            foreach (IUpnpHandler handler in m_handlers)
            {
                handler.ProcessRequest(request);
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
