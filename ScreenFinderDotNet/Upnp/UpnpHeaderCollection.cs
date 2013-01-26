using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenFinderDotNet.Upnp
{
    internal class UpnpHeaderCollection : IEnumerable<UpnpHeader>
    {

        private readonly IEnumerable<UpnpHeader> m_items;

        public UpnpHeaderCollection(
                IEnumerable<UpnpHeader> items
            )
        {
            m_items = items;
        }

        public IEnumerable<UpnpHeader> this[string name]
        {
            get{
                IEnumerable<UpnpHeader> matches = m_items
                    .Where(
                        item => String.Equals(
                            name,
                            item.Name,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    );
                return matches;
            }
        }

        IEnumerator<UpnpHeader> IEnumerable<UpnpHeader>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            IEnumerable<UpnpHeader> me = this;
            return me.GetEnumerator();
        }
    }
}
