using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ.SystemMessages;

namespace Avitia.Controller
{
    /// <summary>
    /// Forms the Body attribute of the IMessage class for all interprocess Avitia system messages
    /// An object that specifies the message contents. 
    /// </summary>
    [Serializable]
    public class NodeMessage
    {
        /// <summary>
        /// The object type stored in DataObject 
        /// The DataType property indicates the type of information that is stored in the message body.
        /// </summary>
        public Type DataType;
        /// <summary>
        /// The Object property can be any serializable object, such as a text string, structure object, class instance, or embedded object.
        /// Container Object for other classes and structures.  The corresponding
        /// Byte[] array for binary is provided for storing to SQL or other repository.
        /// </summary>
        public Object DataObject;
        //public Byte[] DataObjectContainer
        //{
        //    #region Get
        //    get
        //    {       //  return a byte array
        //        if (this.DataObject == null) this.DataObject = new Object();
        //        return (MessageTools.ObjectToByteArray(this.DataObject));
        //    }
        //    #endregion
        //    #region Set
        //    set
        //    {       //  store to generic object container
        //        if(!(value == null))
        //            this.DataObject = (Object)MessageTools.ByteArrayToObject(value);
        //    }
        //    #endregion
        //}

        #region Testing
        ///// <summary>
        ///// 
        ///// </summary>
        //private Byte[] _text;
        ///// <summary>
        ///// 
        ///// </summary>
        //public string Text
        //{
        //    get
        //    {
        //        return (Encoding.UTF8.GetString(_text));
        //    }
        //    set
        //    {
        //        _text = Encoding.UTF8.GetBytes(value);
        //    }
        //}
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public NodeMessage()
        {
        }
        /// <summary>
        /// Store and record a generic object container
        /// </summary>
        /// <param name="obj"></param>
        public void AddObject(Object obj)
        {
            this.DataType = obj.GetType();
            this.DataObject = obj; 
        }
    }

    /// <summary>
    /// Node services, state machines and other system components that are node addressable.
    /// 
    /// The value is the Socket port# when required
    /// 
    /// Each Node communicates with a custom message type inherited derived from containerMessageBase
    /// </summary>
    /// <remarks>
    /// When the identifier is implemented an identically named XML configuration must be located in the
    /// CONFIG location to be read by the ORM layer.
    /// <list type="bullet">
    ///     Node prefix identifiers
    ///     <item>ns - node service implements a device or service interface</item>
    ///     <item>nm - node machine implementing a state machine controller</item>
    ///     <item></item>
    /// </list>
    /// </remarks>
    public enum SystemNode
    {
        /// <summary>
        /// Primary system synchronization 
        /// </summary>
        Controller = 1,
        /// <summary>
        /// </summary>
        Environment = 2,
        /// <summary>
        /// 
        /// </summary>
        Identity = 3,
        /// <summary>
        /// 
        /// </summary>
        Memory = 4,
        /// <summary>
        /// Failed to identify a node as a SystemNode  
        /// </summary>
        Unidentified = 49
    }
    /// <summary>
    /// Track counters and other data related to monitoring 
    /// traffic statistics at the node interface.
    /// </summary>
    public class NodeStatus
    {
        /// <summary>
        /// count of incoming messages received 
        /// </summary>
        public int m_incomingcount = 0;
        /// <summary>
        /// ET of incoming messages.
        /// Everytime it's set indicates a new message, keep count.
        /// </summary>
        private int _incomingETTotal = 0;
        /// <summary>
        /// 
        /// </summary>
        public int m_incomingETTotal
        {
            get { return this._incomingETTotal; }
            set
            {
                this.m_incomingcount++;
                this._incomingETTotal = value;
            }
        }
        /// <summary>
        /// Calculate the average
        /// </summary>
        public int IncomingAvgET
        {
            get
            {
                try
                {
                    return (int)(this.m_incomingETTotal / this.m_incomingcount);
                }
                catch (Exception)   //  math error - just return 0, error should show up elsewhere
                {
                    return (0);
                }
            }
        }

        /// <summary>
        /// count of outgoing messages sent 
        /// </summary>
        public int m_outgoingcount = 0;
        /// <summary>
        /// ET of outgoing messages 
        /// </summary>
        private int _outgoingETTotal = 0;
        /// <summary>
        /// 
        /// </summary>
        public int m_outgoingETTotal
        {
            get { return this._outgoingETTotal; }
            set
            {
                this.m_outgoingcount++;
                this._outgoingETTotal = value;
            }
        }
        /// <summary>
        /// Calculate the average
        /// </summary>
        public int OutgoingAvgET
        {
            get
            {
                try
                {
                    return (int)(this.m_outgoingETTotal / this.m_outgoingcount);
                }
                catch (Exception)   //  math error - just return 0, error should show up elsewhere
                {
                    return (0);
                }
            }
        }

    }

}
