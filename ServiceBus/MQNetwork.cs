using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ.Topology;
using EasyNetQ;
using EasyNetQ.InMemoryClient;
namespace Avitia.Controller
{
    /// <summary>
    /// Static Configuration routines for establishing routing connections between system components.
    /// Refer to the System flow to identify and configure the topic routng schemes.  
    /// 
    ///     [MachineInstance].[FromComponent].[ToComponent]
    /// 
    /// To extend routing schema beyond topic routing requires enhancing the ServiceBus model.
    /// </summary>
    /// <remarks>
    /// This configuration is specific to this implementation only.   Configuration can be extended
    /// to be read from an XML configuration to dynamically configure.  
    /// </remarks>
    public class MQNetwork
    {
        #region Variables
        /// <summary>
        /// Topic from any source
        /// </summary>
        public static readonly String Wildcard = ".*";
        /// <summary>
        /// Global inMemory RabbitMQ client.
        /// 
        /// Inmemory provides very fast access and response as no disk access is required.
        /// Alternatively no persistence is provided - everything needs to be built everytime.
        /// </summary>
        private static IAdvancedBus inMemoryBus;
        /// <summary>
        /// Control multi-threaded access to global resources 
        /// </summary>
        private static readonly object queueLock = new object();
        /// <summary>
        /// 
        /// </summary>
        private static readonly object channelLock = new object();
        #endregion

        /// <summary>
        /// Configure specific routes for each component.  This method must be updated for each routing configuration change.
        /// The publish nodes are for testing the simulation and documenting the expected interconnections.
        /// The subscribe nodes configure expected topics for this node.
        /// 
        /// Each subscriptions is a separate binding to the component's internal queue.
        /// </summary>
        /// <remarks>
        ///                         [To]          [From]
        /// 
        /// Controller Publish:     [Memory]     .[Controller]
        /// Memory Publish:         [Controller] .[Memory]
        ///                                                   
        /// Controller Subscribe:   [Controller] .[*]
        /// Memory Subscribe:       [Memory]     .[*]
        /// </remarks>
        /// <param name="service">host service to facilitate access to all properties</param>
        /// <returns></returns>
        public static NodeInterface BuildNode(NodeServiceBase service)
        {
            NodeInterface node = new NodeInterface(service);
            switch (service.thisComponent)
            {
                case SystemNode.Controller:
                    node.Publish.Add(node.RouteKey(service.thisComponent, SystemNode.Environment));
                    node.Publish.Add(node.RouteKey(service.thisComponent,SystemNode.Memory));
                    node.Subscribe.Add(node.RouteKey(SystemNode.Environment, service.thisComponent));
                    node.Subscribe.Add(node.RouteKey(SystemNode.Memory, service.thisComponent));
                    break;
                case SystemNode.Environment:
                    node.Publish.Add(node.RouteKey(service.thisComponent,SystemNode.Controller));
                    node.Subscribe.Add(node.RouteKey(SystemNode.Controller,service.thisComponent));
                    break;
                case SystemNode.Memory:
                    node.Publish.Add(node.RouteKey(service.thisComponent,SystemNode.Controller));
                    node.Subscribe.Add(node.RouteKey(SystemNode.Controller,service.thisComponent));
                    break;
                default:
                    break;
            }
            node.QueueConfiguration(service);       //  define the queue subscription topics
            return (node);
        }
        
        /// <summary>
        /// Thread safe channel allocation
        /// </summary>
        /// <returns></returns>
        public static IAdvancedPublishChannel mqChannel()
        {
            lock(channelLock)
            {
                return(inMemoryBus.OpenPublishChannel());
            }
        }
        /// <summary>
        /// Create Queue against MQRabbit Memory Client for Multi-threaded access
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="msghandler"></param>
        public static void mqQueue(IQueue queue, Func<IMessage<NodeMessage>, MessageReceivedInfo, Task> msghandler)
        {
            lock (queueLock)
            {
                inMemoryBus.Subscribe<NodeMessage>(queue, msghandler); //  (5) Instantiate an async queue consumer
            }
        }
        
        /// <summary>
        /// MQNetwork Constructor
        /// iterate through the SystemComponent collection. 
        /// </summary>
        /// <remarks>
        /// Perfromance Note.
        /// Modified InMemoryRabbitHutch to remove default console logger.  The default console added significant
        /// ET overhead and variance to messaging.  
        /// From 1-7ms to 0.01-0.05ms
        /// </remarks>
        /// <returns>Complete servicebus routing configuration</returns>
        public static void StartServiceBus()
        {
            try
            {
                inMemoryBus = InMemoryRabbitHutch.CreateBus().Advanced;     //  no connection required for memory bus
                while (!inMemoryBus.IsConnected) Thread.Sleep(10);          //  make sure it's available
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n\tServiceBus Failure[{0}]\n", ex.Message);
            }
        }
        /// <summary>
        /// /
        /// </summary>
        public static void StopServiceBus()
        {
            try 
	        {
                inMemoryBus.Dispose();
            }
            catch (Exception)
            {
            }
        }
    }
}
