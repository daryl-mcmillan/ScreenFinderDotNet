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

            if (requestText.StartsWith("GET /dd.xml", StringComparison.InvariantCultureIgnoreCase))
            {
                SendDD(client);
            }
            else if (requestText.StartsWith("GET /apps/YouTube", StringComparison.InvariantCultureIgnoreCase))
            {
                SendApp(client, "YouTube");
            }
            else
            {
                Send404(client);
            }
            client.Dispose();
        }

        private void SendDD(Socket socket)
        {
            string uuid = "78976003-c31c-44a8-b3b6-1d9ab8f0ff96";

            string dd = "<root xmlns='urn:schemas-upnp-org:device-1-0'>"
                + "<specVersion>"
                + "<major>1</major>"
                + "<minor>0</minor>"
                + "</specVersion>"
                + "<device>"
                + "<deviceType>urn:schemas-upnp-org:device:tvdevice:1</deviceType>"
                + "<friendlyName>my tv</friendlyName>"
                + "<manufacturer>nobody</manufacturer>"
                + "<modelName>tv1000</modelName>"
                + "<UDN>uuid:" + uuid + "</UDN>"
                + "</device>"
                + "</root>";

            string responseText =
                "HTTP/1.1 200 OK\r\n"
                + "Content-Type: application/xml\r\n"
                + "Connection: close\r\n"
                + "Application-URL: http://192.168.1.101:34567/apps\r\n"
                + "Content-Length: " + dd.Length + "\r\n"
                + "\r\n"
                + dd
                ;
            byte[] responseData = Encoding.UTF8.GetBytes(responseText);
            socket.Send(responseData);
        }

        private void SendApp(Socket socket, string appName)
        {
            string info = "<service xmlns='urn:dial-multiscreen-org:schemas:dial'>"
                + "<name>YouTube</name>"
                + "<options allowStop='false'/>"
                + "<state>running</state>"
                + "<link rel='run' href='run'/>"
                + "</service>";

            string responseText =
                "HTTP/1.1 200 OK\r\n"
                + "Content-Type: application/xml\r\n"
                + "Content-Length: " + info.Length.ToString() + "\r\n"
                + "\r\n"
                + info
                ;

            byte[] responseData = Encoding.UTF8.GetBytes(responseText);
            socket.Send(responseData);
        }

        private void Send404(Socket socket)
        {
            string responseText = "HTTP/1.1 404 Not Found\r\nConnection: close\r\n\r\n";
            byte[] responseData = Encoding.UTF8.GetBytes(responseText);
            socket.Send(responseData);
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
