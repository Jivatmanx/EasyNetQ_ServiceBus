#region .NET
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;

#endregion
#region EasyQ
using EasyNetQ;
using EasyNetQ.Loggers;
using EasyNetQ.ConnectionString;
using EasyNetQ.Topology;
using EasyNetQ.InMemoryClient;
using EasyNetQ.Tests;
#endregion

namespace Avitia.Controller
{
    /// <summary>
    /// RabbitMQ Bus Management Components and Notes.
    /// <list type="bullet">AMPQ Protocol Components 
    ///     <item>Exchange - (Message Transfer Agent) receives messages from publisher applications and routes these to "message queues",
    ///           based on arbitrary criteria, usually message properties or content.  Exchanges never store messages.</item>
    ///     <item>Exchange Type - The algorithm and implementation of a particular model of exchange (Topic, Direct, Fanout)</item>
    ///     <item>Message Queue - (Mailbox) stores messages until they can be safely processed by a consuming client 
    ///           application (or multiple applications)</item>
    ///     <item>Binding - (Routing Tables in Exchange) defines the relationship between a message queue and an 
    ///           exchange and provides message routing criteria</item>
    ///     <item>Publishers - send messages to individual transfer agents</item>
    ///     <item>Consumers - take messages from mailboxes. </item>
    ///     <item>Routing key: A virtual address that an exchange may use to decide how to route a specific message.</item>
    /// </list>
    /// 
    /// <list type="bullet">Routing
    ///     <item>Direct - binding key must match the routing key exactly – no wildcard support.</item>
    ///     <item>point-to-point - routing key is usually the name of a message queue. </item>
    ///     <item>topic pub-sub - routing key is usually the topic hierarchy value.</item>
    ///     <item>routing key corresponds to an email To: or Cc: or Bcc: address, without the server information</item>
    ///     <item>Queue Name - if left unspecified, the server chooses a name and provides this to the client. Generally, when
    ///           applications share a message queue they agree on a message queue name beforehand, and when an
    ///           application needs a message queue for its own purposes, it lets the server provide a name</item>
    /// </list>
    /// 
    /// Queue Type Selection - the default is transient.  The queues are not persistent and are 
    ///         released when the exchange is released by the object.
    /// <list type="bullet">Queue Usage:
    ///     <item>Queue.Declare   queue=app.svc01</item>
    ///     <item>Basic.Subscribe queue=app.svc01</item>
    ///     <item>Basic.Publish   routing-key=app.svc01</item>
    ///     <item></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Two Bus types are available
    ///     <list type="bullet">
    ///     <item>Default - Queues are created and managed from RabbitMQ Console</item>
    ///     <item>InMemory - Queues are totally in memory and not managed from any console.  For development
    ///           use the default option and set to inmemory for production when speed is critical.
    ///           Persistence is not available with this option</item>
    ///     <item><see cref="http://www.rabbitmq.com/documentation.html"/></item>
    ///     </list>
    /// 
    /// Exchange to Exchange Routing: The routing toplogy employes both 
    /// <list type="bullet">
    ///     <item></item>
    ///     <item><see cref="http://www.rabbitmq.com/blog/2010/10/19/exchange-to-exchange-bindings/"/></item>
    /// </list>
    /// </remarks>
    public class NodeInterface : IDisposable
    {
        #region ServiceBus
        /// <summary>
        /// Reference to the Service provided cancellation token
        /// </summary>
        public CancellationToken m_cancellation;
        /// <summary>
        /// ServiceBus traffic statistics on incoming/outgoing message and avg ET for each .
        /// </summary>
        public NodeStatus ns = new NodeStatus();
        /// <summary>
        /// Local transient memory Exchange 
        /// </summary>
        private IExchange memoryExchange;

        /// <summary>
        /// this Node Component identifier
        /// </summary>
        public SystemNode thisComponent;
        /// <summary>
        /// List of topics to subscribe to.  By default this is the local component.
        /// </summary>
        /// <remarks>
        /// The MQNetwork will have multiple subscription topics as the Externale gateway
        /// foor routing between internal components and the external interfaces.
        /// </remarks>
        public List<String> SubscriptionTopics = new List<String>();

