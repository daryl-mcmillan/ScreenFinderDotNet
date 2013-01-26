using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Dial
{
    class DialService : IDisposable
    {

        private readonly TcpListener m_listener;
        private bool m_disposed = false;

        private DialService( TcpListener listener)
        {
            m_listener = listener;
        }

        private void Accept()
        {
            m_listener.BeginAcceptSocket(HandleClient, null);
        }

        private void HandleClient(IAsyncResult result)
        {
            Socket client;
            try
            {
                client = m_listener.EndAcceptSocket(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            ReadRequest(client);
            Accept();
        }

        private void ReadRequest(Socket client)
        {
            byte[] buffer = new byte[65536];
            int offset = 0;
            int received = client.Receive( buffer,offset, buffer.Length-offset, SocketFlags.None);
            if( received == 0 ){
                client.Dispose();
                return;
            }
            string requestText = Encoding.UTF8.GetString(buffer, 0, received);
            Console.WriteLine(requestText);
            string responseText = "HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n";
            byte[] responseData = Encoding.UTF8.GetBytes( responseText);
            client.Send( responseData);
            client.Dispose();
        }

        

        private void HandleRequest(HttpListenerContext context)
        {
            Console.WriteLine("{0} {1}", context.Request.HttpMethod, context.Request.Url);
            switch (context.Request.Url.AbsolutePath)
            {
                case "/dd.xml":

                case "/apps":
                case "/apps/":

                default:
                    Send404(context);
                    return;
            }
        }

        private void Send404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.StatusDescription = "Not Found";
            context.Response.Close();
        }

        public void Dispose()
        {
            if (m_disposed)
            {
                return;
            }
            m_listener.Stop();
            m_disposed = true;
        }

        public static DialService Start( int port )
        {

            TcpListener listener = new TcpListener( IPAddress.Any, port);
            listener.Start();
            DialService service = new DialService(listener);
            service.Accept();
            return service;
        }


    }
}
