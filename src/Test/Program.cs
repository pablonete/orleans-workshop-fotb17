using Interface;
using Orleans;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
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

            await WriteUserProps(mark);
            await WriteUserProps(jack);

            var ok = await mark.AddFriend(jack);
            if (ok)
            {
                Console.WriteLine("Invited!");
                await WriteUserProps(mark);
                await WriteUserProps(jack);
            }

            for (int i = 0; i < 10; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                await user.SetName($"User #{i}");
                await user.SetStatus(i % 3 == 0 ? "Sad" : "Happy");
                await (i % 2 == 0 ? mark : jack).AddFriend(user);
                await WriteUserProps(user);
            }

            await WriteUserProps(mark);
            await WriteUserProps(jack);
        }

        private static async Task WriteUserProps(IUser user)
        {
            var props = await user.GetProperties();
            Console.WriteLine($"{user.GetPrimaryKeyString()}: {props}");
        }
    }
}
