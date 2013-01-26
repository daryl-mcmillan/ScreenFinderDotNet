using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal class UpnpRequest
    {

        private static readonly Regex m_requestLine = new Regex(
            @"^(?<verb>[a-z0-9-]+) (?<uri>[^ \r\n\t]+) http/(?<version>1.[01])$",
            RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase
        );

        private static readonly Regex m_headerPattern = new Regex(
            @"^(?<name>[a-z0-9-]+):[ \t]+(?<value>.*)$",
            RegexOptions.Compiled
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnoreCase
        );

        private readonly Action<string> m_replyFunc;
        private readonly IPEndPoint m_clientEndPoint;
        private readonly string m_verb;
        private readonly string m_uri;
        private readonly string m_httpVersion;
        private UpnpHeaderCollection m_headers;

        public UpnpRequest(
                Action<string> replyFunc,
                IPEndPoint clientEndPoint,
                string verb,
                string uri,
                string httpVersion,
                IEnumerable<UpnpHeader> headers
            )
        {
            m_replyFunc = replyFunc;
            m_clientEndPoint = clientEndPoint;
            m_verb = verb;
            m_uri = uri;
            m_httpVersion = httpVersion;
            m_headers = new UpnpHeaderCollection( headers );
        }

        public IPEndPoint ClientEndPoint
        {
            get { return m_clientEndPoint; }
        }

        public string Verb
        {
            get { return m_verb; }
        }

        public string Uri
        {
            get { return m_uri; }
        }

        public string HttpVersion
        {
            get { return m_httpVersion; }
        }

        public UpnpHeaderCollection Headers
        {
            get { return m_headers; }
        }

        public void Reply(Action<UpnpResponseBuilder> writer)
        {
            UpnpResponseBuilder builder = new UpnpResponseBuilder( m_httpVersion );
            writer(builder);
            string response = builder.FormatResponse();
            m_replyFunc(response);
        }

        public static bool TryParse(
                Action<string> replyFunc,
                IPEndPoint clientEndPoint,
                string message,
                out UpnpRequest request
            )
        {
            request = null;
            string[] lines = message.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            if (lines.Length == 0)
            {
                return false;
            }
            Match requestLine = m_requestLine.Match(lines[0]);
            if (!requestLine.Success)
            {
                return false;
            }

            string verb = requestLine.Groups["verb"].Value.ToUpperInvariant();
            string uri = requestLine.Groups["uri"].Value;
            string httpVersion = requestLine.Groups["version"].Value;

            UpnpHeader[] headers = lines
                .Skip(1)
                .Select(ParseHeaderOrNull)
                .Where(h => h != null).ToArray();

            request = new UpnpRequest(
                replyFunc,
                clientEndPoint,
                verb,
                uri,
                httpVersion,
                headers
            );
            return true;

        }

        private static UpnpHeader ParseHeaderOrNull(string rawHeader)
        {
            if (String.IsNullOrEmpty(rawHeader))
            {
                return null;
            }
            Match m = m_headerPattern.Match(rawHeader);
            if (!m.Success)
            {
                return null;
            }
            string name = m.Groups["name"].Value;
            string value = m.Groups["value"].Value;
            UpnpHeader header = new UpnpHeader(name, value);
            return header;
        }

    }
}
