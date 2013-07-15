using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avitia.Controller.Messages
{
    /// <summary>
    /// Queue Operation result status codes
    /// </summary>
    public enum MessageStatus
    {
        /// <summary>
        /// Operation has not been executed and is pending execution 
        /// </summary>
        Pending,
        /// <summary>
        /// Operation has completed successfully 
        /// </summary>
        OK,
        /// <summary>
        /// Operation hasn't completed and continue to inspect feedback data
        /// to determine if the operation is continuing properly.
        /// </summary>
        Continue,
        /// <summary>
        /// Feedback indicated an anomally - though can still continue 
        /// </summary>
        Warning,
        /// <summary>
        /// 
        /// </summary>
        Critical,
        /// <summary>
        /// Operation identified an unrecoverable state 
        /// </summary>
        Fatal
    }
    /// <summary>
    /// Priority Queue operations
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// keep-alive command 
        /// </summary>
        oHeartBeat = 9000,
        /// <summary>
        /// 
        /// </summary>
        oSPRecognition = 5000,
        /// <summary>
        /// 
        /// </summary>
        oSPRecognitionResult = 5010,
        /// <summary>
        /// 
        /// </summary>
        oSPSynthesis = 6000,
        /// <summary>
        /// Thread exit, reset from SystemControllerX 
        /// </summary>
        oReset = 1000,
        /// <summary>
        /// The controller issued a cancel request to an inprocess operation
        /// </summary>
        oCancel = 1100
    }
    /// <summary>
    /// Node services and other system components and services that are node addressable.
    /// </summary>
    public enum NodeServiceIdentifier
    {
        /// <summary>
        /// SysLog - log messages to the defined local or remote syslog service. 
        /// </summary>
        cSyslog = 0,
        /// <summary>
        /// Speech - voice synthesizer receives SSML to execute 
        /// </summary>
        cSpeech = 1,
        /// <summary>
        /// Audio - detects and recognizes Grammar defined speech  
        /// </summary>
        cAudio = 2,
        /// <summary>
        /// Video - processes visual recognition of faces and landmark objects 
        /// </summary>
        cVideo = 3,
        /// <summary>
        /// I2C - manages communication to the I2C devices managed by the microcontroller 
        /// </summary>
        cI2C = 4,
        /// <summary>
        /// Console - management and administrative messaging to the webservice console 
        /// </summary>
        cConsole = 5,
        /// <summary>
        /// Router - router/controller service
        /// </summary>
        cRouter = 6,
        /// <summary>
        /// Broadcast - messaging intended for all active services
        /// </summary>
        cBroadcast
    }
    
}
