using Interface;
using Orleans;
using Orleans.Runtime;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static readonly string connectionString = "UseDevelopmentStorage=true";

        static void Main(string[] args)
        {
            var clusterId = Guid.NewGuid().ToString();
            StartAzureTableSilo(1, clusterId);

            var client = StartAzureTableClient(clusterId);
            Test(client).Wait();

            System.Console.WriteLine("Press any key...");
            System.Console.ReadKey();
        }

        public static void StartAzureTableSilo(int index, string clusterId)
        {
            var siloConfig = ClusterConfiguration.LocalhostPrimarySilo(11110 + index, 29999 + index);
            siloConfig.Defaults.DefaultTraceLevel = Severity.Warning;
            siloConfig.Globals.LivenessType = GlobalConfiguration.LivenessProviderType.AzureTable;
            siloConfig.Globals.ReminderServiceType = GlobalConfiguration.ReminderServiceProviderType.AzureTable;
            siloConfig.Globals.DataConnectionString = connectionString;
            siloConfig.Globals.DeploymentId = clusterId;
            siloConfig.AddAzureTableStorageProvider("Storage", connectionString);

            var silo = new SiloHost("Test Silo", siloConfig);
            silo.InitializeOrleansSilo();
            silo.StartOrleansSilo();

            System.Console.WriteLine($"Silo {index} started.");
        }

        public static IClusterClient StartAzureTableClient(string clusterId)
        {
            var clientConfig = ClientConfiguration.LocalhostSilo();
            clientConfig.GatewayProvider = ClientConfiguration.GatewayProviderType.AzureTable;
            clientConfig.DataConnectionString = connectionString;
            clientConfig.DeploymentId = clusterId;

            var client = new ClientBuilder().UseConfiguration(clientConfig).Build();
            client.Connect().Wait();

            System.Console.WriteLine("Client connected");
            return client;
        }

        public static async Task Test(IClusterClient client)
        {
            var mark = client.GetGrain<IUser>("mark@b.com");
            var jack = client.GetGrain<IUser>("jack@b.com");

            // Don't populate anymore, users are already saved on storage
            // await PopulateUsers(client, mark, jack);

            await WriteUserProps(mark);
            await WriteUserProps(jack);
        }

        private static async Task WriteUserProps(IUser user)
        {
            var props = await user.GetProperties();
            Console.WriteLine($"{user.GetPrimaryKeyString()}: {props}");
        }

        public static async Task PopulateUsers(IClusterClient client, IUser mark, IUser jack)
        {
            await mark.SetName("Mark");
            await mark.SetStatus("Share your lie with me!");

            await jack.SetName("Jack");
            await jack.SetStatus("Tweet me!");

            await mark.AddFriend(jack);

            for (int i = 0; i < 100; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                await user.SetName($"User #{i}");
                await user.SetStatus(i % 3 == 0 ? "Sad" : "Happy");
                await (i % 2 == 0 ? mark : jack).AddFriend(user);
                //await WriteUserProps(user);
            }

            var tasks = new List<Task>();
            for (int i = 100; i < 200; i++)
            {
                var user = client.GetGrain<IUser>($"user{i}@outlook.com");
                tasks.Add(user.SetName($"User #{i}"));
                tasks.Add(user.SetStatus(i % 3 == 0 ? "Sad" : "Happy"));
                tasks.Add((i % 2 == 0 ? mark : jack).AddFriend(user));
                //tasks.Add(WriteUserProps(user));
            }

            await Task.WhenAll(tasks);
        }
    }
}
