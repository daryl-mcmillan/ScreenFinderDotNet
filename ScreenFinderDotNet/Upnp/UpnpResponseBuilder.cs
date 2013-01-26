using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal class UpnpResponseBuilder
    {

        private readonly string m_httpVersion;
        private int m_statusCode = 200;
        private string m_statusMessage = "OK";
        private readonly List<UpnpHeader> m_headers = new List<UpnpHeader>();

        public UpnpResponseBuilder(string httpVersion)
        {
            m_httpVersion = httpVersion;
        }

        public void SetStatus(int code, string message)
        {
            m_statusCode = code;
            m_statusMessage = message;
        }

        public void AddHeader(string name, string value)
        {
            m_headers.Add(new UpnpHeader(name, value));
        }

        public string FormatResponse()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("HTTP/{0} {1} {2}\r\n", m_httpVersion, m_statusCode, m_statusMessage);
            foreach (UpnpHeader header in m_headers)
            {
                builder.AppendFormat("{0}: {1}\r\n", header.Name, header.Value);
            }
            builder.Append("\r\n");
            return builder.ToString();
        }

    }
}
