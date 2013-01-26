using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Dial
{
    class DialService : IDisposable
    {

        private readonly int m_port;
        private readonly HttpListener m_listener;
        private bool m_disposed = false;

        private DialService(int port, HttpListener listener)
        {
            m_port = port;
            m_listener = listener;
        }

        private void Accept()
        {
            m_listener.BeginGetContext(HandleContext, null);
        }

        public void HandleContext(IAsyncResult result)
        {
            try
            {
                m_listener.EndGetContext(result);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            Accept();
        }

        public void Dispose()
        {
            if (m_disposed)
            {
                return;
            }
            IDisposable d = m_listener;
            d.Dispose();
            m_disposed = true;
        }

        public static DialService Start()
        {

            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://*:34567/");
            listener.Start();
            DialService service = new DialService(34567, listener);
            service.Accept();
            return service;
        }


    }
}
