using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Xml;

using EasyNetQ;

namespace Avitia.Controller
{
    /// <summary>
    /// Node Service Base Class
    /// Encapsulates the Device Interface and other services that operate as an
    /// asynchronous service requiring communication with the Controller context.
    /// 
    /// <list type="bullet">Base Functionality:
    ///     <item>Instantiate node object</item>
    ///     <item>Connect node outoing to controller router</item>
    ///     <item>Message Recieve Interface</item>
    ///     <item>Message Send Interface</item>
    ///     <item>TimeContinuum response service</item>
    /// </list> 
    /// </summary>
    /// <remarks>
    /// This class is abstracted by higher level classes that define the Service() method 
    /// to identify the service management layer.
    /// </remarks>
    public abstract class NodeServiceBase :IDisposable
    {
        #region Service Configuration Settings
        /// <summary>
        /// Machine Instance ()
        /// </summary>
        //public String instance;
        /// <summary>
        /// this Node Component identifier
        /// Local Node Service Mnemonic Identifier.  Cascaded down to all
        /// contained hosted controls.
        /// </summary>
        public SystemNode thisComponent;
        /// <summary>
        /// Amount of seconds to block on an AsyncReceive
        /// </summary>
        //private TimeSpan m_ReceiveTimeout = TimeSpan.FromSeconds(5);
        /// <summary>
        /// 
        /// </summary>
        public CancellationTokenSource m_nodetokenSource = new CancellationTokenSource();
        /// <summary>
        /// controlling this service's node process - force restart  
        /// </summary>
        public CancellationToken m_nodetoken;                
        /// <summary>
        /// Reference to the Router/Controller cancellation token
        /// </summary>
        protected CancellationToken m_cancellationController;
        #endregion
        
        #region Threading Synchronization
        /// <summary>
        /// ServiceBus interface.
        /// Manages incoming and outgoing RabbitMQ messages.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// </list>
        /// </remarks> 
        public NodeInterface m_node;
        /// <summary>
        /// 
        /// </summary>
        public EventWaitHandle m_waitHandle;        
        #endregion

        /// <summary>
        /// Base Constructor - defined by SystemController
        /// </summary>
        /// <param name="_component">Service Identifier</param>
        public NodeServiceBase(SystemNode _component)
        {
            this.thisComponent = _component;            //  node identifier
        }
        /// <summary>
        /// Connects/Links outgoing block to invoking router/controller instance.
        /// <list type="number">
        ///     <item>Initialize the interface node</item>
        ///     <item>Connect outgoing node connection to the controller router</item>
        ///     <item>Set miscellanous meta configurations</item>
        ///     <item>Start asynchronous interface tasks</item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// Note.  The incoming controller connection is established here. The outgoing router was connected 
        /// by the controller at the time the NodeService was defined.
        /// </remarks>
        /// <param name="cct">Controller initiated cancellation token</param>
        /// <param name="_waitHandle">Process completion signal</param>
        public Boolean Configure(
                    CancellationToken cct,
                    EventWaitHandle _waitHandle)
        {
            this.m_waitHandle = _waitHandle;
            this.m_cancellationController = cct;        //  controller cancellation token

            this.m_nodetoken = m_nodetokenSource.Token; //  instantiate the token                
            #region Build Node
            try
            {
                this.m_node = MQNetwork.BuildNode(this);    //  initialize the node interface traffic
            }
		    catch (Exception ex)
            {
                Console.WriteLine(String.Format("1102 Service({0}) Ex:({1})", this.thisComponent,ex.Message));
                return(false);
            }

            Console.WriteLine(String.Format("1100 Service({0}) Started", this.thisComponent));
            return (true);
 
	        #endregion        
        }
        /// <summary>
        /// The controller set the CancellationToken to cascade to the NodeInterface
        /// to start the service shutdown process. 
        /// 
        /// <list type="number">
        ///     <item>Set nodeinterface cancellation</item>
        ///     <item>Stop the nodeinterface threads</item>
        ///     <item>Signal to the controller that this service has stopped</item>
        ///     <item></item>
        /// </list>
        /// </summary>
        public void StopService()
        {
            try
            {
                this.m_nodetokenSource.Cancel(true);            //  cancel the nodeinterface
                Thread.Sleep(25);               //  wait for catchup..
                this.m_waitHandle.Set();        //  signal to the controller
            }
            catch (Exception)
            {
                Console.WriteLine(String.Format("1121 Service({0}) Aborted", this.thisComponent));
            }
        }

        /// <summary>
        /// Defined by the class that extends this base class to operate on the 
        /// generated data and messages received from the router/controller.
        /// 
        /// Intermediate Message handler.  Provides a method to convert an incoming message to
        /// a format that can be consumed by ServiceDestination dervice.
        /// 
        /// </summary>
        /// <remarks>
        /// Connect this method to the node newtork interface.
        /// 
        /// The owning conatiner is provided access to method interfaces for incoming and outgoing
        /// messaging.   Leaving the queuing and blocking to the underlying NodeService construct.
        /// 
        /// Implementing the interfaces is a design factor depending on the intended use of the final 
        /// service.
        /// <list type="bullet">
        ///     <item>Spawn the inderlying incoming/outgoing message interfaces</item>
        ///     <item>Determine if the device is a consumer or producer of messages</item>
        ///     <item>Asynchrounous start the  message receive interface</item>
        ///     <item>When terminated wait for the ReceiveService to exit before exiting the Service</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        public abstract void Service(object obj);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public abstract Task ServiceMessageHandler(IMessage<NodeMessage> message, MessageReceivedInfo info);

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            this.m_node.Dispose();      //  nodeinterface 
        }

        /// <summary>
        /// Convert first node name in the routekey to the equivalent SystemNode Enum.
        /// This results in an integral type that can be used in a switch statement and 
        /// indexing the m_serviceTypes dictionary. 
        /// </summary>
        /// <param name="route">Routekey with two or more nodes.</param>
        /// <returns>Corresponding SystemNode Enum for first string node</returns>
        public SystemNode GetDestinationNode(String route)
        {
            try
            {
                String[] nodes = route.Split('.');
                SystemNode node = (SystemNode)Enum.Parse(typeof(SystemNode), nodes[0]);
                return (node);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("1128 Service({0}) Unknown Route({1})-({2})", this.thisComponent, route, ex.Message));
                return (SystemNode.Unidentified);       //  not found
            }
        }
    }
}
