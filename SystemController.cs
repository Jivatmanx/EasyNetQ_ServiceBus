using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Diagnostics;


namespace Avitia.Controller
{
    /// <summary>
    /// SystemController - start and manage all service node traffic.
    /// 
    /// Construct and instantiate Controller bound Node Services and Routers.
    /// </remarks>
    public class SystemController : IDisposable
    {
        /// <summary>
        /// Number of ms to block on reading the incoming message localQueue
        /// </summary>
        private int m_localTimeout = 512;

        #region Node Service Controls
        /// <summary>
        /// NodeService Definitions
        /// List of service mnemonics and the service class.  The service class is inherited from the nodeservice base class.
        /// </summary>
        protected readonly List<NodeServiceBase> services = new List<NodeServiceBase>()
            {
                { new NodeServiceGeneric(SystemNode.Controller)}
               ,{ new NodeServiceGeneric(SystemNode.Environment)}
               ,{ new NodeServiceGeneric(SystemNode.Memory)}
            };
        /// <summary>
        /// The running service signals to the SysetmController when it terminates. 
        /// </summary>
        WaitHandle[] m_waitHandles;
        /// <summary>
        /// Service threads - long lasting controlled by service cancellation token 
        /// </summary>
        private Thread[] m_ComponentThreadx;
        /// <summary>
        /// Managed node service object instatiated by the controller. 
        /// </summary>
        protected NodeServiceBase[] service;
        /// <summary>
        /// Cancellation signal generator - one for each service instantiated by the conytroller.  
        /// </summary>
        protected CancellationTokenSource[] tokenSource;    // The token ServiceSource for issuing the cancelation request.
        /// <summary>
        /// Cancellation tokens referenced by controller managed node services.
        /// </summary>
        protected CancellationToken[] token;                // issued token 
        #endregion

        /// <summary>
        /// Define all the System StartupTimer controls. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public SystemController()
        {
            #region Define Node Service Arrays
            this.service = new NodeServiceBase[services.Count];                 //  service pattern
            this.m_waitHandles = new WaitHandle[services.Count];
            this.m_ComponentThreadx = new Thread[services.Count];           //  service threads
            this.tokenSource = new CancellationTokenSource[services.Count]; //  
            this.token = new CancellationToken[services.Count];             //  service cancellation
            #endregion

            #region Cancellation Token Instantiations
            for (int i = 0; i < services.Count; i++)
            {
                this.tokenSource[i] = new CancellationTokenSource();    //  factory
                this.token[i] = this.tokenSource[i].Token;                
            }
            #endregion
        }
        /// <summary>
        /// Configure and Instantiate Node Service(s) on separate threads
        /// </summary>
        /// <returns></returns>
        public Boolean Startup()
        {
            try
            {
                Console.WriteLine(String.Format("1005 Controller Startup"));
                for (int i = 0; i < services.Count; i++)
                {
                    var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);    // new wait handle for stopping thread
                    this.m_waitHandles[i] = waitHandle;
                    this.service[i] = services[i];
                    try
                    {
                        this.service[i].Configure( this.token[i], waitHandle);//    define network

                        NodeServiceBase serviceTask = this.service[i];      //  instantiate service - index value not stable inside Task  
                        
                        this.m_ComponentThreadx[i] = new Thread(new ParameterizedThreadStart(serviceTask.Service));     //  assign to thread container
                        this.m_ComponentThreadx[i].Name = this.service[i].thisComponent.ToString();                     //  identifier
                        this.m_ComponentThreadx[i].Start(new object());     //  pass empty parameter to task (to be defined)
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(String.Format("2200 ConfigureNodeService({0})-Ex:({1})", this.service[i].ToString(), ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("2010 Controller StartupTimer() Ex({0})", ex.Message));
                return (false);
            }
            return (true);        
        }
        /// <summary>
        /// Monitor running threads for stops - restart when stopped  
        /// </summary>
        /// <param name="msecs">testing period</param>
        public void Monitor(long msecs)
        {
            Thread.Sleep(5000); //  pause for threads to get started
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            while (true)
            {
                foreach (var item in this.m_ComponentThreadx)
                {
                    if (stopwatch.Elapsed.TotalMilliseconds > msecs)    //  timeout?
                        return;                                         //  o
                    try
                    {
                        if (item.Join(100))
                        {
                            Console.WriteLine(String.Format("4320 Controller Thread({0}) Stopped-Restarting", item.Name));		
                            item.Start();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        /// <summary>
        /// System Shutdown
        /// 
        /// Issue a Cancel Token first at the nodeservice level then the global service token
        /// After all nodes signal completion execute some cleanup tasks and release all resources.
        /// </summary>
        /// <remarks>
        /// Stop and wait at least 1s for each service component to complete.  If you don't wait long enough
        /// they processes may not all be stopped.
        /// <list type="bullet">
        ///     <item>All Node/Statemachine services</item>
        ///     <item>outgoing Node Router</item>
        ///     <item>outgoing statemachine Router</item>
        ///     <item>Incoming message management Router</item>
        /// </list>
        /// Final cleanup - dispose all resources..
        /// </remarks>
        public void ShutDown()
        {
            for (int i = 0; i < this.services.Count; i++)
            {
                this.tokenSource[i].Cancel(true);       //  signal service token cancellation request
                this.service[i].StopService();          //  stop thread
            }
            try 
            {
                Thread.Sleep(512);
                if( WaitHandle.WaitAll(this.m_waitHandles,this.m_localTimeout*8))
                    Console.WriteLine(String.Format("4200 Controller Shutdown"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("4210 Controller Ex({0})", ex.Message));		
            }
            finally
            {
                MQNetwork.StopServiceBus();     //  release resources
                this.Dispose();                 //  final garbage collection
            }
        }
       
        /// <summary>
        /// IDisposableinterface 
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
