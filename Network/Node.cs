using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Avitia.Controller
{

    /// <summary>
    /// Node Object managing network messaging.  
    /// 
    /// One instantiation is required by each node service operating on the router/controller dataflow network.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Reference to the Router/Controller cancellation token
        /// </summary>
        public CancellationToken m_ct;

        /// <summary>
        /// traffic statistics
        /// </summary>
        private NodeStatus ns = new NodeStatus();

        /// <summary>
        /// Constructor
        /// </summary>
        public Node(CancellationToken ct)
        {
            m_ct = ct;                              //  cancellation token
        }
    }
}


