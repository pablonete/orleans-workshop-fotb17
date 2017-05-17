using Interface;
using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System.Threading.Tasks;

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

            System.Console.WriteLine("Silo started.");

            var clientConfig = ClientConfiguration.LocalhostSilo();
            var client = new ClientBuilder().UseConfiguration(clientConfig).Build();
            client.Connect().Wait();

            System.Console.WriteLine("Client connected");

            Test(client).Wait();

            System.Console.WriteLine("Press any key...");
            System.Console.ReadKey();
        }

        public static async Task Test(IClusterClient client)
        {
            var mark = client.GetGrain<IUser>("mark@b.com");
            await mark.SetName("Mark");
            await mark.SetStatus("Share your lie with me!");

            var jack = client.GetGrain<IUser>("jack@b.com");
            await jack.SetName("Jack");
            await jack.SetStatus("Tweet me!");

            var props = await mark.GetProperties();
            System.Console.WriteLine($"Mark: {props}");

            var jackProps = await jack.GetProperties();
            System.Console.WriteLine($"Mark: {jackProps}");
        }
    }
}
