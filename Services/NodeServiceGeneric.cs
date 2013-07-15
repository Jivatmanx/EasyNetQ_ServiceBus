using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using EasyNetQ;

namespace Avitia.Controller
{

    /// <summary>
    /// Generic node service defined operations
    /// </summary>
    public class NodeServiceGeneric : NodeServiceBase
    {
        /// <summary>
        /// Default Service constructor 
        /// </summary>
        public NodeServiceGeneric(SystemNode _component)
            : base(_component)
        {
        }
        
        /// <summary>
        /// Override the Service Handler for this service definition.
        /// </summary>
        /// <remarks>
        /// The service is instantiated by the SystemController Startup() method
        /// which is responsible for configuring related components and initiated the 
        /// encapsulating thread for this service.
        /// </remarks>
        /// <returns></returns>
        public override void Service(object obj)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);

            while(true) // Wait for cancellation to be called
            {
                Thread.Sleep(rnd.Next((int)64, 512));
                this.SimulateMessageTraffic();
                if (this.m_cancellationController.IsCancellationRequested)
                {
                    break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void SimulateMessageTraffic()
        {
            foreach (var routekey in this.m_node.Publish)
            {
                Random rnd = new Random(DateTime.Now.Millisecond);
                Console.WriteLine("RK:[" + routekey + "] SRC:[" + this.thisComponent + "]");

                var message = new Message<NodeMessage>(new NodeMessage());
                message.Properties.AppId = routekey;            //  set the destination routekey


                this.m_node.QueueMessage(message);              //  queue the message
                if (this.m_node.m_cancellation.IsCancellationRequested)
                    return;
                this.m_node.ns.m_outgoingcount++;               //  count how many were sent
                Thread.Sleep(rnd.Next((int)512, 2048));        //  throttle to ensure consumer has time to receive
            }
        }

        /// <summary>
        /// receives incoming subscription topic messages for this service 
        /// </summary>
        /// <remarks>This handler needs to be enhanced with a list of handlers
        /// one for each distinct message managed by the coupled service.</remarks>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public override async Task ServiceMessageHandler(IMessage<NodeMessage> message, MessageReceivedInfo info)
        {
            this.m_node.ns.m_incomingETTotal += (int)(DateTime.Now.Ticks - message.Properties.Timestamp);      
        }
    }
}
