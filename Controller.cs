using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;


namespace Avitia.Controller
{
    /// <summary>
    /// Standalone Testing of SystemController
    /// </summary>
    class Controller
    {
        static void Main(string[] args)
        {
            MQNetwork.StartServiceBus();       //  instantiate shared inMemory RabbitMQ Exchange
            SystemController sys = new SystemController();

            sys.Startup();
            //sys.Monitor(long.MaxValue);
            sys.Monitor(10000);
            sys.ShutDown();

            Environment.Exit(0);
        }
    }
}
