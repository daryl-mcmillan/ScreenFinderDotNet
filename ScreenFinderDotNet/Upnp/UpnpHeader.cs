using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal class UpnpHeader
    {

        private readonly string m_name;
        private readonly string m_value;

        public UpnpHeader(string name, string value)
        {
            m_name = name;
            m_value = value;
        }

        public string Name
        {
            get { return m_name; }
        }

        public string Value
        {
            get { return m_value; }
        }

    }
}
