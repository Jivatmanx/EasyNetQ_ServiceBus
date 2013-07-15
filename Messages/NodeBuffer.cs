using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;

namespace Avitia.Controller.Messages
{
    public class NodeBuffer
    {
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<Object> m_incoming = new ConcurrentQueue<object>();
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<Object> m_outgoing = new ConcurrentQueue<object>();

    }
}
