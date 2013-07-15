using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Avitia.Controller
{
    /// <summary>
    /// Machine states dictated by an aggregate of the service interface conditions.
    /// </summary>
    public enum MachineStates
    {
        /// <summary>
        /// Machine is starting up and awaiting for an operational interface/sensor status
        /// </summary>
        Initialize = 0,
        /// <summary>
        /// Machine is waiting for interface/sensors and Context logic
        /// </summary>
        Discovery = 1,
        /// <summary>
        /// Machine is motion to reach an identified location
        /// </summary>
        Motion,
        /// <summary>
        /// Machine is interacting with speech synthesis and recognition
        /// </summary>
        Conversation,
        /// <summary>
        /// Machine is operational and awaiting for sensor/interface event
        /// </summary>
        Attention,
    }
}
