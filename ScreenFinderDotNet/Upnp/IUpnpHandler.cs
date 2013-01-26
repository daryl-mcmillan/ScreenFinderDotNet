using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal interface IUpnpHandler
    {

        void ProcessRequest(UpnpRequest request);

    }
}
