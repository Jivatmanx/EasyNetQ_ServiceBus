using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using System.Collections.Concurrent;
using Avitia.Library;
using Avitia.Controller.Messages;
using Avitia.Controller.Network;
namespace Avitia.Controller.Services
{
    /// <summary>
    /// NodeService Base class for State Machine processes for communication to 
    /// and management from the Controller.
    /// </summary>
    /// <remarks>
    /// The node communications protocols are shared with the Device Interfaces
    /// </remarks>
    public class NodeServiceStateMachine : NodeService
    {
        /// <summary>
        /// Default Service constructor 
        /// </summary>
        public NodeServiceStateMachine()
            : base()
        {        }
        
        /// <summary>
        /// Override the Service Handler for this service definition
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <returns></returns>
        public override async Task Service()
        {

        }

    }
}
