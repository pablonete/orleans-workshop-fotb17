using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo();
            siloConfig.Defaults.DefaultTraceLevel = Orleans.Runtime.Severity.Warning;
            var silo = new SiloHost("Test Silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            System.Console.WriteLine("Press any key...");
            System.Console.ReadKey();
        }
    }
}
