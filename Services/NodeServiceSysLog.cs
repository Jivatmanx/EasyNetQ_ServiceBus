using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;
using System.Collections.Concurrent;
using Avitia.Library;
using Avitia.Controller.Messages;
using Avitia.Controller.Network;

namespace Avitia.Controller.Services
{
    #region Namespace Description
    /// <summary>
    /// Define all interface service m_nodecomponents encapsulated within the base node service class.
    /// Each service is specific to the hardware interface being supported.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }
    
    #endregion

    /// <summary>
    /// Syslog node service defined operations
    /// </summary>
    public class NodeServiceSysLog : NodeService
    {
        /// <summary>
        /// Default Service constructor 
        /// </summary>
        public NodeServiceSysLog()
            : base()
        {
        }
        
        /// <summary>
        /// Override the Service Handler for this service definition
        /// </summary>
        /// <remarks>
        /// Connect this method to the device interface.
        /// <list type="bullet">
        ///     <item>Determine if the device is a consumer or producer of messages</item>
        ///     <item>Asynchrounous start the  message receive interface</item>
        ///     <item>When terminated wait for the RecieveService to exit before exiting the Service</item>
        /// </list>
        /// </remarks>
        /// <returns></returns>
        public override async Task Service()
        {
            Task rcv = new Task(() => 
                { this.ReceiveMessage(); }, TaskCreationOptions.LongRunning);
            rcv.Start();        //  asynchronous start

            Random rnd = new Random(DateTime.Now.Millisecond);
            int p = 1000000;
            while (true)
            {
                try
                {
                    p--;
                    Task<Boolean> rc = this.SendMessage(p);     //  Send the controller a message                 
                    if (!rc.Result) break;
                    Thread.Sleep(rnd.Next(1024, 2048));
                }
                catch (Exception ex)
                {
                    log.log.Critical(logFacility.Service, String.Format("1110 ({0})-Ex({1})", this.m_id, ex.Message));
                    break;
                }
            }

            Task.WaitAll(rcv);
            log.log.Info(logFacility.Service, String.Format("1120 [{0}] Ending", this.m_id));
        }

    }
}
