using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyNetQ.SystemMessages;
using EasyNetQ;
namespace Avitia.Library
{
    /// <summary>
    /// Generic container - enveloped by the NodeMessage for final delivery
    /// </summary>
    [Serializable]
    public abstract class ContainerMessageBase 
    {
        /// <summary>
        /// Define a base container message
        /// </summary>
        public ContainerMessageBase()
        {
            try
            {
            }
            catch (Exception)
            { }
        }


    }
}
