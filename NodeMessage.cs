using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyNetQ.SystemMessages;

namespace Avitia.Library
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
}
