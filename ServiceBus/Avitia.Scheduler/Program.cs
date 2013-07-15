using log4net.Config;
using Topshelf;

namespace Avitia.Scheduler
{
    public class Program
    {
        /// <summary>
        /// Install Avitia.Scheduler as a Windows Service - Topshelf builds an instance of the service
        /// <list type="bullet">
        ///     <item>Runs the service using the local system account - SQL Security Login map NT Authority/System account to the Avitia.System database</item>
        ///     <item>Delayed start</item>
        /// </list>
        /// </summary>
        static void Main()
        {
            XmlConfigurator.Configure();

            HostFactory.Run(hostConfiguration =>
            {
                #region Service Settings
                hostConfiguration.SetDescription("Avitia.Machine (1.05) Scheduler Service for controller events.");
                hostConfiguration.SetDisplayName("Avitia.Scheduler");
                hostConfiguration.SetServiceName("Avitia.Scheduler");

                hostConfiguration.RunAsLocalSystem();
                hostConfiguration.StartAutomaticallyDelayed();
                hostConfiguration.DependsOnMsSql();
                //hostConfiguration.SetInstanceName("A1");
                
                #endregion
                //  The lambda that gets opened here to expose the service configuration options 
                hostConfiguration.Service<ISchedulerService>(serviceConfiguration =>
                {
                    serviceConfiguration.ConstructUsing(_ => SchedulerServiceFactory.CreateScheduler());//  .

                    serviceConfiguration.WhenStarted((service, _) =>
                    {
                        service.Start();
                        return true;
                    });
                    serviceConfiguration.WhenStopped((service, _) =>
                    {
                        service.Stop();
                        return true;
                    });
                });
            });
        }
    }
}
