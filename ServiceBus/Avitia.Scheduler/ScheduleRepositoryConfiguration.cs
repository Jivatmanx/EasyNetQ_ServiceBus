using System.Configuration;

namespace Avitia.Scheduler
{
    /// <summary>
    /// Windows Service operational configuration settings from App.Config
    /// </summary>
    public class ScheduleRepositoryConfiguration : ConfigurationBase
    {
        private const string connectionStringKey = "scheduleDb";

        public string ConnectionString { get; set; }
        public int PurgeBatchSize { get; set; }
        public int MaximumScheduleMessagesToReturn { get; set; }

        /// <summary>
        /// The number of days after a schedule item triggers before it is purged.
        /// </summary>
        public int PurgeDelayDays { get; set; }
        /// <summary>
        /// Read from App.Config
        /// </summary>
        /// <returns></returns>
        public static ScheduleRepositoryConfiguration FromConfigFile()
        {
            return new ScheduleRepositoryConfiguration
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[connectionStringKey].ConnectionString,
                PurgeBatchSize = GetIntAppSetting("PurgeBatchSize"),
                PurgeDelayDays = GetIntAppSetting("PurgeDelayDays"),
                MaximumScheduleMessagesToReturn = GetIntAppSetting("MaximumScheduleMessagesToReturn")
            };
        }
    }
}