        /// <summary>
        /// List of bound Routekey(s) to route/publish messages.
        /// </summary>
        public List<String> Publish = new List<String>();
        /// <summary>
        /// list of topics to bind the single internal queue for this component. 
        /// </summary>
        public List<String> Subscribe = new List<String>();
        #endregion        
        #region LocalQueue
        /// <summary>
        /// Timeout (ms) waiting for queuing operations 
        /// </summary>
        public int m_ReceiveTimeoutx = 256;

        private readonly object _syncLock = new object();
        /// <summary>
        /// Thread-Safe shared queue for passing messages thru the network m_nodecomponents. Incoming message queueBuffer
        /// Contains command and synchronization messages from all node components for routing to
        /// other components or the controller. 
        /// </summary>
        /// <remarks>Uses m_outgoingqueuebase as the base object</remarks>
        public BlockingCollection<IMessage<NodeMessage>> m_outgoingqueue;
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentQueue<IMessage<NodeMessage>> m_outgoingqueuebase = new ConcurrentQueue<IMessage<NodeMessage>>();
        #endregion
        #region Service Configuration
        /// <summary>
        /// 
        /// </summary>
        protected Task[] m_messaging = new Task[2];
        #endregion        

        /// <summary>
        /// Default Constructor-connect to service bus
        /// <list type="bullet">Memory Exchange Construction
        ///     <item>Start the publishing task</item>
        ///     <item>Topic Routing - requires node based RouteKey</item>
        ///     <item>Auto-delete (exchange andQueues are deleted when all queues have closed)</item>
        /// </list>
        /// <list type="bullet">Local Persistent Exchange Construction
        ///     <item>Topic Routing - requires node based RouteKey</item>
        ///     <item>Durable (exchange & Queues persist)</item>
        /// </list>
        /// <list type="bullet">Queue Construction
        ///     <item>Instantiate uniquely named transient queue</item>
        ///     <item>Bind the queue to the exchange</item>
        /// </list>
        /// </summary>
        /// 
        /// <param name="_service">
        /// <list type="bullet">Host service for this node interface, for directly accessing following:
        ///     <item>Cancellation token for process control by owning process</item>
        ///     <item>Message handler to be invoked by the incoming topic queue</item>
        ///     <item>Service component type for queue naming and topic routing identification</item>
        /// </list>
        /// </param>
        /// <param name="_topology">Define Exchange type to be used by this node  </param>
        public NodeInterface(NodeServiceBase _service) 
        {
            #region Node Overhead
            this.m_cancellation = _service.m_nodetoken;                              //  cancellation token
            this.m_outgoingqueue = new BlockingCollection<IMessage<NodeMessage>>(this.m_outgoingqueuebase);

            m_messaging[1] = new Task(() => { this.PublishQueuedMessage(); }, TaskCreationOptions.LongRunning);
            m_messaging[1].Start();        //  asynchronous start
            #endregion
            #region Exchange Configuration
            this.thisComponent = _service.thisComponent;   //  this node identifier
            this.SubscriptionTopics.Add(this.thisComponent.ToString() + MQNetwork.Wildcard);
            this.memoryExchange =               //  (2) Instantiate Topic Only MTA - routes on a routing pattern and is deleted when all references close.
                Exchange.DeclareTopic(ConfigurationManager.AppSettings["SBmemoryExchange"]);
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        public void QueueConfiguration(NodeServiceBase _service)
        {
            try
            {
                Func<IMessage<NodeMessage>, MessageReceivedInfo, Task> msghandler =
                    new Func<IMessage<NodeMessage>, MessageReceivedInfo, Task>(_service.ServiceMessageHandler);
                                                //  (3) Mailbox/Queue Declarations for subscribing to messages
                IQueue queue =  Queue.DeclareTransient("Q_" + this.thisComponent.ToString());
                queue.BindTo(this.memoryExchange,  this.Subscribe.ToArray());        //  (3a) Bind Mailbox(s) to MTA - declaring the RoutingKey/routing pattern to accept for this queue
                MQNetwork.mqQueue(queue, msghandler);   //  (3b) Create Queue to memory Client Bus
            }
		    catch (Exception)
            {
            } 
        }
        /// <summary>
        /// Queue message to a local concurrent thread-safe queue.  
        /// This process minimizes waiting when the servicebus is remote or in a busy state.
        /// <list type="number">Message Identifier Components
        ///     <item>message.Properties.AppId      -   internal routingkey destination component</item>
        ///     <item>message.Properties.ReplyTo    -   internal source component</item>
        ///     <item>message.Properties.Type       -   (optional) External destination</item>
        ///     <item>message.Properties.Timestamp  -   timestamp in ticks when message entered the MQ</item>
        ///     <item></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// Note that a timeout or token cancellation is handled as an exception 
        /// </remarks>
        /// <param name="message"></param>
        /// <returns>True if successful</returns>
        public Boolean QueueMessage(IMessage<NodeMessage> message)
        {
            if (message.Properties.AppId == null)
            {
                Console.WriteLine(String.Format("1440 QueueMessage({0}) No RouteKey present",this.thisComponent.ToString()));
                return (false);
            }
            while (true)
            {
                try
                {
                    message.Properties.ReplyTo = this.thisComponent.ToString();
                    message.Properties.Timestamp = (DateTime.Now.Ticks);
                    if (this.m_outgoingqueue.TryAdd(message,
                        m_ReceiveTimeoutx,                      //  set timeout waiting for data to be added 
                        m_cancellation))
                    {
                        return (true);                          //  successfully locally queued
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("timed out"))
                    {
                        continue;             //  ignore
                    }
                    if (ex.Message.Contains("task was"))
                    {
                        return (false);       //    restart requested  
                    }
                    return (false);
                }
            }
        }
        /// <summary>
        /// Dequeue local message and publish the queued Message thru the RabbitMQ servicebus.
        /// 
        /// </summary>
        /// <remarks>
        /// Read and process messages on incoming message localQueue until Network cancellation/Exception issued.
        /// <list type="number">
        ///     <item>Block on reading localQueue until message arrives or timeout occurs.
        ///           Timeouts are retryable and continues, IntervalPending period is configured at startup.</item>
        ///     <item>Process Message (optional)</item>
        ///     <item>Route to message.Properties.AppId</item>
        ///     <item>(Optional) For external routing Publish to MQGateway and message.Properties.Type for the external destination</item>
        ///     <item>Log that message was routed and the elapsed time (ms) since origination message has been queued</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        public void PublishQueuedMessage()
        {
            IMessage<NodeMessage> message;
            IAdvancedPublishChannel channel = MQNetwork.mqChannel();
            while (true)
            {
                try
                {
                    if (this.m_outgoingqueue.TryTake(out message,
                            m_ReceiveTimeoutx,              //  set timeout waiting for data to be available 
                            m_cancellation))
                    {
                            channel.Publish(this.memoryExchange, message.Properties.AppId, message);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("timed out"))
                    {
                        continue;       //  ignore
                    }
                    if (ex.Message.Contains("was canceled"))
                    {
                        channel.Dispose();
                        Console.WriteLine( String.Format("1420 Node({0}) Stopped - inET({1}) inCt({2}) ouET({3}) ouCt({4})",
                            this.thisComponent, this.ns.IncomingAvgET, this.ns.m_incomingcount, this.ns.OutgoingAvgET, this.ns.m_outgoingcount));
                        return;       //  end the thread
                    }
                }
            }
        }

        /// <summary>
        /// Create point-to-point topic routing key between two nodes.
        /// This key serves both for Publishing and Subscription Queue Configuration.  
        /// </summary>
        /// <remarks>
        ///                         [From]       [To]
        /// 
        /// Controller Publish:     [Controller].[Memory]
        /// Controller Subscribe:   [*]         .[Controller]
        ///                                                   
        /// Memory Publish:         [Memory]    .[Controller]
        /// Memory Subscribe:       [*]         .[Memory]
        /// </remarks>
        /// <param name="_from">source component</param>
        /// <param name="_to">destination Component</param>
        /// <returns>Topic RouteKey</returns>
        public String RouteKey(SystemNode _from,SystemNode _to)
        {
            return (_to.ToString()+"."+_from.ToString());
        }
       
        /// <summary>
        /// Deconstructor
        /// </summary>
        public void Dispose()
        {
            this.m_messaging[1].Dispose();
            GC.SuppressFinalize(this);
        }

    }
}
