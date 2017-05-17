using Interface;
using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;
using System;
using System.Linq;
using Orleans.Runtime;

namespace Grains
{
    [StorageProvider(ProviderName = "Storage")]
    public class UserGrain : Grain<UserProperties>, IUser, IRemindable
    {
        private Random rand;
        public override Task OnActivateAsync()
        {
            this.rand = new Random(this.GetHashCode());

            this.RegisterOrUpdateReminder("poke", TimeSpan.FromSeconds(this.rand.Next(60)), TimeSpan.FromSeconds(60));
            // Reminder is persistent, once we register it for some users, they will get it even if they are not activated
            // until we call UnregisterReminder("poke")

            return base.OnActivateAsync();
        }

        private async Task OnTimer(object state)
        {
            if (this.State.Friends.Count > 0)
            {
                var luckyFriend = this.State.Friends.ToList()[this.rand.Next(this.State.Friends.Count)];
                await luckyFriend.Poke(this, "I'm bored!");
            }
        }

        public async Task<bool> AddFriend(IUser friend)
        {
            var ok = await friend.InviteFriend(this);
            if (!ok)
            {
                return false;
            }

            if (!this.State.Friends.Contains(friend))
            {
                this.State.Friends.Add(friend);
            }

            await this.WriteStateAsync();

            return true;
        }

        public async Task<UserProperties> GetProperties()
        {
            if ((string)RequestContext.Get("test") == "go-poke")
            {
                await this.State.Friends.ToList()[0].Poke(this, "testing");
            }

            return this.State;
        }

        public async Task<bool> InviteFriend(IUser friend)
        {
            if (!this.State.Friends.Contains(friend))
            {
                this.State.Friends.Add(friend);
            }

            await this.WriteStateAsync();
            return true;
        }

        public Task SetName(string name)
        {
            this.State.Name = name;
            return this.WriteStateAsync();
        }

        public Task SetStatus(string status)
        {
            this.State.Status = status;
            return this.WriteStateAsync();
        }

        public Task Poke(IUser user, string message)
        {
            var test = (string)RequestContext.Get("test");
            Console.WriteLine($"User {user.GetPrimaryKeyString()} poked ${this.GetPrimaryKeyString()} with '{message}' {test}");
            return Task.CompletedTask;
        }

        public async Task ReceiveReminder(string reminderName, TickStatus status)
        {
            await this.OnTimer(null);
        }
    }
}
