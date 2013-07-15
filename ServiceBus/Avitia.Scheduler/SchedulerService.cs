using System;
using System.Transactions;
using EasyNetQ.SystemMessages;
using EasyNetQ.Topology;
using EasyNetQ.Loggers;
using EasyNetQ;
namespace Avitia.Scheduler
{
    public interface ISchedulerService
    {
        void Start();
        void Stop();
    }
    /// <summary>
    /// Windows Service 
    /// 
    /// Polls for pending scheduled message to route to receiving queue.   The polling period
    /// is set in App.Config.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">Notes:
    ///     <item>Subscribe - Queue Name uses the full namespace of the messagetype</item>
    ///     <item>Publish - declare a topic routed exchange for routing the message</item>
    ///     <item></item>
    /// </list>
    /// </remarks>
    public class SchedulerService : ISchedulerService
    {
        private const string schedulerSubscriptionId = "schedulerSubscriptionId";
        #region Local Variables
        /// <summary>
        /// RabbitMQ Connection
        /// </summary>
        private readonly IBus bus;
        private readonly IEasyNetQLogger log;
        private readonly IScheduleRepository scheduleRepository;
        private readonly SchedulerServiceConfiguration configuration;

        private System.Threading.Timer publishTimer;
        private System.Threading.Timer purgeTimer;
        #endregion

        public SchedulerService(
            IBus bus, 
            IEasyNetQLogger log, 
            IScheduleRepository scheduleRepository, 
            SchedulerServiceConfiguration configuration)
        {
            this.bus = bus;
            this.scheduleRepository = scheduleRepository;
            this.configuration = configuration;
            this.log = log;
        }
        /// <summary>
        /// Assign delegate to the three service events.
        /// <list type="bullet">
        ///     <item>Receive scheduling requests from queue</item>
        ///     <item>Interval Polling timer to publish pending schedules</item>
        ///     <item>Interval polling timer to purge expired schedules</item>
        /// </list>
        /// </summary>
        public void Start()
        {
            log.InfoWrite("[1010] Starting SchedulerService (1.2) [{0}]",schedulerSubscriptionId);
            bus.Subscribe<ScheduleMe>(schedulerSubscriptionId, OnMessage);

            this.publishTimer = new System.Threading.Timer(OnPublishTimerTick, null, 0, configuration.PublishIntervalSeconds * 1000);
            this.purgeTimer = new System.Threading.Timer(OnPurgeTimerTick, null, 0, configuration.PurgeIntervalSeconds * 1000);
        }

        public void Stop()
        {
            log.DebugWrite("Stopping SchedulerService (1.1)");
            if (publishTimer != null)
            {
                publishTimer.Dispose();
            }
            if (purgeTimer != null)
            {
                purgeTimer.Dispose();
            }
            if (bus != null)
            {
                bus.Dispose();
            }
        }

        public void OnMessage(ScheduleMe msgIn)
        {
            log.DebugWrite("[1020] Scheduler:Got({0}) Schedule Message",msgIn.BindingKey);
            scheduleRepository.Store(msgIn);
        }
        /// <summary>
        /// Retrieve from SQL repository pending messages.  Bindingkey has intended receipient of message.
        /// Create the TransactionScope to execute the commands can commit or roll back as a single unit of work. 
        /// </summary>
        /// <param name="state"></param>
        public void OnPublishTimerTick(object state)
        {
            if (!bus.IsConnected) return;
            try
            {
                using(var scope = new TransactionScope())
                using(var channel = bus.Advanced.OpenPublishChannel())  
                {
                    var scheduledMessages = scheduleRepository.GetPending();
                    foreach (var scheduledMessage in scheduledMessages)
                    {
                        log.DebugWrite(string.Format(
                            "[1001] Publishing Scheduled Message - BindingKey: [{0}]", scheduledMessage.BindingKey));

                        var exchange = Exchange.DeclareTopic(scheduledMessage.BindingKey);
                        channel.Publish(
                            exchange, 
                            scheduledMessage.BindingKey, 
                            new MessageProperties{ Type = scheduledMessage.BindingKey }, 
                            scheduledMessage.InnerMessage);
                    }

                    scope.Complete();   //  all operations within the scope are completed successfully.
                }
            }
            catch (Exception ex)
            {
                log.ErrorWrite("Error in schedule pol:{0}", ex.Message);
            }
        }

        private void OnPurgeTimerTick(object state)
        {
            scheduleRepository.Purge();
        }
    }
}